using System;
using NftIndexer.Dtos;
using NftIndexer.Entities;

namespace NftIndexer.Interfaces
{
    public interface ITokenService
    {
        public Task<PaginationDto> GetToken(int skip = 0, int take = 10, string address = "", string owner = "", bool history = false);
    }
}

