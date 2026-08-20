using System.Data;
using SupportFlow.Domain.Enums;


namespace SupportFlow.Application.DTOs.Users;

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Department Department { get; set; }
    public string? PhoneNumber { get; set; }
    public Role Role { get; set; }
}