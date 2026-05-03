using api.Domain;
using api.Domain.Models;

namespace api.Application.Services;

public interface ITaskCreatingService
{
    Task InsertAsync(TaskCreationModel model, CancellationToken cancellationToken);
    Task InsertVoiceRecordAsync(Guid requestId, Guid userId, string fileName, CancellationToken cancellationToken);
    Task UpdateVoiceRecordSpeechAsync(Guid id, Guid userId, string recognizeSpeech, CancellationToken cancellationToken);
    Task UpdateVoiceRecordStatusAsync(Guid id, Guid userId, VoiceRecordSavingStatusEnum status, CancellationToken cancellationToken);
}
