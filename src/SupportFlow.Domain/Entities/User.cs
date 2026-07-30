using SupportFlow.Domain.Enums;
namespace SupportFlow.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Department Department { get; private set; }
    public string? PhoneNumber { get; private set; }
    
    public Role Role { get; private set; }

    public User(
        string name,
        string lastName,
        string email,
        string passwordHash,
        Department department,
        string? phoneNumber,
        Role role
    )
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty ", nameof(name));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty ", nameof(lastName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty ", nameof(email));
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password cannot be empty ", nameof(passwordHash));
        
        if (!Enum.IsDefined(typeof(Department), department))
            throw new ArgumentException("Invalid department", nameof(department));
        
        Id = Guid.NewGuid();
        Name = name;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Department = department;
        PhoneNumber = phoneNumber;
        Role = role;
        
    }

}