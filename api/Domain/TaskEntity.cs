using System.ComponentModel.DataAnnotations;

namespace api.Domain;

public class TaskEntity
{
    [Key]
    public Guid Id { get; set; }
    public required TaskTypeEnum TaskType { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required TaskPriorityEnum TaskPriority { get; set; } = TaskPriorityEnum.Medium;
    public DateTime? DueDateTime { get; set; }
    public Guid UserId { get; set; }
    public required DateTime Dtc { get; set; }
}

public enum TaskPriorityEnum
{
    Low,
    Medium,
    High
}

public enum TaskTypeEnum
{
    Goal,
    Event,
    Task
}