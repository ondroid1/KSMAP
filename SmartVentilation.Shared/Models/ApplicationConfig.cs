using System.Collections.Generic;

namespace SmartVentilation.Shared.Models
{
    public class ApplicationConfig
    {
        public int CalendarCheckIntervalInMinutes { get; set; } = 15;
        public int VentilationCheckIntervalInMinutes { get; set; } = 15;
        public int TemperatureCheckIntervalInMinutes { get; set; } = 15;
        public string EventsFilePath { get; set; }
        public string VentilationFilePath { get; set; }
        public string TemperatureFilePath { get; set; }
        public List<ScheduledEventType> EventTypes { get; set; } = new List<ScheduledEventType>();
        public string OpenWeatherApiKey { get; set; }
        public string OpenWeatherApiPlace { get; set; }
        public int LowTemperatureMinutes { get; set; } = 0;
        public int MiddleTemperatureMinutes { get; set; } = 0;
        public int HighTemperatureMinutes { get; set; } = 0;
    }
}
