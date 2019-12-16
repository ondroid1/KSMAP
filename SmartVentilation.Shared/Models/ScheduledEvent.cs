using System;

namespace SmartVentilation.Shared.Models
{
    public class ScheduledEvent
    {
        public ScheduledEventType EventType { get; set; }
        
        public string Name { get; set; }

        public DateTime TimeFrom { get; set; }

        public DateTime TimeTo { get; set; }
    }
}
