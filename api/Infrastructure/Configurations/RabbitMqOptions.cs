namespace api.Infrastructure.Configurations;

public class RabbitMqOptions
{
    public static string Position => "RabbitMq";

    [ConfigurationKeyName("Endpoint")]
    public required string Endpoint { get; set; }
    [ConfigurationKeyName("Username")]
    public required string Username { get; set; }
    [ConfigurationKeyName("Password")]
    public required string Password { get; set; }

}
