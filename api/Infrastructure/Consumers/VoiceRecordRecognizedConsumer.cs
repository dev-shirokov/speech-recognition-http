using api.Domain.Models;
using api.Features.VoiceRecordSave;
using api.Infrastructure.Services;
using MassTransit;
using System.Text.Json;

namespace api.Infrastructure.Consumers;

public record VoiceRecordRecognizedModel(Guid Uuid, Guid UserId, string Text)
{
    public override string ToString() => $"Uuid: {Uuid}, UserId: {UserId}, Text: {Text}";
};

public class VoiceRecordRecognizedConsumer(ILogger<VoiceRecordRecognizedConsumer> logger, ISpeechRecognitionService recognitionService) : IConsumer<VoiceRecordRecognizedModel>
{
    public async Task Consume(ConsumeContext<VoiceRecordRecognizedModel> context)
    {
        var recognizeText = context.Message.Text;

        if (!string.IsNullOrEmpty(recognizeText))
        {
            var jsonStringResult = await recognitionService.ProcessAsync(recognizeText, context.CancellationToken);
            if (!string.IsNullOrEmpty(jsonStringResult))
            {
                TaskCreationModel? model = default;

                try
                {
                    model = JsonSerializer.Deserialize<TaskCreationModel>(jsonStringResult);
                    if(model is null)
                    {
                        throw new LlmRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, null);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Failed to create a json object from the LLM response. Request: {recognizeText}");
                    //todo handle
                }
            }
        }
    }
}

public enum TaskTypeEnum
{
    Issue,
    Goal,
    Remind
}

public enum TaskPriorityEnum
{
    High,
    Medium,
    Low,
    Unknown
}