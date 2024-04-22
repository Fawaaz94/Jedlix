using Jedlix.Application.Models;
using Jedlix.Application.Utilities;

namespace Jedlix.Application.Services;

public static class ChargeSchedule
{
    public static List<ChargingInterval> GenerateChargeSchedule(this InputData inputData)
    {
        var chargingSchedule = new List<ChargingInterval>();
        
        var carData = inputData.CarData;
        var userSettings = inputData.UserSettings;
        
        var currentTime = inputData.StartingTime.StringToDateTimeUTC();
        var leavingTime = userSettings.LeavingTime.StringToDateTime();
        
        var desiredChargeInKwh = (userSettings.DesiredStateOfCharge / 100.0M) * carData.BatteryCapacity;
        var directChargingAsDecimal = userSettings.DirectChargingPercentage / 100.0M * carData.BatteryCapacity; 
        
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
            // Check if tariff is in range
            var (applicableTariff, isApplicable) = FindApplicableTariff(userSettings.Tariffs, currentTime);
            
            if (!isApplicable) continue;
            
            var isCharging = ShouldCharge(userSettings.Tariffs, applicableTariff, carData.CurrentBatteryLevel, desiredChargeInKwh);
            
            var nextStartTime = applicableTariff.EndTime.StringToDateTime();
            
            var chargingDuration = (nextStartTime - currentTime).Hours;
            
            carData.CurrentBatteryLevel += isCharging ? 
                AddChargeWithCapacityLimit(
                    chargingDuration,
                    carData.CurrentBatteryLevel,
                    carData.BatteryCapacity,
                    carData.ChargePower) : 0;
                
            chargingSchedule.Add( new ChargingInterval
            {
                StartTime = currentTime,
                EndTime = nextStartTime,
                IsCharging = isCharging
            });

            currentTime = nextStartTime;
        }

        return chargingSchedule;
    }
    
    private static bool ShouldCharge(List<Tariff> tariffs, Tariff? currentTariff, decimal currentBatteryLevel, decimal desiredStateOfCharge)
    {
        var tariffsCount = tariffs.Count;
        
        if (currentBatteryLevel >= desiredStateOfCharge)
            return false;

        if (tariffsCount == 1)
            return true;
        
        tariffs = tariffs.OrderBy(t => t.EnergyPrice).ToList();
        
        var isEven = tariffsCount % 2 == 0;

        // Calculate how many tariffs should have IsCharge set to true
        var numToCharge = isEven ? 1 : (tariffsCount + 1) / 2;

        // Check if the additional tariff would have IsCharge set to true
        return currentTariff?.EnergyPrice <= tariffs[numToCharge - 1].EnergyPrice;
    }

    private static (Tariff applicableTariff, bool isApplicable) FindApplicableTariff(List<Tariff> tariffs, DateTime currentTime)
    {
        foreach (var tariff in tariffs)
        {
            if (currentTime >= tariff.StartTime.StringToDateTime() && currentTime < tariff.EndTime.StringToDateTime())
            {
                return (tariff, true);
            }
        }

        return (new Tariff(), false);
    }

    private static decimal AddChargeWithCapacityLimit(int chargeDuration, decimal currentBatterLevel, decimal batteryCapacity, decimal chargePower)
    {
        var remainingCapacity = batteryCapacity - currentBatterLevel;
        var chargeToAdd = chargeDuration * chargePower;

        if (chargeToAdd > remainingCapacity)
            chargeToAdd = remainingCapacity;

        return currentBatterLevel + chargeToAdd;
    }
}