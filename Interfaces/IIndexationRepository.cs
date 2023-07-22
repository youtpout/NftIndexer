using System;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;

namespace NftIndexer.Interfaces
{
    public interface IIndexationRepository
    {
        Task<bool> SaveERC721(List<EventLog<TransferEventDTO>> events);

    }
}

