using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartVentilation.Shared.Models;
using SmartVentilation.Shared.Services;
using SmartVentilation.Web.Models;

namespace SmartVentilation.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationConfig _applicationConfig;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            var json = System.IO.File.ReadAllText("configuration.json");
            _applicationConfig = JsonConvert.DeserializeObject<ApplicationConfig>(json);
        }

        public IActionResult Index(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime viewDate))
            {
                viewDate = DateTime.Now.Date;
            }

            var model = new HomeViewModel
            {
                ViewDate = viewDate
            };

            if (viewDate.Date == DateTime.Now.Date)
            {
                var json =  System.IO.File.ReadAllText(_applicationConfig.TemperatureFilePath);;
                model.Temperature = JsonConvert.DeserializeObject<int>(json);
            }
            else
            {
                model.Temperature = null;
            }

            return View(model);
        }

        public async Task<JsonResult> GetEvents(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime viewDate))
            {
                viewDate = DateTime.Now.Date;
            }

            var eventService = new EventService();
            var eventList = await eventService.GetScheduledEvents(_applicationConfig.EventsFilePath, viewDate, viewDate.AddDays(1));
            var calendarEventList = new List<CalendarEvent>();
            
            foreach (var scheduledEvent in eventList)
            {
                AddStartUpAndRunOut(calendarEventList, scheduledEvent);
                calendarEventList.Add(new CalendarEvent(scheduledEvent, VentilationPhase.MainRun));
            }

            return Json(calendarEventList);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // TODO přemísunout ven z kontroleru
        private static void AddStartUpAndRunOut(List<CalendarEvent> calendarEventList, ScheduledEvent scheduledEvent)
        {
            if (scheduledEvent.EventType.VentilationStartUpInMinutes > 0)
            {
                var startUpEvent = new ScheduledEvent
                {
                    EventType = scheduledEvent.EventType,
                    Name = $"Náběh - {scheduledEvent.Name}",
                    TimeFrom = scheduledEvent.TimeFrom.AddMinutes(-scheduledEvent.EventType
                        .VentilationStartUpInMinutes),
                    TimeTo = scheduledEvent.TimeFrom
                };
                calendarEventList.Add(new CalendarEvent(startUpEvent, VentilationPhase.StartUp));
            }

            if (scheduledEvent.EventType.VentilationRunOutInMinutes > 0)
            {
                var startUpEvent = new ScheduledEvent
                {
                    EventType = scheduledEvent.EventType,
                    Name = $"Doběh - {scheduledEvent.Name}",
                    TimeFrom = scheduledEvent.TimeTo,
                    TimeTo = scheduledEvent.TimeTo.AddMinutes(scheduledEvent.EventType
                        .VentilationRunOutInMinutes)
                };
                calendarEventList.Add(new CalendarEvent(startUpEvent, VentilationPhase.RunOut));
            }
        }
    }
}
