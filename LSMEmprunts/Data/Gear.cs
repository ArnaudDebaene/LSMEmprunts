using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LSMEmprunts.Data
{
    public enum GearType
    {
        Regulator,
        BCD,
        Tank
    }

    public class Gear
    {
        public int Id { get; set; }

        public GearType Type { get; set; }

        public string Name { get; set; }

        public string BarCode { get; set; }

        public ICollection<Borrowing> Borrowings { get; set; }

        public string Size { get; set; }
    }


    class GearMapping : IEntityTypeConfiguration<Gear>
    {
        public void Configure(EntityTypeBuilder<Gear> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.BarCode).IsRequired();
        }
    }
}
