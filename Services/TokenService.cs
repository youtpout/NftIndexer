using System;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Entities;
using NftIndexer.Interfaces;

namespace NftIndexer.Services
{
    public class TokenService : ITokenService
    {
        private readonly NftIndexerContext _dbContext;

        public TokenService(NftIndexerContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<Token>> GetToken(int skip = 0, int take = 10, string address = "all", bool history = false)
        {
            List<Token> tokens = new List<Token>();
            if (take > 100 || take < 1)
            {
                take = 100;
            }

            if (skip < 0)
            {
                skip = 0;
            }

            IQueryable<Token> filter = _dbContext.Tokens.OrderBy(x => x.Id);

            if (address != "all")
            {
                string adr = address.ToLower();
                filter = filter.Where(x => x.Contract.Address.Contains(adr));
            }

            if (history)
            {
                filter = filter.Include(x => x.TokenHistories);
            }

            tokens = filter.Skip(skip).Take(take).ToList();

            return tokens;
        }

        
    }
}

