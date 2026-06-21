using Microsoft.EntityFrameworkCore;
using Modern.Api.Domain;

namespace Modern.Api.Infrastructure.Ef;

// Code First, mapped against the schema an EF6 EDMX used to describe visually.
// The schema is unchanged — only the mapping moved from the designer into this class.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("tbl_Orders"); // legacy table name, kept as-is
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                .HasColumnName("OrderId")
                .ValueGeneratedNever();

            entity.Property(o => o.CustomerName)
                .HasColumnName("CustomerName")
                .HasMaxLength(200);

            entity.Property(o => o.Status)
                .HasColumnName("StatusCode")
                .HasConversion<int>();

            entity.Property(o => o.CreatedAt)
                .HasColumnName("CreatedAt");

            entity.ComplexProperty(o => o.Total, total =>
            {
                total.Property(m => m.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
                total.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

            // The EDMX used to generate this insert as a stored procedure call.
            // EF Core supports the same thing via InsertUsingStoredProcedure, re-wired
            // explicitly instead of through the designer — see Phase 2 in the article.
            // Left as a comment here: it requires a real usp_InsertOrder procedure in
            // the target database, and parameters for Total are configured through a
            // nested owned-type builder (OwnsOne), not through ComplexProperty above.
            //
            // entity.InsertUsingStoredProcedure("usp_InsertOrder", sp => sp
            //     .HasParameter(o => o.CustomerName)
            //     .HasParameter(o => o.Status)
            //     .HasParameter(o => o.CreatedAt)
            //     .HasResultColumn(o => o.Id));
        });
    }
}
