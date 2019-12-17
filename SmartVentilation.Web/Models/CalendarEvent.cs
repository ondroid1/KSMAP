using SmartVentilation.Shared.Models;

namespace SmartVentilation.Web.Models
{
    public class CalendarEvent
    {
        public CalendarEvent(ScheduledEvent scheduledEvent)
        {
            // TODO
            Id = 1;
            Title = scheduledEvent.Name;
            Start = scheduledEvent.TimeFrom.ToString("s");
            End = scheduledEvent.TimeTo.ToString("s");
            AllDay = false;
            BackgroundColor = "lightgreen";
            BorderColor = "lightgreen";
        }

        public int Id { get; set;}                  //        id = 1,
        public string Title { get; set;}            //        title = "Náběh",
        public string Start { get; set;}            //        start = DateTime.Now.AddHours(-2.5).ToString("s"),
        public string End { get; set;}              //        end = DateTime.Now.AddHours(-2).ToString("s"),
        public bool AllDay { get; set;}             //        allDay = false,
        public string BackgroundColor { get; set;}  //        backgroundColor = "lightgreen",
        public string BorderColor { get; set;}      //        borderColor = "lightgreen"

    }
}
