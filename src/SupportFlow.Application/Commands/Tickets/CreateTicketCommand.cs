using SupportFlow.Domain.Enums;
namespace SupportFlow.Application.Commands.Tickets;

public class CreateTicketCommand
{
  public string Title { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Priority Priority { get; set; }
  public Guid CreatedByUserId { get; set; }
}