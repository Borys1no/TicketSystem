using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Commands.Users;

public class CreateUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> Handle(CreateUserCommand command)
    {
        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = new User(
            command.Name,
            command.LastName,
            command.Email,
            passwordHash,
            command.Department,
            command.PhoneNumber,
            command.Role
        );
        
        await _userRepository.AddAsync(user);
        return user;
    }
}