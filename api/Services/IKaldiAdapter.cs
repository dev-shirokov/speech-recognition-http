using api.Models;

namespace api.Services;

public interface IKaldiAdapter
{
    /// <summary>
    /// Recognize speech from audio file to text
    /// </summary>
    /// <param name="file">File from form-data submit</param>
    Task<KaldiResult?> Recognize(IFormFile file);
}