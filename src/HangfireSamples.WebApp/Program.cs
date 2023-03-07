using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using HangfireSamples.Jobs;

namespace HangfireSamples;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;
        var configuration = builder.Configuration;
        var env = builder.Environment;

        services.AddScoped<IMyRecurringJob, MyRecurringJob>();
        services.AddRazorPages();

        services.AddHangfire(hangfire =>
       {
           hangfire.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
           hangfire.UseSimpleAssemblyNameTypeSerializer();
           hangfire.UseRecommendedSerializerSettings();
           hangfire.UseColouredConsoleLogProvider();
           hangfire.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"),
               new SqlServerStorageOptions
               {
                   CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                   SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                   QueuePollInterval = TimeSpan.Zero,
                   UseRecommendedIsolationLevel = true,
                   DisableGlobalLocks = true
               });

           var server = new BackgroundJobServer(new BackgroundJobServerOptions
           {
               ServerName = "hangfire-samples",
           });
       });

        var app = builder.Build();

        app.UseHangfireDashboard();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();

        app.MapPost("/recurring-job", () =>
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    BackgroundJob.Delete(recurringJob.Id);
                }
            }

            RecurringJob.AddOrUpdate<IMyRecurringJob>(job => job.DoSomethingReentrant(), Cron.Minutely);
        })
        .WithName("recurring-job");

        app.Run();
    }
}
