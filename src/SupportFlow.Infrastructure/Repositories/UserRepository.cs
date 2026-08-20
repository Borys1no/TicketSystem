using Microsoft.EntityFrameworkCore;
using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;
using SupportFlow.Infrastructure.Data;

namespace SupportFlow.Infrastructure.Repositories;

public class UserRepository :IUserRepository

{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .ToListAsync();
    }
}