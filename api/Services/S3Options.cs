namespace api.Services;

public class S3Options
{
    public static string Position => "S3";

    [ConfigurationKeyName("Endpoint")]
    public string Endpoint { get; set; } = String.Empty;
    [ConfigurationKeyName("Accesskey")]
    public string AccessKey { get; set; } = String.Empty;
    [ConfigurationKeyName("SecretKey")]
    public string SecretKey { get; set; } = String.Empty;
    [ConfigurationKeyName("BucketName")]
    public string BucketName { get; set; } = String.Empty;
}
