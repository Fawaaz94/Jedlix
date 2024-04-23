using System.Globalization;
using System.Text;
using Jedlix.Application.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Jedlix.Application.Utilities;

public static class Helpers
{
    public static DateTime StringToDateTime(this string timeString)
    {
        return DateTime.ParseExact(timeString, "HH:mm", null);
    }

    public static DateTime StringToDateTimeUtc(this string timeString)
    {
        const string format = "yyyy-MM-ddTHH:mm:ssZ";
        return DateTime
            .ParseExact(timeString, format, CultureInfo.InvariantCulture).ToUniversalTime();
    }
    
    public static async Task<InputData?> ConvertJsonToInputData(this IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var jsonContent = await reader.ReadToEndAsync();
        
        return JsonConvert.DeserializeObject<InputData>(jsonContent);
    }
    
    public static MemoryStream ConvertToJsonFile(this List<ChargingInterval> chargingSchedule)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
        {
            var json = JsonConvert.SerializeObject(chargingSchedule, Formatting.Indented);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
        }
        return stream;
    }
}