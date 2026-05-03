namespace api.Domain.Models;

public record TaskCreationModel
{
    public required TaskTypeEnum Type { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public required TaskPriorityEnum Priority { get; init; }
    public string? ErrorMessage { get; init; }
    public string RequestText { get; set; }
    public Guid UserId { get; set; }
}
