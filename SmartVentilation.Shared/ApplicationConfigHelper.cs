using Newtonsoft.Json;
using SmartVentilation.Shared.Config;
using SmartVentilation.Shared.Models;

namespace SmartVentilation.Shared
{
    public class ApplicationConfigHelper
    {
        public static ApplicationConfig GetApplicationConfig()
        {
            string configurationFilePath = ConfigurationManager.AppSetting["ConfigurationFilePath"];

            var json = System.IO.File.ReadAllText(configurationFilePath);
            return JsonConvert.DeserializeObject<ApplicationConfig>(json);
        }
    }
}
