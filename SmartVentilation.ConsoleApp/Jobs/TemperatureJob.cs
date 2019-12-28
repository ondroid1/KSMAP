using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Google.Apis.Calendar.v3;
using Newtonsoft.Json;
using NLog;
using Quartz;
using SmartVentilation.Shared;
using SmartVentilation.Shared.Models;

namespace SmartVentilation.ConsoleApp.Jobs
{
    [DisallowConcurrentExecution]
    public class TemperatureJob : IJob
    {
        private static ILogger logger = LogManager.GetLogger("temperatureLogger"); //.GetCurrentClassLogger();

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        private static readonly string[] Scopes = {CalendarService.Scope.CalendarReadonly};
        private readonly ApplicationConfig _applicationConfig;

        public TemperatureJob()
        {
            // TODO DI
            _applicationConfig = ApplicationConfigHelper.GetApplicationConfig();
        }

        /// <summary>
        /// Spuštění výkonné části jobu
        /// </summary>
        public Task Execute(IJobExecutionContext context)
        {
            logger.Info("ZAČÁTEK čtení teploty");
            var temperature = GetCurrentTemperature();
            logger.Info($"Aktuální teplota v místě {_applicationConfig.OpenWeatherApiPlace} je {temperature}°C");
            File.WriteAllText(_applicationConfig.TemperatureFilePath, JsonConvert.SerializeObject(temperature));
            logger.Info("KONEC čtení teploty");
            return Task.CompletedTask;
        }

        private int GetCurrentTemperature()
        {
            using WebClient client = new WebClient();
            var apiUrl =
                $"http://api.openweathermap.org/data/2.5/weather?q={_applicationConfig.OpenWeatherApiPlace}&mode=xml&units=metric&APPID={_applicationConfig.OpenWeatherApiKey}";
            string xmlContent = client.DownloadString(apiUrl);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);

            return GetTemperature(xmlDocument);
        }

        public int GetTemperature(XmlDocument xmlDocument)
        {
            var temperatureNode = xmlDocument.SelectSingleNode("//temperature");
            var temperatureValue = temperatureNode.Attributes["value"].Value;
            return (int) Math.Round(Convert.ToDecimal(temperatureValue, new CultureInfo("en-US")));
        }
    }
}