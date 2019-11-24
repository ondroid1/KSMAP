﻿using System.Collections.Generic;

namespace SmartValidation.Shared.Models
{
    public class ApplicationConfig
    {
        public int RefreshIntervalInMinutes { get; set; } = 15;
        public List<EventType> EventTypes { get; set; } = new List<EventType>();
    }
}