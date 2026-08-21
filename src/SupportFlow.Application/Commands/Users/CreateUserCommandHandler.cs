using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Commands.Users;

public class CreateUserCommandHandler
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Handle(CreateUserCommand command)
    {
        var user = new User(
            command.Name,
            command.LastName,
            command.Email,
            command.PasswordHash,
            command.Department,
            command.PhoneNumber,
            command.Role
        );
        await _userRepository.AddAsync(user);
        return user;
    }
}