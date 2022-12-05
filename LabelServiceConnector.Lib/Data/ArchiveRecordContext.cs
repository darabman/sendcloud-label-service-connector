using Microsoft.EntityFrameworkCore;

namespace LabelServiceConnector.Lib.Data
{
    public class ArchiveRecordContext : DbContext
    {
        public const int ArchivePeriodHours = 24;

        public const int ArchiveAfterDays = 30;

        public DbSet<ParcelRecordEntity> ParcelRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source=archive.db");
            }
        }
    }
}
