using api.Controllers;
using api.Services;
using MassTransit;
using Minio;
using Minio.DataModel.Args;
using System.Diagnostics;

namespace api.Consumers;

public record VoiceRecordSaved(Guid Uuid, int UserId, string ThreeGppObjectName)
{
    public override string ToString() =>  $"Uuid: {Uuid}, UserId: {UserId}, ThreeGppObjectName: {ThreeGppObjectName}";
}

public class VoiceRecordSavedConsumer(ILogger<VoiceRecordSavedConsumer> logger, IMinioClient minioClient, IPublishEndpoint publishEndpoint) : IConsumer<VoiceRecordSaved>
{
    public async Task Consume(ConsumeContext<VoiceRecordSaved> context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        var waveObjectName = $"{context.Message.UserId}_{context.Message.Uuid}.wav";

        await Convert(context.Message, waveObjectName, context.CancellationToken);

        var model = new VoiceRecordConverted(context.Message.Uuid, context.Message.UserId, waveObjectName);

        var waveRecord = new WaveVoiceRecord(context.Message.Uuid, context.Message.UserId, 0/*todo */);

        ApplicationData.WaveVoiceRecords.Add(waveRecord.ComplexId, waveRecord);

        await publishEndpoint.Publish<VoiceRecordConverted>(model, context.CancellationToken);

        stopwatch.Stop();

        logger.LogInformation($"Consume message {context.Message}. Message handled in {stopwatch.ElapsedMilliseconds} ms");
    }

    private async Task Convert(VoiceRecordSaved model, string waveFileName, CancellationToken token)
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

        if(inputStream.Length > 0)
        {
            ThreeGppToWaveFfmpegConverter processor = new();

            await processor.Convert(inputStream, outputStream);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket("tolio-app-wav")
                .WithStreamData(outputStream)
                .WithContentType("audio/x-wav")
                .WithObject(waveFileName)
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
