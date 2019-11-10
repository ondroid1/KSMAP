using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace SmartVentilation.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Job.InitializeJobs();

            Console.WriteLine("Tasks started!");

            Console.ReadLine();
        }
    }

    public static class Job
    {
        public static async void InitializeJobs()
        {
            var scheduler = await new StdSchedulerFactory().GetScheduler();
            await scheduler.Start();

            var userEmailsJob = JobBuilder.Create<CalendarReadingJob>()
                .WithIdentity("CalendarReadingJob")
                .Build();
            var userEmailsTrigger = TriggerBuilder.Create()
                .WithIdentity("CalendarReadingJobCron")
                .StartNow()
                .WithCronSchedule("* * * ? * *")
                .Build();

            var result = await scheduler.ScheduleJob(userEmailsJob, userEmailsTrigger);
        }
    }
}
