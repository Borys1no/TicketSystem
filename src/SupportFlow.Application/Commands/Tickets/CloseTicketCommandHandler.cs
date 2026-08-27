using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Commands.Tickets;

public class CloseTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;

    public CloseTicketCommandHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> Handle(CloseTicketCommand command)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");
        ticket.Close();
        await _ticketRepository.UpdateAsync(ticket);
        return ticket;
    }
}