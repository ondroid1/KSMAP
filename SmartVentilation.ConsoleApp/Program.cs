using System;
using System.Configuration;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using SmartValidation.Shared;
using SmartValidation.Shared.Models;
using SmartVentilation.ConsoleApp.Jobs;

namespace SmartVentilation.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var applicationConfig = ApplicationConfigHelper.GetApplicationConfig();

            Job.InitializeJobs(applicationConfig);

            Console.WriteLine("Quartz jobs started!");

            Console.ReadLine();
        }
    }

    public static class Job
    {
        public static async void InitializeJobs(ApplicationConfig applicationConfig)
        {
            InitializeCalendarReadingJob(applicationConfig);
            InitializeVentilationJob(applicationConfig);
        }

        public static async void InitializeCalendarReadingJob(ApplicationConfig applicationConfig)
        {
            var calendarReadingJob = JobBuilder.Create<CalendarReadingJob>()
                .WithIdentity("CalendarReadingJob")
                .Build();
            
            var cronSchedule = $"0 0/{applicationConfig.CalendarCheckIntervalInMinutes} * * * ?";

            Console.WriteLine($"Running calendar check every {applicationConfig.CalendarCheckIntervalInMinutes} minute(s).");
            
            var calendarReadingJobTrigger = TriggerBuilder.Create()
                .WithIdentity("CalendarReadingJobCron")
                .StartNow()
                .WithCronSchedule(cronSchedule)
                .Build();

            var scheduler = await new StdSchedulerFactory().GetScheduler();
            
            await scheduler.Start();

            var result = await scheduler.ScheduleJob(calendarReadingJob, calendarReadingJobTrigger);
        }

        public static async void InitializeVentilationJob(ApplicationConfig applicationConfig)
        {
            var ventilationJob = JobBuilder.Create<VentilationJob>()
                .WithIdentity("VentilationJob")
                .Build();
            
            var cronSchedule = $"0 0/{applicationConfig.VentilationCheckIntervalInMinutes} * * * ?";

            Console.WriteLine($"Running ventilation check every {applicationConfig.VentilationCheckIntervalInMinutes} minute(s).");
            
            var ventilationJobTrigger = TriggerBuilder.Create()
                .WithIdentity("VentilationJobCron")
                .StartNow()
                .WithCronSchedule(cronSchedule)
                .Build();

            var scheduler = await new StdSchedulerFactory().GetScheduler();
            
            await scheduler.Start();

            var result = await scheduler.ScheduleJob(ventilationJob, ventilationJobTrigger);
        }
    }
}