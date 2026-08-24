using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Queries.Users;

public class GetUsersQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> Handle(GetUsersQuery query)
    {
        return await _userRepository.GetAllAsync();
    }
}