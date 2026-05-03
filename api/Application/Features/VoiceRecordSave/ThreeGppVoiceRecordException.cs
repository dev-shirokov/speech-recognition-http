namespace api.Application.Features.VoiceRecordSave;

public class ThreeGppVoiceRecordException : Exception
{
    protected string FileName { get; init; }
    protected string FileContentType { get; init; }
    protected long FileLength { get; init; }

    public ThreeGppVoiceRecordException(string fileName, string fileContentType, long fileLength, string errorMessage, Exception e)
        : base($"{errorMessage}. FileName: {fileName}, FileContentType: {fileContentType}, FileLength: {fileLength}", e)
    {
        FileName = fileName;
        FileContentType = fileContentType;
        FileLength = fileLength;
    }
}

public class DatabaseSaveThreeGppVoiceRecordException : ThreeGppVoiceRecordException
{
    public DatabaseSaveThreeGppVoiceRecordException(string fileName, string fileContentType, long fileLength, Exception e)
        : base(fileName, fileContentType, fileLength, "Failed to save original (3gp) voice record in database", e)
    {

    }
}

public class S3SaveThreeGppVoiceRecordException : ThreeGppVoiceRecordException
{
    public S3SaveThreeGppVoiceRecordException(string fileName, string fileContentType, long fileLength, Exception e)
        : base(fileName, fileContentType, fileLength, "Failed to save original (3gp) voice record in media storage", e)
    {

    }
}

public class QueueSendThreeGppVoiceRecordException : ThreeGppVoiceRecordException
{
    public QueueSendThreeGppVoiceRecordException(string fileName, string fileContentType, long fileLength, Exception e)
        : base(fileName, fileContentType, fileLength, "Failed to send in queue 'voice-record-saved'", e)
    {

    }
}
