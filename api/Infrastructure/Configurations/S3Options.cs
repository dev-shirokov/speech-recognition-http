namespace api.Infrastructure.Configurations;

public class S3Options
{
    public static string Position => "S3";

    [ConfigurationKeyName("Endpoint")]
    public required string Endpoint { get; set; }
    [ConfigurationKeyName("Accesskey")]
    public required string AccessKey { get; set; }
    [ConfigurationKeyName("SecretKey")]
    public required string SecretKey { get; set; }
    [ConfigurationKeyName("BucketName")]
    public required string BucketName { get; set; }
}
