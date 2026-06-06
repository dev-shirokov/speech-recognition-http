namespace api.Application.Features.Exceptions;

public class WaveVoiceRecordException : Exception
{
    protected string FileName { get; init; }

    public WaveVoiceRecordException(string fileName, string errorMessage, Exception e)
        : base($"{errorMessage}. FileName: {fileName}.", e)
    {
        FileName = fileName;
    }
}

public class ConvertingToWaveVoiceRecordException : WaveVoiceRecordException
{
    public ConvertingToWaveVoiceRecordException(string fileName, Exception e)
        : base(fileName, $"Failed to convert to wave format of file '{fileName}' ", e)
    {
    }
}

public class SavingMetadataOfWaveVoiceRecordException : WaveVoiceRecordException
{
    public SavingMetadataOfWaveVoiceRecordException(string fileName, Exception e)
        : base(fileName, $"Failed to save metadata of wave file '{fileName}' ", e)
    {
    }
}

public class QueueSendWaveVoiceRecordException : WaveVoiceRecordException
{
    public QueueSendWaveVoiceRecordException(string fileName, Exception e)
        : base(fileName, "Failed to send in queue 'voice-record-converted'", e)
    {

    }
}

