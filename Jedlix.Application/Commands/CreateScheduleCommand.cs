using Jedlix.Application.Models;
using Jedlix.Application.Services;
using Jedlix.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Jedlix.Application.Commands;

public record CreateScheduleCommand(IFormFile File) : IRequest<List<ChargingInterval>>;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, List<ChargingInterval>> 
{
    public async Task<List<ChargingInterval>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var chargingSchedule = new List<ChargingInterval>();
        var inputData = await request.File.ConvertJsonToInputData();
    
        if (inputData != null) 
            chargingSchedule = inputData.GenerateChargeSchedule();

        return chargingSchedule;
    }
}
