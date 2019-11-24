﻿using System.ComponentModel.DataAnnotations;

namespace SmartValidation.Shared.Models
{
    public class EventType
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string Color { get; set; }
        public int VentilationStartUpInMinutes { get; set; } = 30;
        public int VentilationRunOutInMinutes { get; set; } = 30;
    }
}
