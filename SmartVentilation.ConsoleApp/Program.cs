using System;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using SmartValidation.Shared.Models;
using SmartVentilation.ConsoleApp.Config;

namespace SmartVentilation.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var applicationConfig = GetApplicationConfig();

            Job.InitializeJobs(applicationConfig);

            Console.WriteLine("Tasks started!");

            Console.ReadLine();
        }

        private static ApplicationConfig GetApplicationConfig()
        {
            string configurationFilePath = ConfigurationManager.AppSetting["ConfigurationFilePath"];

            var json = System.IO.File.ReadAllText(configurationFilePath);
            return JsonConvert.DeserializeObject<ApplicationConfig>(json);
        }
    }

    public static class Job
    {
        public static async void InitializeJobs(ApplicationConfig applicationConfig)
        {
            var scheduler = await new StdSchedulerFactory().GetScheduler();
            await scheduler.Start();

            var userEmailsJob = JobBuilder.Create<CalendarReadingJob>()
                .WithIdentity("CalendarReadingJob")
                .Build();
            var cronSchedule = $"0 0/{applicationConfig.RefreshIntervalInMinutes} * * * ?";
            Console.WriteLine($"Cron schedule: {cronSchedule}");
            var userEmailsTrigger = TriggerBuilder.Create()
                .WithIdentity("CalendarReadingJobCron")
                .StartNow()
                .WithCronSchedule(cronSchedule)
                .Build();

            var result = await scheduler.ScheduleJob(userEmailsJob, userEmailsTrigger);
        }
    }
}
