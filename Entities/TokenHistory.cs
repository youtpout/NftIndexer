using System;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using System.ComponentModel.DataAnnotations;

namespace NftIndexer.Entities
{
    public class TokenHistory
    {

        public TokenHistory()
        {
        }

        [Key]
        public long Id { get; set; }
        /// <summary>
        /// Id of the token in the database (not the token id onchain)
        /// </summary>
        public long TokenId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        /// <summary>
        /// Transfer, Mint, Uri, Burn
        /// </summary>
        public string EventType { get; set; }
        public string? Uri { get; set; }
        /// <summary>
        /// Metadatas of the nft obtained during this transfer event
        /// </summary>
        public string? Metadatas { get; set; }

        /// <summary>
        /// Amount of token transfered, default one for ERC721
        /// </summary>
        public long Amount { get; set; }

        public long BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string TransactionHash { get; set; }
        public long TransactionIndex { get; set; }
        public long LogIndex { get; set; }

        public DateTime Time { get; set; }
        public string? Error { get; set; }

        public virtual Token Token { get; set; }
    }
}

