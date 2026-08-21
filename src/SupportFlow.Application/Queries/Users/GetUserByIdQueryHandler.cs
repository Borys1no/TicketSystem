using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;

namespace SupportFlow.Application.Queries.Users;

public class GetUserByIdQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await _userRepository.GetByIdAsync(query.Id);
    }
}