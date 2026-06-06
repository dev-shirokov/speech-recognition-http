using api.Application.Models;

namespace api.Application.Services;

public interface IAsmrAdapter
{
    /// <summary>
    /// Recognize speech from audio file bytes to text
    /// </summary>
    /// <param name="file">File from form-data submit</param>
    /// <param name="token">Cancellation token</param>
    Task<KaldiResult?> Recognize(byte[] fileBytes, CancellationToken token);


    /// <summary>
    /// Recognize speech audio file stream to text
    /// </summary>
    /// <param name="inputStream">Stream of audio file</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    Task<KaldiResult?> Recognize(Stream inputStream, CancellationToken token);
}
