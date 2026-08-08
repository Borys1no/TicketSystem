using Microsoft.EntityFrameworkCore;
using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;
using SupportFlow.Domain.Enums;
using SupportFlow.Infrastructure.Data;

namespace SupportFlow.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.CreatedByUserId)
            .Include(t => t.AssignedToUserId)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        return await _context.Tickets
            .Include(t => t.CreatedByUserId)
            .Include(t => t.AssignedToUserId)
            .OrderByDescending(t => t.CreateAt)
            .ToListAsync();
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var ticket = await GetByIdAsync(id);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(Domain.Enums.TicketStatus status)
    {
        return await _context.Tickets
            .Where(t=> t.Status == status)
            .Include(t => t.CreatedByUserId)
            .Include(t => t.AssignedToUserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId)
    {
        return await _context.Tickets
            .Where(t => t.AssignTo != null && t.AssignedToUserId == userId)
            .Include(t => t.CreatedByUserId)
            .Include(t => t.AssignedToUserId)
            .ToListAsync();
    }
}