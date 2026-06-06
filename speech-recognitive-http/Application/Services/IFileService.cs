using api.Infrastructure.Services;

namespace api.Application.Services;

/// <summary>
/// Сервис загрузки файлов из S3.
/// </summary>
public interface IFileService
{
    Task Save(PutObjectModel model, CancellationToken cancellationToken);
}