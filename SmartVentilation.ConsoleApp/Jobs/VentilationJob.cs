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

namespace SmartVentilation.ConsoleApp.Jobs
{
    [DisallowConcurrentExecution]
    public class VentilationJob : IJob
    {
        private static ILogger logger = LogManager.GetLogger("ventilationLogger");

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
            var scheduledEvents = GetCurrentEvents();
            SendVentilationCommands(scheduledEvents);
            logger.Info("Konec řízení ventilace");
            return Task.CompletedTask;
        }

        private List<ScheduledEvent> GetCurrentEvents()
        {
            var scheduledEventsJson = File.ReadAllText(_applicationConfig.EventsFilePath);
            var currentEvents = JsonConvert.DeserializeObject<List<ScheduledEvent>>(scheduledEventsJson)
                .Where(x => x.TimeFrom.AddMinutes(- x.EventType.VentilationStartUpInMinutes) <= _now
                            && x.TimeTo.AddMinutes(x.EventType.VentilationRunOutInMinutes) >= _now).ToList();
            
            logger.Info($"Právě probíhající události:");
            foreach (var currentEvent in currentEvents)
            {
                logger.Info($"{currentEvent.EventType.Code} - {currentEvent.TimeFrom}");
            }

            return currentEvents;
        }

        /// <summary>
        /// Čtení událostí z kalendáře
        /// </summary>
        public void SendVentilationCommands(List<ScheduledEvent> currentEvents)
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
                        if (currentEvent.TimeFrom.AddMinutes(- currentEvent.EventType.VentilationStartUpInMinutes) >= _now 
                            && currentEvent.TimeFrom <= _now)
                        {
                            ventilationPhase = VentilationPhase.StartUp;
                        }
                    }
                    else if (currentEvent.TimeTo.AddMinutes(- currentEvent.EventType.VentilationRunOutInMinutes) >= _now 
                             && currentEvent.TimeTo >= _now)
                    {
                        ventilationPhase = VentilationPhase.RunOut;
                    }
                }
            }

            logger.Info($"Ventilace nastavena na fázi {ventilationPhase.ToString().ToUpper()}");
        }
    }
}