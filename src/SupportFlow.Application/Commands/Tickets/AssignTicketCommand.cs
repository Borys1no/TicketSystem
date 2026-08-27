namespace SupportFlow.Application.Commands.Tickets;

public class AssignTicketCommand
{
    public Guid TicketId { get; set; }
    public Guid TechnicianId { get; set; }
}
