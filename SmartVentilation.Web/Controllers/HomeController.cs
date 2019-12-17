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
                calendarEventList.Add(new CalendarEvent(scheduledEvent));
            }

            var eventList2 = new object[]
            {
                new {
                    id = 1,
                    title = "Náběh",
                    start = DateTime.Now.AddHours(-2.5).ToString("s"),
                    end = DateTime.Now.AddHours(-2).ToString("s"),
                    allDay = false,
                    backgroundColor = "lightgreen",
                    borderColor = "lightgreen"
                },
                new {
                    id = 1,
                    title = "Ventilace 100%",
                    start = DateTime.Now.AddHours(-2).ToString("s"),
                    end = DateTime.Now.ToString("s"),
                    allDay = false,
                    backgroundColor = "#378006",
                    borderColor = "#378006",
                    textColor = "white"
                }
            };

            return Json(calendarEventList);
            //return Json(eventList2);
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}
