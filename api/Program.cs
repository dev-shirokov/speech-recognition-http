using api.Consumers;
using api.Services;
using MassTransit;
using Microsoft.Extensions.Options;
using Minio;
using Quartz.Impl.AdoJobStore.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        builder.Services.Configure<S3Options>(builder.Configuration.GetSection(S3Options.Position));
        builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection(ServiceEndpointsOptions.Position));


        builder.Services.AddTransient<IFileService, FileService>();
        builder.Services.AddTransient<IKaldiAdapter, KaldiAdapter>();


        var sp = builder.Services.BuildServiceProvider();

        var s3options = sp.GetRequiredService<IOptions<S3Options>>().Value;

        builder.Services.AddMinio(configureClient =>
        {
            configureClient
                    .WithEndpoint(s3options.Endpoint)
                    .WithCredentials(s3options.AccessKey, s3options.SecretKey)
                    .WithSSL(false)
                .Build();
        });


        builder.Services.AddMassTransit(x =>
        {

            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<VoiceRecordSavedConsumer>();
            x.AddConsumer<VoiceRecordConvertedConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("root");
                    h.Password("Zud3OY=rouz85W");
                });

                cfg.ReceiveEndpoint("voice-record-saved", 
                    ep => ep.ConfigureConsumer<VoiceRecordSavedConsumer>(context));

                cfg.ReceiveEndpoint("voice-record-converted", 
                    ep => ep.ConfigureConsumer<VoiceRecordConvertedConsumer>(context));
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