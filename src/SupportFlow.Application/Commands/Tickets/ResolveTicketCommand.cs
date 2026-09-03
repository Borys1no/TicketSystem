namespace SupportFlow.Application.Commands.Tickets;

public class ResolveTicketCommand
{
    public Guid TicketId { get; set; }
    public Guid TechnicianId { get; set; }
}