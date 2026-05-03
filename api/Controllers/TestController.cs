using api.Infrastructure.Consumers;
using api.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/test")]
[ApiController]
public class TestController(ILogger<TestController> logger,
    IFileService fileService,
    IPublishEndpoint publishEndpoint,
    IKaldiAdapter kaldiAdapter,
    ISpeechRecognitionService speechRecogntionService) : ControllerBase
{
    public static Guid UserId = Guid.CreateVersion7();


    [HttpPost("/test-llm")]
    public async Task<IActionResult> Post(string speechText, CancellationToken token)
    {
        logger.LogInformation("PUT /test-llm");
        var result = await speechRecogntionService.ProcessAsync(speechText, token);
        return Ok(result);
    }

    [HttpPut("/test-mq")]
    public async Task<IActionResult> Post(CancellationToken token)
    {
        logger.LogInformation("PUT /test-mq");
        await publishEndpoint.Publish<VoiceRecordSavedModel>(new VoiceRecordSavedModel(Guid.CreateVersion7(), UserId));
        return Accepted();
    }

    [HttpPut("/test-s3")]
    public async Task<IActionResult> Post(IFormFile file, CancellationToken token)
    {
        logger.LogInformation("PUT /test-s3");
        await fileService.Save(new PutObjectModel(file.OpenReadStream(), file.Name, file.ContentType), token);
        return Accepted();
    }

    [HttpPost("test/speech/recognize/file/bytes")]
    public async Task<IActionResult> Post1(IFormFile file, CancellationToken token)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var result = await kaldiAdapter.Recognize(ms.ToArray(), token);
        return Ok(result);
    }

    [HttpPost("test/speech/recognize/file/stream")]
    public async Task<IActionResult> Post2(IFormFile file, CancellationToken token)
    {
        using var inputStream = new MemoryStream();
        await file.CopyToAsync(inputStream);
        inputStream.Position = 0;

        var result = await kaldiAdapter.Recognize(inputStream, token);

        return Ok(result);
    }
}
