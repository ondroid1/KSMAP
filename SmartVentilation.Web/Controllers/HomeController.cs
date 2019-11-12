using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartVentilation.Web.Models;

namespace SmartVentilation.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetEvents(double start, double end)
        {
            //var userName = Session["UserName"] as string;
            //if (string.IsNullOrEmpty(userName))
            //{
            //    return null;
            //}
 
            var fromDate = ConvertFromUnixTimestamp(start);
            var toDate = ConvertFromUnixTimestamp(end);
 
            //var rep = Resolver.Resolve<IEventRepository>();
            //var events = rep.ListEventsForUser(userName, fromDate, toDate);
 
            var eventList = new object[]
            {
                new {
                    id = 1,
                    title = "Test",
                    start = DateTime.Now.AddHours(-2).ToString("s"),
                    end = DateTime.Now.ToString("s"),
                    allDay = false
                }
            };
                
 
            //var rows = eventList.ToArray();
            return Json(eventList);
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
