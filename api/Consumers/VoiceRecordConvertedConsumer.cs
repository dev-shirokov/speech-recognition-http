using api.Controllers;
using api.Services;
using MassTransit;
using Minio;
using Minio.DataModel.Args;
using System.Diagnostics;

namespace api.Consumers;

public record VoiceRecordConverted(Guid Uuid, int UserId, string WaveObjectName)
{
    public override string ToString() => $"Uuid: {Uuid}, UserId: {UserId}, WavePath: {WaveObjectName}";
};
class VoiceRecordConvertedConsumer(ILogger<VoiceRecordConvertedConsumer> logger, IMinioClient minioClient, IKaldiAdapter kaldiAdapter) : IConsumer<VoiceRecordConverted>
{
    public async Task Consume(ConsumeContext<VoiceRecordConverted> context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        var inputStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket("tolio-app-wav")
            .WithObject(context.Message.WaveObjectName)
            .WithCallbackStream(async stream => await stream.CopyToAsync(inputStream));

        var getResponse = await minioClient.GetObjectAsync(args, context.CancellationToken);

        inputStream.Position = 0;

        var recognizeResult = await kaldiAdapter.Recognize(inputStream, context.CancellationToken);

        var recognizeRecord = new RecognizeRecord(context.Message.Uuid, context.Message.UserId, recognizeResult?.Text!);
        ApplicationData.RecognizeRecord.Add(recognizeRecord.ComplexId, recognizeRecord);

        inputStream.Close();
        inputStream.Dispose();

        stopwatch.Stop();

        logger.LogInformation($"Consume message {context.Message}. Result: {recognizeResult!.Text} Message handled in {stopwatch.ElapsedMilliseconds} ms");
    }
}
