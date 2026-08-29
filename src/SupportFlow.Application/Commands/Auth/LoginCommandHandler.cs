using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Commands.Auth;

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> Handle(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is null)
            throw new InvalidOperationException("Invalid email or password.");

        var passwordValid = _passwordHasher.Verify(
            command.Password,
            user.PasswordHash);
        if (!passwordValid)
            throw new InvalidOperationException("Invalid email or password.");

        return _jwtTokenGenerator.GenerateToken(user);
    }

}