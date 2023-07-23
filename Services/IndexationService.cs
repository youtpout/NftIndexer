using System;
using System.Numerics;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Contracts.Standards.ERC721;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using NftIndexer.Entities;
using NftIndexer.Interfaces;
using NftIndexer.Repositories;

namespace NftIndexer.Services
{
    public class IndexationService : IIndexationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexationService> _logger;
        private readonly ISyncRepository _syncInfoRepository;
        private readonly IIndexationRepository _indexationRepository;
        private readonly string _ipfsGateway;
        private static BigInteger lastBlockIndexed = 0;

        private static ulong startBlock = 0;
        private static ulong increment = 100;
        private static ulong toBlock = 100;

        private readonly Web3 _web3;

        public IndexationService(IConfiguration configuration,
            ILogger<IndexationService> logger, IIndexationRepository indexationRepository, ISyncRepository syncInfoRepository)
        {
            _configuration = configuration;
            _logger = logger;

            increment = ulong.Parse(_configuration["BlockRange"]);
            _ipfsGateway = _configuration["IpfsGateway"];

            _syncInfoRepository = syncInfoRepository;
            _indexationRepository = indexationRepository;

            var url = _configuration["RpcUrl"];
            // private key from nethereum exemple don't use it as personnal wallet
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var account = new Account(privateKey);
            _web3 = new Web3(account, url);
        }

        public async Task<bool> IndexData()
        {
            try
            {
                var latestBlockMined = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                ulong lastMined = ((ulong)latestBlockMined.Value);

                lastBlockIndexed = await _syncInfoRepository.GetLastBlockIndexed();
                startBlock = ((ulong)lastBlockIndexed) + 1;
                if (lastMined < startBlock)
                {
                    _logger.LogInformation($"No new block");
                    return true;
                }
                toBlock = startBlock + increment;
                if (toBlock > lastMined)
                {
                    toBlock = lastMined;
                }

                _logger.LogInformation($"From block {startBlock} to {toBlock}");

                // filter erc721 transfer event
                var allTransferEventsForContract = await FilterEventAllContracts<TransferEventDTO>(startBlock, toBlock);
                _logger.LogInformation($"ERC721 Events detected {allTransferEventsForContract.Count}");

                // filter erc1155 single transfer event
                var allTransferSingleEventsForContract = await FilterEventAllContracts<TransferSingleEventDTO>(startBlock, toBlock);
                _logger.LogInformation($"ERC1155 Transfer single Events detected {allTransferSingleEventsForContract.Count}");

                // filter erc1155 batch transfer event
                var allTransferBatchEventsForContract = await FilterEventAllContracts<TransferBatchEventDTO>(startBlock, toBlock);
                _logger.LogInformation($"ERC1155 Transfer single Events detected {allTransferBatchEventsForContract.Count}");

                // filter erc1155 uri event
                var allUriEventsForContract = await FilterEventAllContracts<URIEventDTO>(startBlock, toBlock);
                _logger.LogInformation($"ERC1155 Transfer single Events detected {allUriEventsForContract.Count}");


                await _indexationRepository.SaveERC721(allTransferEventsForContract);

                SyncInfo info = new SyncInfo() { FromBlock = ((long)startBlock), ToBlock = ((long)toBlock), Time = DateTime.UtcNow };
                await _syncInfoRepository.Create(info);

                startBlock += increment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexData");
                SyncInfo info = new SyncInfo() { FromBlock = ((long)startBlock), ToBlock = ((long)toBlock), Time = DateTime.UtcNow, Error = ex.Message };
                await _syncInfoRepository.Create(info);
            }

            return true;
        }


        private async Task<List<EventLog<T>>> FilterEventAllContracts<T>(ulong blockFrom, ulong blockTo) where T : IEventDTO, new()
        {
            var transferEventHandlerAnyContract = _web3.Eth.GetEvent<T>();
            var filterAllTransferEventsForAllContracts = transferEventHandlerAnyContract.CreateFilterInput(fromBlock: new BlockParameter(blockFrom), toBlock: new BlockParameter(blockTo));
            var allTransferEventsForContract = await transferEventHandlerAnyContract.GetAllChangesAsync(filterAllTransferEventsForAllContracts);
            return allTransferEventsForContract;
        }


    }
}

