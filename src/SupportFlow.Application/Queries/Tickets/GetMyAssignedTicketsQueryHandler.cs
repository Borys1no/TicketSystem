using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Queries.Tickets;

public class GetMyAssignedTicketsQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetMyAssignedTicketsQueryHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> Handle(
        GetMyAssignedTicketsQuery query)
    {
        return await _ticketRepository.GetByAssignedToUserAsync(
            query.UserId);
    }
}