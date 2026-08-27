using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Commands.Tickets;

public class ReopenTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;

    public ReopenTicketCommandHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> Handle(ReopenTicketCommand command)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId);
        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");
        ticket.Reopen();
        await _ticketRepository.UpdateAsync(ticket);
        return ticket;
    }
}