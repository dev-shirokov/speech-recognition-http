
using Quartz;

namespace api.BackgroundWorkers;

public static class ServiceRegistrations
{
    public static IServiceCollection AddBackroundWorkers(this IServiceCollection services)
    {

        return services;
    }

    //private static void ConfigureQuartzJob<T>(this IServiceCollectionQuartzConfigurator quartzConfigurator, string cronExpression, bool runOnceWhenAppStarts = false) where T : IJob
    //{
    //    var jobKey = new JobKey(typeof(T).FullName!);

    //    quartzConfigurator.AddJob<T>(jobConfigurator => jobConfigurator.WithIdentity(jobKey));

    //    quartzConfigurator.AddTrigger(triggerConfigurator => triggerConfigurator
    //        .ForJob(jobKey)
    //        .WithCronSchedule(cronExpression));

    //    if (runOnceWhenAppStarts)
    //    {
    //        // Запустить джобу один раз при старте приложения
    //        quartzConfigurator.AddTrigger(triggerConfigurator => triggerConfigurator
    //            .ForJob(jobKey)
    //            .WithSimpleSchedule());
    //    }
    //}

}

//public class ThreeGppToWaveConvertJob : IJob
//{
//    public Task Execute(IJobExecutionContext context)
//    {
//        throw new NotImplementedException();
//    }
//}

