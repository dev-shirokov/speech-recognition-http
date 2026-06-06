using api.Domain;

namespace api.Domain.Models;

public record TaskCreatingStatus(Guid RequestId, Guid? TaskId, string Status, string? ErrorMessage)
{
    public bool Ended { get; set; }
}