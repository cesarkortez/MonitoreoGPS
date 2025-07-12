using System.Net.Http;
using System.Text;
using System.Text.Json;

var http = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
var rnd = new Random();
string[] vehicleIds = { "V1", "V2", "V3", "V4", "V5" };

while (true)
{
    foreach (var id in vehicleIds)
    {
        var coord = new
        {
            VehicleId = id,
            Latitude = rnd.NextDouble() * 180 - 90,
            Longitude = rnd.NextDouble() * 360 - 180,
            Timestamp = DateTime.UtcNow
        };
        // 15% fallo
        if (rnd.NextDouble() < 0.15) continue;
        var content = new StringContent(JsonSerializer.Serialize(coord), Encoding.UTF8, "application/json");
        await http.PostAsync("/api/geolocation", content);
    }
    await Task.Delay(2000);
}
