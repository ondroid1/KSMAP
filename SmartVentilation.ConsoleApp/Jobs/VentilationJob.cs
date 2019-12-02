using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Newtonsoft.Json;
using NLog;
using Quartz;
using SmartValidation.Shared;
using SmartValidation.Shared.Models;

namespace SmartVentilation.ConsoleApp.Jobs
{
    [DisallowConcurrentExecution]
    public class VentilationJob : IJob
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

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
            logger.Debug("Doing hard work!");
            _now = DateTime.Now;
            Console.WriteLine($"{_now} - ****JOB**** Ventilation Check");
            var scheduledEvents = GetCurrentEvents();
            SendVentilationCommands(scheduledEvents);
            return Task.CompletedTask;
        }

        private List<ScheduledEvent> GetCurrentEvents()
        {
            var scheduledEventsJson = File.ReadAllText(_applicationConfig.EventsFilePath);
            var currentEvents = JsonConvert.DeserializeObject<List<ScheduledEvent>>(scheduledEventsJson)
                .Where(x => x.TimeFrom.AddMinutes(- x.EventType.VentilationStartUpInMinutes) <= _now
                            && x.TimeTo.AddMinutes(x.EventType.VentilationRunOutInMinutes) >= _now).ToList();
            
            Console.WriteLine($"Currently running events:");
            foreach (var currentEvent in currentEvents)
            {
                Console.WriteLine($"{currentEvent.EventType.Code} - {currentEvent.TimeFrom}");
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
                Console.WriteLine("No commands to send. Making sure the ventilation unit is stopped.");
                return;
            }

            Console.WriteLine("Sending commands to ventilation unit.");


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

            Console.WriteLine($"Sending command for {ventilationPhase}");
        }
    }
}