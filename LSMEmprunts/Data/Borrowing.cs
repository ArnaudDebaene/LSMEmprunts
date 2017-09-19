using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LSMEmprunts.Data
{
    public enum BorrowingState
    {
        Open,
        GearReturned,
        ForcedClose
    }

    public class Borrowing
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int GearId { get; set; }
        public Gear Gear { get; set; }

        public DateTime BorrowTime { get; set; }

        public DateTime? ReturnTime { get; set; }

        public BorrowingState State { get; set; }

        public string Comment { get; set; }
    }

    class BorrowingMapping : IEntityTypeConfiguration<Borrowing>
    {
        public void Configure(EntityTypeBuilder<Borrowing> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.State).HasDefaultValue(BorrowingState.Open);

            builder.HasOne(e => e.User).WithMany(e=>e.Borrowings).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Gear).WithMany(e=>e.Borrowings).HasForeignKey(e => e.GearId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.State);
        }
    }
}
