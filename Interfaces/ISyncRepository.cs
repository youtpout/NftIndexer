using System;
using NftIndexer.Entities;

namespace NftIndexer.Interfaces
{
    public interface ISyncRepository : IRepositoryBase<SyncInfo>
    {
        Task<ulong> GetLastBlockIndexed();
    }
}

