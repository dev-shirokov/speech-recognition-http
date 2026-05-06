using api.Application.Features.Exceptions;
using api.Application.Services;
using MassTransit;
using Minio;
using Minio.DataModel.Args;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace api.Infrastructure.Consumers;

public record VoiceRecordConvertedModel(Guid Uuid, Guid UserId)
{
    public string WaveObjectName => $"{UserId}_{Uuid}.wav";
    public override string ToString() => $"Uuid: {Uuid}, UserId: {UserId}";
};

class VoiceRecordConvertedConsumer(ILogger<VoiceRecordConvertedConsumer> logger,
    IPublishEndpoint publishEndpoint,
    IMinioClient minioClient,
    IAsmrAdapter kaldiAdapter,
    ITaskCreatingService taskCreatingService) : IConsumer<VoiceRecordConvertedModel>
{
    public async Task Consume(ConsumeContext<VoiceRecordConvertedModel> context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        string? recognizeSpeech = default;

        try
        {
            recognizeSpeech = await RecognizeVoiceRecord(context.Message.WaveObjectName, context.CancellationToken); 
            
            logger.LogInformation($"Voice record recognized. {context.Message}. Recognized: '{recognizeSpeech}'. Elapsed: {stopwatch.ElapsedMilliseconds} ms");

        }
        catch (Exception e)
        {
            throw new KaldiRecognizeVoiceRecordException(context.Message.WaveObjectName, e);
        }

        if (recognizeSpeech != null)
        {
            try
            {
                await taskCreatingService.UpdateVoiceRecordSpeechAsync(context.Message.Uuid, context.Message.UserId, recognizeSpeech, context.CancellationToken);

                logger.LogInformation($"Voice record metadata saved in persist storage. {context.Message}. Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                throw new SaveRecognizeSpeechException(context.Message.WaveObjectName, e);
            }

            try
            {
                await publishEndpoint.Publish(new VoiceRecordRecognizedModel(context.Message.Uuid, context.Message.UserId, recognizeSpeech));

                logger.LogInformation($"Recognize speech send in queue 'voice-record-recognized'. {context.Message}");

            }
            catch (Exception e)
            {
                throw new QueueSendRecognizeSpeechException(context.Message.Uuid, context.Message.UserId, e);
            }

        }

        stopwatch.Stop();
    }

    private async Task<string?> RecognizeVoiceRecord(string waveObjectName, CancellationToken token)
    {
        var inputStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket("tolio-app-wav")
            .WithObject(waveObjectName)
            .WithCallbackStream(async stream => await stream.CopyToAsync(inputStream));

        var getResponse = await minioClient.GetObjectAsync(args, token);

        inputStream.Position = 0;

        var recognizeResult = await kaldiAdapter.Recognize(inputStream, token);

        inputStream.Close();
        inputStream.Dispose();

        return recognizeResult?.Text;
    }
}
