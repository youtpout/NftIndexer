using System;
using System.Numerics;
using NftIndexer.Entities;

namespace NftIndexer.Dtos
{
    public class TokenDto
    {
        public TokenDto()
        {
            TokenHistories = new HashSet<TokenHistoryDto>();
        }


        public string ContractType { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Address { get; set; }
        public BigInteger TokenId { get; set; }
        public string? Uri { get; set; }
        public string? Metadatas { get; set; }
        public string Owner { get; set; }

        public virtual ICollection<TokenHistoryDto> TokenHistories
        {
            get; set;
        }
    }
}

