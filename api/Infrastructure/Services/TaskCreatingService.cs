using api.Domain;
using api.Domain.Models;

namespace api.Infrastructure.Services;

public interface ITaskCreatingService
{
    Task InsertAsync(TaskCreationModel model, CancellationToken cancellationToken);
    Task InsertVoiceRecordAsync(Guid requestId, Guid userId, string fileName, CancellationToken cancellationToken);
    Task UpdateVoiceRecordSpeechAsync(Guid id, Guid userId, string recognizeSpeech, CancellationToken cancellationToken);
    Task UpdateVoiceRecordStatusAsync(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, CancellationToken cancellationToken);
}

public class TaskCreatingService(ILogger<TaskCreatingService> logger, MyDbContext dbContext) : ITaskCreatingService
{
    public async Task InsertAsync(TaskCreationModel model, CancellationToken cancellationToken)
    {
        TaskEntity entity = new TaskEntity
        {
            Description = model.Description,
            DueDateTime = model.DueDate,
            Title = model.Title,
            UserId = model.UserId,
            Dtc = DateTime.UtcNow,
            TaskPriority = model.Priority,
            TaskType = model.Type
        };

        await dbContext.Tasks.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertVoiceRecordAsync(Guid id, Guid userId, string fileName, CancellationToken cancellationToken)
    {
        VoiceRecordEntity entity = new VoiceRecordEntity
        {

            Id = id,
            UserId = userId,
            Path = fileName,
            Status = VoiceRecordSavingStatusEnum.ThreeGppSaved
        };

        await dbContext.VoiceRecords.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVoiceRecordStatusAsync(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new ArgumentNullException(nameof(VoiceRecordEntity));

        entity.Status = status;
        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVoiceRecordSpeechAsync(Guid id, Guid userId, string recognizeSpeech, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new ArgumentNullException(nameof(VoiceRecordEntity));

        entity.RecognizeSpeech = recognizeSpeech;
        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
