using System;
using System.Numerics;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NftIndexer.Interfaces;

namespace NftIndexer.Services
{
    public class IndexationService : IIndexationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexationService> _logger;
        private static BigInteger lastBlock = 0;

        private static ulong startBlock = 0;
        private static ulong increment = 100;

        private readonly Web3 _web3;

        public IndexationService(IMemoryCache memoryCache, IMapper mapper, IConfiguration configuration,
            ILogger<IndexationService> logger)
        {
            _memoryCache = memoryCache;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;

            increment = ulong.Parse(_configuration["BlockRange"]);

            var url = _configuration["RpcUrl"];
            // private key from nethereum exemple don't use it as personnal wallet
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var account = new Account(privateKey);
            _web3 = new Web3(account, url);
        }

        public async Task<bool> IndexData()
        {


            ulong toBlock = startBlock + increment;

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

            startBlock += increment;

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

