namespace api.Infrastructure.Services;

public interface IConverter
{
    Task Convert(Stream inputStream, Stream outputStream);
}