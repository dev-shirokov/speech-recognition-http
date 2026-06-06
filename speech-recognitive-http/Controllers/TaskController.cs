using api.Application.Features.Exceptions;
using api.Application.Features.VoiceRecordSave;
using api.Application.Services;
using api.Domain;
using api.Infrastructure.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Requestum;

namespace api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/task")]
public class TaskController(ITaskCreatingService taskCreatingService, ILogger<TaskController> logger, IRequestum requestum, IPublishEndpoint publishEndpoint) : Controller
{
    //todo authorize
    public static Guid UserId = Guid.CreateVersion7();

    [HttpGet("{requestId}")]
    public async Task<IActionResult> Get(Guid requestId, CancellationToken cancellationToken, [FromServices] ITaskCreatingService creatingService)
    {
        var task = await creatingService.Get(requestId, UserId, cancellationToken);
        return Ok(task);
    }

    [HttpPut("request/{requestId}/create")]
    public async Task<IActionResult> Create(Guid requestId, IFormFile file, CancellationToken token)
    {
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

        return Accepted(new VoiceRecordAccepted(requestId));
    }

    //todo: realize etag caching 
    [HttpGet("request/{requestId}/status")]
    public async Task<IActionResult> CreationStatus(Guid requestId, CancellationToken cancellationToken)
    {
        var status = await taskCreatingService.GetCreationStatus(requestId, cancellationToken);

        var list = new List<string> {
            VoiceRecordSavingStatusEnum.SpeechRecognizeError.ToString(),
            VoiceRecordSavingStatusEnum.JsonRecognizeError.ToString(),
            VoiceRecordSavingStatusEnum.TaskSaved.ToString()
        };

        if (list.Contains(status!.Status))
        {
            status.Ended = true;
        }

        return Ok(status);
    }

    [HttpDelete("request/{requestId}/cancel")]
    public async Task<IActionResult> Cancel(Guid requestId, CancellationToken cancellationToken)
    {
        return Accepted();
    }

    [HttpGet("request/{requestId}/complete")]
    public async Task<IActionResult> Complete (Guid requestId, CancellationToken cancellationToken, [FromServices] ITaskCreatingService creatingService)
    {
        await creatingService.UpdateVoiceRecordStatus(requestId, UserId, VoiceRecordSavingStatusEnum.TaskSaved, cancellationToken);

        return Accepted();
    }
}


public record VoiceRecordAccepted(Guid RequestId);