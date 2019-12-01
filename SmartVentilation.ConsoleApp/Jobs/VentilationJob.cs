using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
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
            Console.WriteLine($"{DateTime.Now} - Ventilation Check");
            var scheduledEvents = GetScheduledEvents();
            RunVentilations(scheduledEvents);
            return Task.CompletedTask;
        }

        private List<ScheduledEvent> GetScheduledEvents()
        {
            var scheduledEventsJson = File.ReadAllText(_applicationConfig.EventsFilePath);
            var scheduledEvents = JsonConvert.DeserializeObject<List<ScheduledEvent>>(scheduledEventsJson)
                .Where(x => x.TimeFrom.AddMinutes(- x.EventType.VentilationStartUpInMinutes) >= DateTime.Now 
                            && x.TimeTo.AddMinutes(x.EventType.VentilationRunOutInMinutes) <= DateTime.Now ).ToList();
            
            Console.WriteLine($"Current ventilations:");
            foreach (var scheduledEvent in scheduledEvents)
            {
                Console.WriteLine($"{scheduledEvent.EventType.Code} - {scheduledEvent.TimeFrom}");
            }

            return scheduledEvents;
        }

        /// <summary>
        /// Čtení událostí z kalendáře
        /// https://developers.google.com/calendar/quickstart/dotnet
        /// </summary>
        public void RunVentilations(List<ScheduledEvent> scheduledEvents)
        {
            Console.WriteLine("RunVentilations.");
        }
    }
}