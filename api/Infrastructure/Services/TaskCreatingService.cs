using api.Application.Services;
using api.Domain;
using api.Domain.Models;
using api.Infrastructure.Persist;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Services;

public class TaskCreatingService(ILogger<TaskCreatingService> logger, MyDbContext dbContext) : ITaskCreatingService
{

    public async Task InsertVoiceRecord(Guid id, Guid userId, string fileName, CancellationToken cancellationToken)
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

    public async Task UpdateVoiceRecordStatus(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new EntityNotFoundException(nameof(VoiceRecordEntity));

        entity.Status = status;

        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVoiceRecordSpeech(Guid id, Guid userId, string recognizeSpeech, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new EntityNotFoundException(nameof(VoiceRecordEntity));

        entity.RecognizeSpeech = recognizeSpeech;

        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVoiceRecordJson(Guid id, Guid userId, string recognizeJson, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new EntityNotFoundException(nameof(VoiceRecordEntity));

        entity.RecognizeJson = recognizeJson;
        entity.Status = VoiceRecordSavingStatusEnum.JsonRecognized;

        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVoiceRecordWithError(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, string errorMessage, CancellationToken cancellationToken)
    {
        var entity = dbContext.VoiceRecords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity is null)
            throw new EntityNotFoundException(nameof(VoiceRecordEntity));

        entity.ErrorMessage = errorMessage;
        entity.Status = status;

        dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertTask(TaskCreationModel model, CancellationToken cancellationToken)
    {
        TaskEntity entity = new()
        {
            Description = model.Description,
            DueDateTime = model.DueDate,
            Title = model.Title,
            UserId = model.UserId,
            Dtc = DateTime.UtcNow,
            TaskPriority = model.Priority ?? TaskPriorityEnum.Medium,
            TaskType = model.Type,
            VoiceRecordId = model.VoiceRecordId
        };

        await dbContext.Tasks.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
