using api.Infrastructure.Configurations.Options;

namespace api.Infrastructure.Configurations;

public static class ServiceRegistrations
{
    public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<S3Options>(builder.Configuration.GetSection(S3Options.Position));
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.Position));
        builder.Services.Configure<AsmrOptions>(builder.Configuration.GetSection(AsmrOptions.Position));
        builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection(LlmOptions.Position));

        return builder;
    }
}
