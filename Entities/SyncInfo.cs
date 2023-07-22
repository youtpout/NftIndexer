using System;
using System.ComponentModel.DataAnnotations;

namespace NftIndexer.Entities
{
	public class SyncInfo
	{
		public SyncInfo()
		{
		}

        [Key]
        public long Id { get; set; }
        public long FromBlock { get; set; }
        public long ToBlock { get; set; }
        public DateTime Time { get; set; }
        public string Error { get; set; }
    }
}

