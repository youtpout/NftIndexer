using System;
using Microsoft.EntityFrameworkCore;

namespace NftIndexer.Entities
{
	public class NftIndexerContext :DbContext
	{
        private readonly IConfiguration _configuration;

        public NftIndexerContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public NftIndexerContext(IConfiguration configuration, DbContextOptions<NftIndexerContext> options)
          : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<TokenHistory> TokenHistories { get; set; }
        public virtual DbSet<SyncInfo> SyncInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("NftDatabase")).UseSnakeCaseNamingConvention();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}

