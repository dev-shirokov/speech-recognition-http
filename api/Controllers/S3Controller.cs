using api.Consumers;
using api.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/storage")]
public class StorageController(ILogger<StorageController> logger, IFileService fileService, IPublishEndpoint publishEndpoint) : Controller
{
    public static int UserId = 1;

    [HttpPut("")]
    public async Task<IActionResult> Put(IFormFile file, CancellationToken token)
    {
        logger.LogInformation("PUT /");
        var record = new ThreeGppVoiceRecord(Guid.CreateVersion7(), UserId, file.Length);

        // save file
        await fileService.Save(new PutObjectModel(file.OpenReadStream(), record.ObjectName, file.ContentType), token);

        // save db
        ApplicationData.ThreeGppVoiceRecords.Add(record.ComplexId, record);

        // send data to mq
        await publishEndpoint.Publish(new VoiceRecordSaved(record.Uuid, UserId, record.ObjectName));

        return Accepted();
    }
}

public static class ApplicationData
{
    public static Dictionary<string, ThreeGppVoiceRecord> ThreeGppVoiceRecords = [];
    public static Dictionary<string, WaveVoiceRecord> WaveVoiceRecords = [];
    public static Dictionary<string, RecognizeRecord> RecognizeRecord = [];
}

public record ThreeGppVoiceRecord(Guid Uuid, int UserId, long FileLength)
{
    public string ComplexId = $"{UserId}:{Uuid}";
    public string ObjectName => $"{UserId}_{Uuid}.3gp";
}

public record WaveVoiceRecord(Guid Uuid, int UserId, long FileLength)
{
    public string ComplexId = $"{UserId}:{Uuid}";
    public string ObjectName => $"{UserId}_{Uuid}.wav";
}

public record RecognizeRecord(Guid Uuid, int UserId, string Speech)
{
    public string ComplexId = $"{UserId}:{Uuid}";
}

