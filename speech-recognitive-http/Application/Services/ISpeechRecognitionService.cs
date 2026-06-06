namespace api.Application.Services;

public interface ISpeechRecognitionService
{
    Task<string> ProcessAsync(string speechText, CancellationToken token);
}
