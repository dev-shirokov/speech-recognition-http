using api.Controllers;
using api.Features.VoiceRecordSave;
using api.Infrastructure.Services;
using MassTransit;
using Minio;
using Minio.DataModel.Args;
using System.Diagnostics;

namespace api.Infrastructure.Consumers;

public record VoiceRecordSavedModel(Guid RequestId, Guid UserId)
{
    public string ThreeGppObjectName => $"{UserId}_{RequestId}.3gp";
    public string WaveObjectName => $"{UserId}_{RequestId}.wav";
    public override string ToString() => $"Uuid: {RequestId}, UserId: {UserId}";
}

public class VoiceRecordSavedConsumer(ILogger<VoiceRecordSavedConsumer> logger, IMinioClient minioClient, ITaskCreatingService taskCreatingService, IPublishEndpoint publishEndpoint) : IConsumer<VoiceRecordSavedModel>
{
    public async Task Consume(ConsumeContext<VoiceRecordSavedModel> context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            await ConvertAndSaveInMediaStorage(context.Message, context.CancellationToken);
        }
        catch (Exception e)
        {
            throw new ConvertingToWaveVoiceRecordException(context.Message.WaveObjectName, e);
        }

        try
        {
            await SaveMetadata(context.Message.RequestId, context.Message.UserId, context.CancellationToken);
        }
        catch (Exception e)
        {
            throw new SavingMetadataOfWaveVoiceRecordException(context.Message.WaveObjectName, e);
        }

        try
        {
            var model = new VoiceRecordConvertedModel(context.Message.RequestId, context.Message.UserId);
            await publishEndpoint.Publish(model, context.CancellationToken);

            logger.LogInformation($"Send message to queue 'voice-record-converted'. {context.Message}");
        }
        catch (Exception e)
        {
            throw new QueueSendWaveVoiceRecordException(context.Message.WaveObjectName, e);
        }

        stopwatch.Stop();
    }

    private Task SaveMetadata(Guid requestId, Guid userId, CancellationToken token)
    {
        return taskCreatingService.UpdateVoiceRecordStatusAsync(requestId, userId, Domain.VoiceRecordSavingStatusEnum.WaveSaved, token);
    }

    private async Task ConvertAndSaveInMediaStorage(VoiceRecordSavedModel model, CancellationToken token)
    {
        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket("tolio-app-3gpp")
            .WithObject(model.ThreeGppObjectName)
            .WithCallbackStream(callbackStream => callbackStream.CopyTo(inputStream));

        var getResponse = await minioClient
            .GetObjectAsync(getObjectArgs, token);

        inputStream.Position = 0;

        if (inputStream.Length > 0)
        {
            ThreeGppToWaveFfmpegConverter processor = new();

            await processor.Convert(inputStream, outputStream);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket("tolio-app-wav")
                .WithStreamData(outputStream)
                .WithContentType("audio/x-wav")
                .WithObject(model.WaveObjectName)
                .WithObjectSize(outputStream.Length);

            var putResponse = await minioClient.PutObjectAsync(putObjectArgs, token);
        }
        else
        {
            logger.LogError($"Object '{model.ThreeGppObjectName}' is empty. Convert to wav is failed");
        }

        inputStream.Close();
        inputStream.Dispose();

        outputStream.Close();
        outputStream.Dispose();
    }
}
