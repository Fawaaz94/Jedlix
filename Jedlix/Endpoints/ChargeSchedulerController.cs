using System.Threading.Tasks;
using Jedlix.Application.Commands;
using Jedlix.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jedlix.Endpoints;

[ApiController]
[Route("[controller]")]
public class ChargeSchedulerController(ISender sender) : ControllerBase
{
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> CreateSchedule(IFormFile file)
    {
        
        var result = await sender.Send(new CreateScheduleCommand(file));

        // foreach (var item in result)
        // {
        //     Console.WriteLine(item);    
        // }
        //
        return Results.File(result.ConvertToJsonFile(), "application/json", "ChargingSchedule.json");
    }
}