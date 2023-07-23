using System;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Dtos;
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


        public async Task<PaginationDto> GetToken(int skip = 0, int take = 10, string address = "", string owner = "", bool history = false)
        {
            List<TokenDto> tokens = new List<TokenDto>();
            if (take > 100 || take < 1)
            {
                take = 100;
            }

            if (skip < 0)
            {
                skip = 0;
            }

            IQueryable<Token> filter = _dbContext.Tokens.Include(x => x.Contract).Include(x => x.TokenHistories)
                .OrderByDescending(x => x.Id).Where(x => x != null && x.Contract != null && x.TokenHistories.Any()); ;

            if (!string.IsNullOrWhiteSpace(address))
            {
                string adr = address.ToLower();
                filter = filter.Where(x => x.Contract.Address.Contains(adr));
            }

            if (!string.IsNullOrWhiteSpace(owner))
            {
                string own = owner.ToLower();
                filter = filter.Where(x => x.TokenHistories.OrderByDescending(x => x.BlockNumber).ThenByDescending(x => x.TransactionIndex).First().To.Contains(owner));
            }

            var count = await filter.CountAsync();

            tokens = filter.Skip(skip).Take(take).ToList().Select((x, index) =>

                new TokenDto()
                {
                    Name = x.Contract.Name ?? string.Empty,
                    Symbol = x.Contract.Symbol ?? string.Empty,
                    Address = x.Contract.Address,
                    ContractType = x.Contract.ContractType,
                    Metadatas = x.Metadatas,
                    Owner = x.TokenHistories.Where(x => x != null).OrderByDescending(x => x.BlockNumber).ThenByDescending(x => x.TransactionIndex).First().To ?? string.Empty,
                    TokenId = x.TokenId,
                    Uri = x.Uri,
                    TokenHistories = !history ? new List<TokenHistoryDto>() : x.TokenHistories.Select(z => new TokenHistoryDto()
                    {
                        Amount = z.Amount,
                        From = z.From,
                        To = z.To,
                        EventType = z.EventType,
                        Uri = z.Uri,
                        BlockNumber = z.BlockNumber,
                        BlockHash = z.BlockHash,
                        TransactionHash = z.TransactionHash,
                        TransactionIndex = z.TransactionIndex,
                        LogIndex = z.LogIndex

                    }).ToList()
                }).ToList();

            return new PaginationDto() { Skip = skip, Take = take, Total = count, Tokens = tokens };
        }


    }
}

