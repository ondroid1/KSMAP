using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartValidation.Shared.Config;
using SmartValidation.Shared.Models;

namespace SmartValidation.Shared
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
