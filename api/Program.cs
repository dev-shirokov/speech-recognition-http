using api.Application.Services;
using api.Infrastructure.Configurations;
using api.Infrastructure.Configurations.Options;
using api.Infrastructure.Consumers;
using api.Infrastructure.Persist;
using api.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.AddOptions();        

        builder.Services.AddTransient<IFileService, FileService>();
        builder.Services.AddTransient<IAsmrAdapter, KaldiAdapter>();
        builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();


        var connection = builder.Configuration.GetConnectionString("pgsql");
        builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connection));


        var serviceProvider = builder.Services.BuildServiceProvider();
        var s3options = serviceProvider.GetRequiredService<IOptions<S3Options>>().Value;

        builder.Services.AddMinio(configureClient =>
        {
            configureClient
                    .WithEndpoint(s3options.Endpoint)
                    .WithCredentials(s3options.AccessKey, s3options.SecretKey)
                    .WithSSL(false)
                .Build();
        });

        var rabbitMqOptions = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<VoiceRecordSavedConsumer>();
            x.AddConsumer<VoiceRecordConvertedConsumer>();
            x.AddConsumer<VoiceRecordRecognizedConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Endpoint, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ReceiveEndpoint("voice-record-saved",
                    ep => ep.ConfigureConsumer<VoiceRecordSavedConsumer>(context));

                cfg.ReceiveEndpoint("voice-record-converted",
                    ep => ep.ConfigureConsumer<VoiceRecordConvertedConsumer>(context));

                cfg.ReceiveEndpoint("voice-record-recognized",
                    ep => ep.ConfigureConsumer<VoiceRecordRecognizedConsumer>(context));
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}