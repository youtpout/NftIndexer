using System;
using NftIndexer.Entities;

namespace NftIndexer.Dtos
{
    public class TokenHistoryDto
    {
        public TokenHistoryDto()
        {

        }

        public string From { get; set; }
        public string To { get; set; }
        public string EventType { get; set; }
        public string? Uri { get; set; }
        public long Amount { get; set; }
        public long BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string TransactionHash { get; set; }
        public long TransactionIndex { get; set; }
        public long LogIndex { get; set; }
    }
}
