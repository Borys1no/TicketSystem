using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Queries.Tickets;

public class GetTicketsQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> Handle()
    {
        return await _ticketRepository.GetAllAsync();
    }
}