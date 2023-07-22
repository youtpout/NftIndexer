using System;
using NftIndexer.Entities;

namespace NftIndexer.Interfaces
{
    public interface ITokenService
    {
        public Task<List<Token>> GetToken(int skip = 0, int take = 10, string address = "all", bool history = false);
    }
}

