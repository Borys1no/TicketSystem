using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}