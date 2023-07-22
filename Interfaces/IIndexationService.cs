using System;
namespace NftIndexer.Interfaces
{
	public interface IIndexationService
	{
		public Task<bool> IndexData();
	}
}

