using Microsoft.EntityFrameworkCore;

namespace LabelServiceConnector.Lib.Data
{
    public class ArchiveRecordContext : DbContext
    {
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
