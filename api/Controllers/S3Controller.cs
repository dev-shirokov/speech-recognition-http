using api.Consumers;
using api.Features.VoiceRecordSave;
using api.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Requestum;

namespace api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/storage")]
public class StorageController(ILogger<StorageController> logger, IRequestum requestum, IFileService fileService, IPublishEndpoint publishEndpoint) : Controller
{
    public static Guid UserId = Guid.CreateVersion7();

    [HttpPut("")]
    public async Task<IActionResult> Put(IFormFile file, CancellationToken token)
    {
        Guid requestId = Guid.CreateVersion7();

        logger.LogInformation("PUT /");

        var command = new SaveVoiceRecordCommand
        {
            RequestId = requestId,
            UserId = UserId,
            FileStream = file.OpenReadStream(),
            FileContentType = file.ContentType,
            FileLength = file.Length
        };

        await requestum.ExecuteAsync(command, token);

        try
        {
            await publishEndpoint.Publish(new VoiceRecordSavedModel(command.RequestId, command.UserId));

            logger.LogInformation($"Voice record (3gp) metadata send in queue. FileName: {command.FileName}, FileContentType: {command.FileContentType}, FileLength: {command.FileLength}");

        }
        catch (Exception e)
        {
            throw new QueueSendThreeGppVoiceRecordException(command.FileName, command.FileContentType, command.FileLength, e);
        }

        return Accepted();
    }
}

