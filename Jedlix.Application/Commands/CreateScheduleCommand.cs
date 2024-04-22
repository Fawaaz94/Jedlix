using Jedlix.Application.Models;
using Jedlix.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Jedlix.Application.Commands;

public record CreateScheduleCommand(IFormFile File) : IRequest<List<ChargingInterval>>;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, List<ChargingInterval>> 
{
    public async Task<List<ChargingInterval>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var inputData = await request.File.ConvertJsonToInputData();

        var carData = inputData.CarData;
        var userSettings = inputData.UserSettings;
        
        var chargingSchedule = new List<ChargingInterval>();
        var currentTime = inputData.StartingTime.StringToDateTimeUTC();
        var leavingTime = userSettings.LeavingTime.StringToDateTime();
        
        var desiredChargeInKwh = (userSettings.DesiredStateOfCharge / 100.0M) * carData.BatteryCapacity;
        
        var directChargingAsDecimal = userSettings.DirectChargingPercentage / 100.0M * carData.BatteryCapacity; // KWH 
        
        if (carData.CurrentBatteryLevel < directChargingAsDecimal)
        {
            var directChargingEndTime = currentTime.AddHours(
                (double)((directChargingAsDecimal - carData.CurrentBatteryLevel) /
                         carData.ChargePower));
            
           chargingSchedule.Add(new ChargingInterval
           {
               StartTime = currentTime,
               EndTime = directChargingEndTime,
               IsCharging = true
           });

           currentTime = directChargingEndTime;
        }
        
        while (currentTime < leavingTime)
        {
            var currentTariff = FindApplicableTariff(userSettings.Tariffs, currentTime);

            var isCharging = ShouldCharge(userSettings.Tariffs, currentTariff, carData.CurrentBatteryLevel, desiredChargeInKwh);
            
            var nextStartTime = currentTariff is not null ? currentTariff.EndTime.StringToDateTime() : leavingTime;
            var nextEndTime = nextStartTime;
            
            var chargingDuration = Math.Min((nextEndTime - currentTime).Hours, (desiredChargeInKwh - carData.CurrentBatteryLevel) / carData.ChargePower);
            
            carData.CurrentBatteryLevel += isCharging ? chargingDuration * carData.ChargePower : 0;
            
            chargingSchedule.Add( new ChargingInterval
            {
                StartTime = currentTime,
                EndTime = nextEndTime,
                IsCharging = isCharging
            });

            currentTime = nextEndTime;
        }

        return chargingSchedule;
    }
    
    private static bool ShouldCharge(List<Tariff> tariffs, Tariff? currentTariff, decimal currentBatteryLevel, decimal desiredStateOfCharge)
    {
        if (currentBatteryLevel >= desiredStateOfCharge)
        {
            return false;
        }

        if (tariffs.Count == 0)
            return false;
        
        tariffs = tariffs.OrderBy(t => t.EnergyPrice).ToList();

        var numTariffs = tariffs.Count;
        var isEven = numTariffs % 2 == 0;

        // Calculate how many tariffs should have IsCharge set to true
        var numToCharge = isEven ? 1 : (numTariffs + 1) / 2;

        // Check if the additional tariff would have IsCharge set to true
        var test = currentTariff?.EnergyPrice <= tariffs[numToCharge - 1].EnergyPrice;
        return currentTariff?.EnergyPrice <= tariffs[numToCharge - 1].EnergyPrice;
    }
    
    private static Tariff FindApplicableTariff(List<Tariff> tariffs, DateTime currentTime)
    {
        foreach (var tariff in tariffs)
        {
            if (currentTime >= tariff.StartTime.StringToDateTime() && currentTime < tariff.EndTime.StringToDateTime())
            {
                return tariff;
            }
        }
        return null;
    }
}
