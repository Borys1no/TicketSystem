using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Queries.Tickets;

public class GetTicketByIdQueryHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
        
    }

    public async Task<Ticket?> Handle(Guid id)
    {
        return await _ticketRepository.GetByIdAsync(id);
    }
}