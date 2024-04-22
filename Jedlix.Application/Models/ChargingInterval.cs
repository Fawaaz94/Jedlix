namespace Jedlix.Application.Models;

public class ChargingInterval
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCharging { get; set; }
}