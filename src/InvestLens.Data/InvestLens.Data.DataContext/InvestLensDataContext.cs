using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Index;
using Microsoft.EntityFrameworkCore;

namespace InvestLens.Data.DataContext;

public class InvestLensDataContext : DbContext
{
    public InvestLensDataContext(DbContextOptions<InvestLensDataContext> options) : base(options)
    {
    }

    public DbSet<Security> Securities { get; set; }
    public DbSet<Engine> Engines { get; set; }
    public DbSet<Market> Markets { get; set; }

    public DbSet<RefreshStatus> RefreshStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnSecurityCreating(modelBuilder);
        OnRefreshStatusCreating(modelBuilder);
        OnEngineCreating(modelBuilder);
        OnMarketCreating(modelBuilder);
        OnBoardCreating(modelBuilder);
        OnBoardGroupCreating(modelBuilder);
        OnDurationCreating(modelBuilder);
        OnSecurityTypeCreating(modelBuilder);
        OnSecurityGroupCreating(modelBuilder);
        OnSecurityCollectionCreating(modelBuilder);
    }

    private void OnSecurityCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Security>(security =>
        {
            security.ToTable("security");

            security.HasKey(s => s.Id);

            security.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

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

            rs.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

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

    private void OnEngineCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Engine>(engine =>
        {
            engine.ToTable("engine");

            engine.HasKey(e => e.Id);

            engine.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            engine.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(45)
                .IsRequired();

            engine.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(765)
                .IsRequired();
        });
    }

    private void OnMarketCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Market>(market =>
        {
            market.ToTable("market");

            market.HasKey(e => e.Id);

            market.Property(e => e.Id)
                .HasColumnName("id");

            market.Property(e => e.TradeEngineId)
                .HasColumnName("trade_engine_id")
                .IsRequired();

            market.Property(m => m.TradeEngineName)
                .HasColumnName("trade_engine_name")
                .HasMaxLength(45)
                .IsRequired();

            market.Property(m => m.TradeEngineTitle)
                .HasColumnName("trade_engine_title")
                .HasMaxLength(765)
                .IsRequired();

            market.Property(m => m.MarketName)
                .HasColumnName("market_name")
                .HasMaxLength(45)
                .IsRequired();

            market.Property(m => m.MarketTitle)
                .HasColumnName("market_title")
                .HasMaxLength(765)
                .IsRequired();

            market.Property(m => m.MarketId)
                .HasColumnName("market_id")
                .IsRequired();

            market.Property(m => m.Marketplace)
                .HasColumnName("marketplace")
                .HasMaxLength(48)
                .IsRequired();


            market.Property(m => m.IsOtc)
                .HasColumnName("is_otc");

            market.Property(m => m.HasHistoryFiles)
                .HasColumnName("has_history_files");

            market.Property(m => m.HasHistoryTradesFiles)
                .HasColumnName("has_history_trades_files");

            market.Property(m => m.HasTrades)
                .HasColumnName("has_trades");

            market.Property(m => m.HasHistory)
                .HasColumnName("has_history");

            market.Property(m => m.HasCandles)
                .HasColumnName("has_candles");

            market.Property(m => m.HasOrderbook)
                .HasColumnName("has_orderbook");

            market.Property(m => m.HasTradingSession)
                .HasColumnName("has_tradingsession");

            market.Property(m => m.HasExtraYields)
                .HasColumnName("has_extra_yields");

            market.Property(m => m.HasDelay)
                .HasColumnName("has_delay");
        });
    }

    private void OnBoardCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(b =>
        {
            b.ToTable("board");

            b.HasKey(s => s.Id);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.BoardGroupId)
                .HasColumnName("board_group_id");

            b.Property(s => s.EngineId)
                .HasColumnName("engine_id");

            b.Property(s => s.MarketId)
                .HasColumnName("market_id");

            b.Property(s => s.BoardId)
                .HasColumnName("boardid")
                .HasMaxLength(12);

            b.Property(s => s.BoardTitle)
                .HasColumnName("board_title")
                .HasMaxLength(381);

            b.Property(s => s.IsTraded)
                .HasColumnName("is_traded");

            b.Property(s => s.HasCandles)
                .HasColumnName("has_candles");

            b.Property(s => s.IsPrimary)
                .HasColumnName("is_primary");
        });
    }

    private void OnBoardGroupCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardGroup>(b =>
        {
            b.ToTable("boardgroup");

            b.HasKey(s => s.Id);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.TradeEngineId)
                .HasColumnName("trade_engine_id");

            b.Property(s => s.TradeEngineName)
                .HasColumnName("trade_engine_name")
                .HasMaxLength(45);

            b.Property(s => s.TradeEngineTitle)
                .HasColumnName("trade_engine_title")
                .HasMaxLength(765);

            b.Property(s => s.MarketId)
                .HasColumnName("market_id");

            b.Property(s => s.MarketName)
                .HasColumnName("market_name")
                .HasMaxLength(45);

            b.Property(s => s.Name)
                .HasColumnName("name")
                .HasMaxLength(192);

            b.Property(s => s.Title)
                .HasColumnName("title")
                .HasMaxLength(765);

            b.Property(s => s.IsDefault)
                .HasColumnName("is_default");

            b.Property(s => s.BoardGroupId)
                .HasColumnName("board_group_id");

            b.Property(s => s.IsTraded)
                .HasColumnName("is_traded");

            b.Property(s => s.IsOrderDriven)
                .HasColumnName("is_order_driven");

            b.Property(s => s.Category)
                .HasColumnName("category")
                .HasMaxLength(45);
        });
    }

    private void OnDurationCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Duration>(b =>
        {
            b.ToTable("duration");

            b.HasKey(s => s.Interval);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.Interval)
                .HasColumnName("interval");

            b.Property(s => s.DurationValue)
                .HasColumnName("duration");

            b.Property(s => s.Days)
                .HasColumnName("days");

            b.Property(s => s.Title)
                .HasColumnName("title")
                .HasMaxLength(765);

            b.Property(s => s.Hint)
                .HasColumnName("hint")
                .HasMaxLength(765);
        });
    }

    private void OnSecurityTypeCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SecurityType>(b =>
        {
            b.ToTable("securitytype");

            b.HasKey(s => s.Id);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.TradeEngineId)
                .HasColumnName("trade_engine_id");

            b.Property(s => s.TradeEngineName)
                .HasColumnName("trade_engine_name")
                .HasMaxLength(45);

            b.Property(s => s.TradeEngineTitle)
                .HasColumnName("trade_engine_title")
                .HasMaxLength(765);

            b.Property(s => s.SecurityTypeName)
                .HasColumnName("security_type_name")
                .HasMaxLength(93);

            b.Property(s => s.SecurityTypeTitle)
                .HasColumnName("security_type_title")
                .HasMaxLength(765);

            b.Property(s => s.SecurityGroupName)
                .HasColumnName("security_group_name")
                .HasMaxLength(93);

            b.Property(s => s.StockType)
                .HasColumnName("stock_type")
                .HasMaxLength(3);
        });
    }

    private void OnSecurityGroupCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SecurityGroup>(b =>
        {
            b.ToTable("securitygroup");

            b.HasKey(s => s.Id);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.Name)
                .HasColumnName("name")
                .HasMaxLength(93)
                .IsRequired();

            b.Property(s => s.Title)
                .HasColumnName("title")
                .HasMaxLength(765)
                .IsRequired();

            b.Property(s => s.IsHidden)
                .HasColumnName("is_hidden")
                .IsRequired();
        });
    }

    private void OnSecurityCollectionCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SecurityCollection>(b =>
        {
            b.ToTable("securitycollection");

            b.HasKey(s => s.Id);

            b.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property(s => s.Name)
                .HasColumnName("name")
                .HasMaxLength(96)
                .IsRequired();

            b.Property(s => s.Title)
                .HasColumnName("title")
                .HasMaxLength(765)
                .IsRequired();

            b.Property(s => s.SecurityGroupId)
                .HasColumnName("security_group_id");
        });
    }
}