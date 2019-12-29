using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartVentilation.Shared.Services
{
    public class TemperatureService
    {
        public async Task<int> GetTemperatureBasedExtraMinutes()
        {
            var applicationConfig = ApplicationConfigHelper.GetApplicationConfig();

            var json = await File.ReadAllTextAsync(applicationConfig.TemperatureFilePath);
            var currentTemperature = JsonConvert.DeserializeObject<int>(json);

            return currentTemperature < 0 
                ? applicationConfig.LowTemperatureMinutes
                : currentTemperature > 20 
                    ? applicationConfig.HighTemperatureMinutes 
                    : applicationConfig.MiddleTemperatureMinutes;
        }
    }
}
