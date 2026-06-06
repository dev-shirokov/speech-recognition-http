namespace api.Infrastructure.Configurations.Options;

public class LlmOptions
{
    public static string Position => "LLM";

    [ConfigurationKeyName("Endpoint")]
    public required string Endpoint { get; set; }

    [ConfigurationKeyName("ModelName")]
    public required string ModelName { get; set; }

}