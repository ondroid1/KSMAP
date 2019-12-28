using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SmartVentilation.Shared.Models;

namespace SmartVentilation.Web.Controllers
{
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            var json = System.IO.File.ReadAllText("configuration.json");
            var model = JsonConvert.DeserializeObject<ApplicationConfig>(json);
            ViewBag.Colors = GetColorOptions();

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(ApplicationConfig model)
        {
            // TODO - ukládání
            if (TryValidateModel(model))
            {
                var json = JsonConvert.SerializeObject(model, Formatting.Indented);
                System.IO.File.WriteAllText("configuration.json", json);
            }

            ViewBag.Colors = GetColorOptions();

            return View(model);
        }


        private List<SelectListItem> GetColorOptions()
        {
            return new[]
            {
                new SelectListItem
                {
                    Text = "Green",
                    Value = "#7bd148"
                },
                new SelectListItem
                {
                    Text = "Bold Blue",
                    Value = "#5484ed"
                },
                new SelectListItem
                {
                    Text = "Turquoise",
                    Value = "#a4bdfc"
                },
                new SelectListItem
                {
                    Text = "Light Green",
                    Value = "#7ae7bf"
                },
                new SelectListItem
                {
                    Text = "Bold Green",
                    Value = "#51b749"
                },
                new SelectListItem
                {
                    Text = "Yellow",
                    Value = "#fbd75b"
                },
                new SelectListItem
                {
                    Text = "Orange",
                    Value = "#ffb878"
                },
                new SelectListItem
                {
                    Text = "Red",
                    Value = "#ff887c"
                },
                new SelectListItem
                {
                    Text = "Dark Red",
                    Value = "#dc2127"
                },
                new SelectListItem
                {
                    Text = "Purple",
                    Value = "#dbadff"
                },
            }.ToList();
        }
    }
}