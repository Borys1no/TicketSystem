using SupportFlow.Domain.Enums;

namespace SupportFlow.Application.DTOs.Tickets;

public class CreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
}