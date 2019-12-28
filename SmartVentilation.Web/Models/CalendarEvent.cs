using SmartVentilation.Shared.Models;
using System;

namespace SmartVentilation.Web.Models
{
    public class CalendarEvent
    {
        public CalendarEvent(ScheduledEvent scheduledEvent, VentilationPhase ventilationPhase)
        {
            // TODO
            Id = 1;
            Title = scheduledEvent.Name;
            Start = scheduledEvent.TimeFrom.ToString("s");
            End = scheduledEvent.TimeTo.ToString("s");
            AllDay = false;
            BackgroundColor = scheduledEvent.EventType.Color;
            BorderColor = "antiquewhite";
            VentilationPhase = ventilationPhase;
            ClassName = ventilationPhase == VentilationPhase.MainRun ? "" : "background-stripe";
        }

        public int Id { get; set;}                  
        public string Title { get; set;}            
        public string Start { get; set;}            
        public string End { get; set;}              
        public bool AllDay { get; set;}             
        public string BackgroundColor { get; set;}  
        public string BorderColor { get; set;}    
        public string ClassName { get; set;}      // CSS Class Name
        public VentilationPhase VentilationPhase { get; set;}      
    }
}
