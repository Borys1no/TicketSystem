using SupportFlow.Application.DTOs.Users;
using SupportFlow.Domain.Entities;

namespace SupportFlow.Application.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponse(User user)
    {
        return new UserResponse()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Department = user.Department.ToString(),
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString()
        };
    }
}