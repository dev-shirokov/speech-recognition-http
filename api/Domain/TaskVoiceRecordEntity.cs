using System.ComponentModel.DataAnnotations;

namespace api.Domain;

public class VoiceRecordEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TaskId { get; set; }
    public required string Path { get; set; }
    public VoiceRecordSavingStatusEnum Status { get; set; }
    public string? RecognizeSpeech { get; set; }
}

public enum VoiceRecordSavingStatusEnum
{
    None,
    ThreeGppSaved,
    WaveSaved,
    Recognized
}
