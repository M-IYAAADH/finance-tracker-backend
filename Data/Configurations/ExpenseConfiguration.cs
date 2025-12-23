using FinanceTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Api.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Date)
                   .IsRequired();

              
            builder.Property(e => e.Type)
                   .IsRequired();
       
            builder.Property(e => e.Description)
                   .HasMaxLength(500);

            builder.HasOne(e => e.User)
                   .WithMany()
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Category)
                   .WithMany()
                   .HasForeignKey(e => e.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.CreatedAt)
                   .IsRequired();

            builder.Property(e => e.UpdatedAt);

        }
    }
}
