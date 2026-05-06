using api.Application.Features.Exceptions;
using api.Application.Features.VoiceRecordSave;
using api.Application.Services;
using api.Domain.Models;
using MassTransit;
using System.Diagnostics;
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
            if (string.IsNullOrEmpty(jsonStringResult))
            {
                logger.LogWarning($"Recognized speech is null or empty. {context.Message}");
                return;
            }

            TaskCreationModel? model = default;
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                model = JsonSerializer.Deserialize<TaskCreationModel>(jsonStringResult);
                if (model is null)
                {
                    throw new LlmRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, null);
                }

                logger.LogInformation($"Voice record defined intent. {context.Message}. Defined: '{jsonStringResult}'. Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (LlmRecognizeSpeechException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LlmRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, e);
            }
        }
    }
}

