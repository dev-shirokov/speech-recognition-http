using System.Text.Json.Serialization;

namespace api.Domain.Models;

public record TaskCreatingModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskTypeEnum Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskPriorityEnum? Priority { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid UserId { get; set; }
    public Guid? VoiceRecordId { get; set; }

    public override string ToString() 
        => $"Title: {Title}, type: {Type}, dueDate: {DueDate}, priority: {Priority}, errorMessage: {ErrorMessage}";
}
