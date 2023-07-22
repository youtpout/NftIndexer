using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Entities;
using NftIndexer.Interfaces;

namespace NftIndexer.Repositories
{
    public class SyncRepository : RepositoryBase<SyncInfo>, ISyncRepository
    {
        private readonly NftIndexerContext _dbContext;

        public SyncRepository(NftIndexerContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<ulong> GetLastBlockIndexed()
        {
            ulong result = 0;
            var lastBlockSynced = await _dbContext.SyncInfos.OrderBy(x => x.ToBlock).LastOrDefaultAsync();
            if(lastBlockSynced != null)
            {
                result = ((ulong)lastBlockSynced.ToBlock);
            }
            return result;
        }
    }
}

