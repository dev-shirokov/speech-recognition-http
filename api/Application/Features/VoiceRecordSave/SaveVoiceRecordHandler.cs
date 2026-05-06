using api.Application.Features.Exceptions;
using api.Application.Services;
using api.Infrastructure.Services;
using Requestum.Contract;
using System.Diagnostics;

namespace api.Application.Features.VoiceRecordSave;

public record SaveVoiceRecordCommand : ICommand
{
    public required Guid RequestId { get; init; }
    public required Guid UserId { get; init; }
    public required Stream FileStream { get; init; }
    public required string FileContentType { get; init; }
    public required long FileLength { get; init; }
    public string ComplexId => $"{UserId}_{RequestId}";
    public string FileName => $"{ComplexId}.3gp";
}


public class SaveVoiceRecordHandler(ILogger<SaveVoiceRecordHandler> logger, ITaskCreatingService taskCreationService, FileService fileService)
    : IAsyncCommandHandler<SaveVoiceRecordCommand>
{
    public async Task ExecuteAsync(SaveVoiceRecordCommand command, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        // 1.
        try
        {
            await fileService.Save(new PutObjectModel(command.FileStream, command.FileName, command.FileContentType), cancellationToken);
            
            logger.LogInformation($"Voice record (3gp) saved in s3 storage. FileName: {command.FileName}, FileContentType: {command.FileContentType}, FileLength: {command.FileLength}. Elapsed: {stopwatch.ElapsedMilliseconds} ms");
        }
        catch (Exception e)
        {
            throw new S3SaveThreeGppVoiceRecordException(command.FileName, command.FileContentType, command.FileLength, e);
        }

        // 2.
        try
        {
            var ms = stopwatch.ElapsedMilliseconds;
            await taskCreationService.InsertVoiceRecordAsync(command.RequestId, command.UserId, command.FileName, cancellationToken);
            
            logger.LogInformation($"Voice record (3gp) metadata saved in persist storage. FileName: {command.FileName}, FileContentType: {command.FileContentType}, FileLength: {command.FileLength}. Elapsed: {stopwatch.ElapsedMilliseconds - ms} ms");
        }
        catch (Exception e)
        {
            throw new DatabaseSaveThreeGppVoiceRecordException(command.FileName, command.FileContentType, command.FileLength, e);
        }        
    }
}

