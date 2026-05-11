using api.Application.Features.Exceptions;
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

public class VoiceRecordRecognizedConsumer(ILogger<VoiceRecordRecognizedConsumer> logger, ISpeechRecognitionService recognitionService, ITaskCreatingService taskCreatingService) : IConsumer<VoiceRecordRecognizedModel>
{
    public async Task Consume(ConsumeContext<VoiceRecordRecognizedModel> context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        var recognizeText = context.Message.Text;

        if (string.IsNullOrEmpty(recognizeText))
        {
            var message = $"Passed recognized speech is null or empty. {context.Message}";

            logger.LogWarning(message);

            await taskCreatingService.UpdateVoiceRecordWithError(context.Message.Uuid, context.Message.UserId, Domain.VoiceRecordSavingStatusEnum.SpeechRecognizeError, message, context.CancellationToken);

            return;
        }

        // speech to json string
        var jsonRecognizeResult = await recognitionService.ProcessAsync(recognizeText, context.CancellationToken);
        if (string.IsNullOrEmpty(jsonRecognizeResult))
        {
            var message = $"Recognized speech is null or empty. {context.Message}";

            logger.LogWarning(message);

            await taskCreatingService.UpdateVoiceRecordWithError(context.Message.Uuid, context.Message.UserId, Domain.VoiceRecordSavingStatusEnum.JsonRecognizeError, message, context.CancellationToken);

            return;
        }

        await taskCreatingService.UpdateVoiceRecordJson(context.Message.Uuid, context.Message.UserId, jsonRecognizeResult, context.CancellationToken);

        TaskCreationModel? model = default;

        // json string deserialize in json object
        try
        {
            model = JsonSerializer.Deserialize<TaskCreationModel>(jsonRecognizeResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // todo handle model is null or errorMessage is not null, throw exception and commit event
            if (model is null || !string.IsNullOrEmpty(model.ErrorMessage))
            {
                throw new LlmRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, null);
            }

            logger.LogInformation($"Voice record defined intent. {context.Message}. Defined: '{jsonRecognizeResult}'. Elapsed: {stopwatch.ElapsedMilliseconds} ms");

            model.UserId = context.Message.UserId;
            model.VoiceRecordId = context.Message.Uuid;
        }
        catch (LlmRecognizeSpeechException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new LlmRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, e);
        }

        try
        {
            await taskCreatingService.InsertTask(model, context.CancellationToken);

            logger.LogInformation($"Task created from voice record. Voice record metadata: {context.Message}. Task metadata: {model}. Elapsed: {stopwatch.ElapsedMilliseconds} ms");

        }
        catch (Exception e)
        {
            throw new SaveTaskFromVoiceRecordException(context.Message.Uuid, context.Message.UserId, e);
        }
    }
}

