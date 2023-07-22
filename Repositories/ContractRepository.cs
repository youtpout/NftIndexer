using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Entities;
using NftIndexer.Interfaces;

namespace NftIndexer.Repositories
{
    public class ContractRepository : RepositoryBase<Contract>, IContractRepository
    {
        private readonly NftIndexerContext _dbContext;

        public ContractRepository(NftIndexerContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }




        public override async Task<bool> Create(Contract entity)
        {
            var sameContract = await _dbContext.Contracts.FirstOrDefaultAsync(a => a.Address == entity.Address);
            if (sameContract == null)
            {
                _dbContext.Contracts.Add(entity);
            }
            else
            {
                entity.Id = sameContract.Id;
                _dbContext.Contracts.Update(entity);
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

