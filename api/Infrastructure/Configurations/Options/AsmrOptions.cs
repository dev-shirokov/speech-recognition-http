namespace api.Infrastructure.Configurations;

public class AsmrOptions
{
    public static string Position => "ASMR";

    [ConfigurationKeyName("Endpoint")]
    public required string Endpoint { get; set; }

}
