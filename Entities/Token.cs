using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace NftIndexer.Entities
{
	public class Token
	{
		public Token()
		{
            TokenHistories = new HashSet<TokenHistory>();
        }

        [Key]
        public long Id { get; set; }
        public long ContractId { get; set; }
        public BigInteger TokenId { get; set; }
        public string? Uri { get; set; }
        public string? Metadatas { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<TokenHistory> TokenHistories { get; set; }

    }
}

