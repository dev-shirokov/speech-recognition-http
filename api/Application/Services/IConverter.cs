namespace api.Application.Services;

public interface IConverter
{
    Task Convert(Stream inputStream, Stream outputStream);
}