using InvestLens.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestLens.Auth.DataContext;

public class InvestLensAuthContext : DbContext
{
    public InvestLensAuthContext(DbContextOptions<InvestLensAuthContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnRoleCreating(modelBuilder);
        OnUserCreating(modelBuilder);
        OnUserToRoleCreating(modelBuilder);
    }

    protected void OnRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(role =>
        {
            role.ToTable("role");

            role.Property(r => r.Name)
                .HasMaxLength(15)
                .IsRequired();
        });
    }

    protected void OnUserCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("user");

            user.Property(u => u.UserName)
                .HasMaxLength(15)
                .IsRequired();

            user.Property(u => u.Email)
                .IsRequired();

            user.Property(u => u.PasswordHash)
                .IsRequired();

            user.Property(u => u.IsDeleted)
                .HasDefaultValue(false);
        });
    }

    private void OnUserToRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasMany(u => u.Roles).WithMany(r => r.Users);
        });
    }
}