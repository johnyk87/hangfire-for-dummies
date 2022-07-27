using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HangfireForDummies;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services
    .AddLogging(options =>
    {
        options.AddSimpleConsole(console =>
        {
            console.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
        });
    })
    .AddHangfire(
        configuration => configuration
            .UseMongoStorage(
                "mongodb://localhost:27017/HANGFIRE",
                new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        BackupStrategy = new NoneMongoBackupStrategy(),
                        MigrationStrategy = new DropMongoMigrationStrategy()
                    },
                    CheckConnection = false,
                    CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
                    InvisibilityTimeout = TimeSpan.FromSeconds(120), // TODO: play with me!
                }))
    .AddHangfireServer(
        options =>
        {
            options.WorkerCount = 2; // TODO: play with me!
        });

var app = builder.Build();

// Setup middleware pipeline
app.UseHangfireDashboard();

// Register Hangfire jobs
RecurringJob.AddOrUpdate<TimeoutJob>(
    nameof(TimeoutJob),
    x => x.ExecuteAsync(null! ,CancellationToken.None),
    Cron.Never);

RecurringJob.AddOrUpdate<OtherTimeoutJob>(
    nameof(OtherTimeoutJob),
    x => x.ExecuteAsync(null!, CancellationToken.None),
    Cron.Never);

app.Run();
