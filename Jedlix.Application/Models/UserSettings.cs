namespace Jedlix.Application.Models;

public class UserSettings
{
    public required int DesiredStateOfCharge { get; set; }
    public required string LeavingTime { get; set; }
    public required int DirectChargingPercentage { get; set; }
    public required List<Tariff> Tariffs { get; set; }
}