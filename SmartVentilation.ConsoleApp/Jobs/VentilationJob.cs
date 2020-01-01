using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Newtonsoft.Json;
using NLog;
using Quartz;
using SmartVentilation.Shared;
using SmartVentilation.Shared.Models;
using SmartVentilation.Shared.Services;

namespace SmartVentilation.ConsoleApp.Jobs
{
    [DisallowConcurrentExecution]
    public class VentilationJob : IJob
    {
        private static readonly ILogger logger = LogManager.GetLogger("ventilationLogger");

        private const string ApplicationName = "Smart Ventilation";
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        private static readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        private readonly ApplicationConfig _applicationConfig;
        private DateTime _now;


        public VentilationJob()
        {
            // TODO DI
            _applicationConfig = ApplicationConfigHelper.GetApplicationConfig();
        }

        /// <summary>
        /// Spuštění výkonné části jobu
        /// </summary>
        public Task Execute(IJobExecutionContext context)
        {
            logger.Info($"Začátek řízení ventilace");

            var temperatureBasedExtraMinutes = GetTemperatureBasedExtraMinutes().Result;
            var scheduledEvents = GetCurrentEvents(temperatureBasedExtraMinutes);
            SendVentilationCommands(scheduledEvents, temperatureBasedExtraMinutes);
            logger.Info("Konec řízení ventilace");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Čtení událostí z kalendáře
        /// </summary>
        public void SendVentilationCommands(List<ScheduledEvent> currentEvents, int temperatureBasedExtraMinutes)
        {
            if (currentEvents.Count == 0)
            {
                logger.Info("Ventilace nastavena na fázi STOP");
                return;
            }

            var ventilationPhase = VentilationPhase.StartUp;

            foreach (var currentEvent in currentEvents)
            {
                if (ventilationPhase != VentilationPhase.MainRun)
                {
                    if (currentEvent.TimeFrom <= _now && currentEvent.TimeTo >= _now)
                    {
                        ventilationPhase = VentilationPhase.MainRun;
                    }
                    else if (ventilationPhase != VentilationPhase.StartUp)
                    {
                        if (currentEvent.TimeFrom
                                .AddMinutes(- currentEvent.EventType.VentilationStartUpInMinutes - temperatureBasedExtraMinutes) >= _now 
                            && currentEvent.TimeFrom <= _now)
                        {
                            ventilationPhase = VentilationPhase.StartUp;
                        }
                    }
                    else if (currentEvent.TimeTo
                                 .AddMinutes(- currentEvent.EventType.VentilationRunOutInMinutes + temperatureBasedExtraMinutes) >= _now 
                             && currentEvent.TimeTo >= _now)
                    {
                        ventilationPhase = VentilationPhase.RunOut;
                    }
                }
            }

            logger.Info($"Ventilace nastavena na fázi {ventilationPhase.ToString().ToUpper()}");
        }

        private List<ScheduledEvent> GetCurrentEvents(int temperatureBasedExtraMinutes)
        {
            try
            {
                var scheduledEventsJson = File.ReadAllText(_applicationConfig.EventsFilePath);
                var currentEvents = JsonConvert.DeserializeObject<List<ScheduledEvent>>(scheduledEventsJson)
                    .Where(x => x.TimeFrom.AddMinutes(- x.EventType.VentilationStartUpInMinutes - temperatureBasedExtraMinutes) <= _now
                                && x.TimeTo.AddMinutes(x.EventType.VentilationRunOutInMinutes + temperatureBasedExtraMinutes) >= _now).ToList();
            
                logger.Info($"Právě probíhající události:");
                foreach (var currentEvent in currentEvents)
                {
                    logger.Info($"{currentEvent.EventType.Code} - {currentEvent.TimeFrom}");
                }

                return currentEvents;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return new List<ScheduledEvent>();
            }
        }

        private async Task<int> GetTemperatureBasedExtraMinutes()
        {
            var temperatureService = new TemperatureService();

            return await temperatureService.GetTemperatureBasedExtraMinutes();
        }
    }
}