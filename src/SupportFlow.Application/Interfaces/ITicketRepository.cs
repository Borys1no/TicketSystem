using SupportFlow.Domain.Entities;

namespace SupportFlow.Application.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket);

    Task<Ticket?> GetByIdAsync(Guid id);

    Task<IEnumerable<Ticket>> GetAllAsync();

    Task UpdateAsync(Ticket ticket);

}