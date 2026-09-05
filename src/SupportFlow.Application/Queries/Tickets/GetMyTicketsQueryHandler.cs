using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Queries.Tickets;

public class GetMyTicketsQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetMyTicketsQueryHandler(
        ITicketRepository ticketRepository
    )
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> Handle(GetMyTicketsQuery query)
    {
        return await _ticketRepository.GetByCreatedByUserAsync(
            query.UserId);
    }
}