using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartVentilation.Shared.Models;

namespace SmartVentilation.Shared.Services
{
    public class EventService
    {
        public async Task<List<ScheduledEvent>> GetScheduledEvents(string eventsFilePath, DateTime fromDate, DateTime toDate)
        {
            var eventsJson = await System.IO.File.ReadAllTextAsync(eventsFilePath);
            var eventList = JsonConvert.DeserializeObject<List<ScheduledEvent>>(eventsJson);

            return eventList;
        }
    }
}
