namespace InvestLens.Auth.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public List<Role> Roles { get; set; } = [];
}