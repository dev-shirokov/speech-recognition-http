namespace api.Application.Features.Exceptions;

public class RecognizeVoiceRecordException : Exception
{
    protected string FileName { get; init; }

    public RecognizeVoiceRecordException(string fileName, string errorMessage, Exception e)
        : base($"{errorMessage}. FileName: {fileName}.", e)
    {
        FileName = fileName;
    }
}

public class KaldiRecognizeVoiceRecordException : RecognizeVoiceRecordException
{
    public KaldiRecognizeVoiceRecordException(string fileName, Exception e)
        : base(fileName, $"Failed to recognize voice record of file '{fileName}' ", e)
    {

    }
}

public class SaveRecognizeSpeechException : RecognizeVoiceRecordException
{
    public SaveRecognizeSpeechException(string fileName, Exception e)
        : base(fileName, $"failed to save recognize speech to metadata '{fileName}' ", e)
    {
    }
}

public class QueueSendRecognizeSpeechException : Exception
{
    public QueueSendRecognizeSpeechException(Guid id, Guid userId, Exception e)
        : base($"Failed to send in queue 'voice-record-recognized'. Id: {id}, UserId: {userId}", e)
    {

    }
}

public class LlmRecognizeSpeechException : Exception
{
    public LlmRecognizeSpeechException(Guid id, Guid userId, Exception? e)
        : base($"Failed handle speech text in LLM. Output model is empty. Id: {id}, UserId: {userId}", e)
    {

    }
}
public class SaveTaskFromVoiceRecordException : Exception
{
    public SaveTaskFromVoiceRecordException(Guid id, Guid userId, Exception? e)
        : base($"Failed save task from voice record in database. Id: {id}, UserId: {userId}", e)
    {

    }
}
