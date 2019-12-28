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
using SmartVentilation.Shared;
using SmartVentilation.Shared.Models;

namespace SmartVentilation.ConsoleApp.Jobs
{
    [DisallowConcurrentExecution]
    public class CalendarReadingJob : IJob
    {
        private static readonly ILogger logger = LogManager.GetLogger("reservationLogger");
        private readonly ApplicationConfig _applicationConfig;
        private const string ApplicationName = "Smart Ventilation";
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        private static readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly };

        public CalendarReadingJob()
        {
            // TODO DI
            _applicationConfig = ApplicationConfigHelper.GetApplicationConfig();
        }

        /// <summary>
        /// Spuštění výkonné části jobu
        /// </summary>
        public Task Execute(IJobExecutionContext context)
        {
            logger.Info("ZAČÁTEK čtení z rezervačního systému");
            ReadFutureCalendarItems();
            logger.Info("KONEC čtení z rezervačního systému");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Čtení událostí z kalendáře
        /// https://developers.google.com/calendar/quickstart/dotnet
        /// </summary>
        public void ReadFutureCalendarItems()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                var credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                //Console.WriteLine($"Credential file saved to: {credPath}");
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("nek71fi8o6nrkukvusvdbv90i8@group.calendar.google.com");
            request.TimeMin = DateTime.Now.Date;
            request.TimeMax = DateTime.Now.Date.AddDays(8);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            logger.Info("Načtené události");
            if (events.Items != null && events.Items.Count > 0)
            {
                var scheduledEvents = new List<ScheduledEvent>();

                foreach (var eventItem in events.Items)
                {
                    if (eventItem.Start.DateTime == null || eventItem.End.DateTime == null)
                    {
                        continue;
                    }

                    scheduledEvents.Add(new ScheduledEvent
                    {
                        Name = GetEventName(eventItem.Summary),
                        TimeFrom = (DateTime) eventItem.Start.DateTime,
                        TimeTo = (DateTime) eventItem.End.DateTime,
                        EventType = GetEventType(eventItem.Summary)
                    });

                    logger.Info($"{eventItem.Summary} ({eventItem.Start.DateTime.ToString()})");
                }

                File.WriteAllText(_applicationConfig.EventsFilePath, JsonConvert.SerializeObject(scheduledEvents, Formatting.Indented), Encoding.UTF8);
            }
            else
            {
                logger.Info("V rezervačním systému nebyly nalezeny žádné budoucí události");
            }
        }

        private ScheduledEventType GetEventType(string eventItemSummary)
        {
            var eventTypeCode = eventItemSummary.Split(" - ")[0];

            return _applicationConfig.EventTypes.SingleOrDefault(x => x.Code == eventTypeCode)
                ?? _applicationConfig.EventTypes.Single(x => x.IsDefault);
        }

        private string GetEventName(string eventItemSummary)
        {
            return eventItemSummary.Split(" - ")[1];
        }
    }
}