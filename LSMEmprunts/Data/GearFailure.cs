using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LSMEmprunts.Data
{
    public class GearFailure
    {
        public int Id { get; set; }

        public int GearId { get; set; }
        public Gear Gear { get; set; }

        public DateTime OpenDate { get; set; }
        public string OpeningDescription { get; set; }

        public DateTime? CloseDate { get; set; }
        public string ClosingDescription { get; set; }
    }

    class GearFailureMapping : IEntityTypeConfiguration<GearFailure>
    {
        public void Configure(EntityTypeBuilder<GearFailure> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasOne(e => e.Gear).WithMany(e => e.Failures).HasForeignKey(e => e.GearId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
