using InvestLens.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestLens.Data.DataContext;

public class InvestLensDataContext : DbContext
{
    public InvestLensDataContext(DbContextOptions<InvestLensDataContext> options) : base(options)
    {
    }

    public DbSet<Security> Security { get; set; }

    public DbSet<RefreshStatus> RefreshStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnSecurityCreating(modelBuilder);
        OnRefreshStatusCreating(modelBuilder);
    }

    private void OnSecurityCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Security>(security =>
        {
            security.ToTable("security");

            security.HasKey(s => s.Id);
            
            security.Property(s => s.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            security.Property(s => s.SecId)
                .HasColumnName("secid")
                .HasMaxLength(51)
                .IsRequired();

            security.HasIndex(s => s.SecId).IsUnique();

            security.Property(s => s.ShortName)
                .HasColumnName("shortname")
                .HasMaxLength(189)
                .HasDefaultValue("")
                .IsRequired();

            security.Property(s => s.RegNumber)
                .HasColumnName("regnumber")
                .HasMaxLength(189)
                .IsRequired(false);

            security.Property(s => s.Name)
                .HasColumnName("name")
                .HasMaxLength(765)
                .IsRequired();

            security.Property(s => s.Isin)
                .HasColumnName("isin")
                .HasMaxLength(51)
                .IsRequired(false);

            security.Property(s => s.IsTraded)
                .HasColumnName("is_traded")
                .IsRequired();

            security.Property(s => s.EmitentId)
                .HasColumnName("emitent_id")
                .IsRequired(false);

            security.Property(s => s.EmitentTitle)
                .HasColumnName("emitent_title")
                .HasMaxLength(765)
                .IsRequired(false);

            security.Property(s => s.EmitentInn)
                .HasColumnName("emitent_inn")
                .HasMaxLength(30)
                .IsRequired(false);

            security.Property(s => s.EmitentOkpo)
                .HasColumnName("emitent_okpo")
                .HasMaxLength(21)
                .IsRequired(false);

            security.Property(s => s.Type)
                .HasColumnName("type")
                .HasMaxLength(93)
                .IsRequired();

            security.Property(s => s.Group)
                .HasColumnName("group")
                .HasMaxLength(93)
                .IsRequired();

            security.Property(s => s.PrimaryBoardId)
                .HasColumnName("primary_boardid")
                .HasMaxLength(12)
                .IsRequired(false);

            security.Property(s => s.MarketpriceBoardId)
                .HasColumnName("marketprice_boardid")
                .HasMaxLength(12)
                .IsRequired(false);
        });

    }

    private void OnRefreshStatusCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshStatus>(rs =>
        {
            rs.ToTable("refresh_status");

            rs.HasKey(p => p.Id);

            rs.Property(p => p.EntityName)
                .HasColumnName("entity_name")
                .HasMaxLength(150)
                .IsRequired();

            rs.HasIndex(p => p.EntityName).IsUnique();

            rs.Property(p => p.RefreshDate)
                .HasColumnName("refresh_date")
                .IsRequired();

            rs.Property(p => p.LastError)
                .HasColumnName("last_error")
                .HasMaxLength(150);
        });
    }
}