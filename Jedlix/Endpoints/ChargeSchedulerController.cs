using Jedlix.Application.Commands;
using Jedlix.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jedlix.Endpoints;

[ApiController]
[Route("[controller]/generate-charge-schedule")]
public class ChargeSchedulerController(ISender sender) : ControllerBase
{
    [HttpPost("file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> CreateScheduleFile(IFormFile file)
    {
        var result = await sender.Send(new CreateScheduleCommand(file));
        return Results.File(result.ConvertToJsonFile(), "application/json", "ChargingSchedule.json");
    }
    
    [HttpPost("json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> CreateScheduleJson(IFormFile file)
    {
        var result = await sender.Send(new CreateScheduleCommand(file));
        return Results.Json(result);
    }
}