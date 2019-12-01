using System.Collections.Generic;

namespace SmartValidation.Shared.Models
{
    public class ApplicationConfig
    {
        public int CalendarCheckIntervalInMinutes { get; set; } = 15;
        public int VentilationCheckIntervalInMinutes { get; set; } = 15;
        public string EventsFilePath { get; set; }
        public List<ScheduledEventType> EventTypes { get; set; } = new List<ScheduledEventType>();
    }
}
