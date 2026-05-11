using api.Domain;
using api.Domain.Models;

namespace api.Application.Services;

public interface ITaskCreatingService
{
    Task InsertTask(TaskCreationModel model, CancellationToken cancellationToken);
    Task InsertVoiceRecord(Guid requestId, Guid userId, string fileName, CancellationToken cancellationToken);
    Task UpdateVoiceRecordJson(Guid id, Guid userId, string recognizeJson, CancellationToken cancellationToken);
    Task UpdateVoiceRecordSpeech(Guid id, Guid userId, string recognizeSpeech, CancellationToken cancellationToken);
    Task UpdateVoiceRecordStatus(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, CancellationToken cancellationToken);
    Task UpdateVoiceRecordWithError(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, string errorMessage, CancellationToken cancellationToken);
}
