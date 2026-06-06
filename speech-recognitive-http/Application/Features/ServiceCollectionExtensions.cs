namespace api.Application.Features;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddRequestum(cfg =>
        {
            // Сканирование сборки - найдёт все обработчики и middleware
            cfg.Default(typeof(Program).Assembly);

            // Настройка времени жизни
            cfg.Lifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
