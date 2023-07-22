using System;
using System.ComponentModel.DataAnnotations;

namespace NftIndexer.Entities
{
	public class Contract
	{
		public Contract()
		{
            Tokens = new HashSet<Token>();
        }

        [Key]
        public long Id { get; set; }
        public string Address { get; set; }
        public string ContractType { get; set; }
        public long LastBlockSynced { get; set; }
        public long LastBlockEvent { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? Uri { get; set; }

        public virtual ICollection<Token>? Tokens { get; set; }
    }
}

