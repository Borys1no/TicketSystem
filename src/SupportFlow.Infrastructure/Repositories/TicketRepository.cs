using Microsoft.EntityFrameworkCore;
using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;
using SupportFlow.Infrastructure.Data;

namespace SupportFlow.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        return await _context.Tickets.ToListAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByCreatedByUserAsync(Guid userId)
    {
        return await _context.Tickets
            .Where(t => t.CreatedByUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedToUserAsync(Guid userId)
    {
        return await _context.Tickets
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync();
    }
}