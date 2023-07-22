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
using NftIndexer.Interfaces;

namespace NftIndexer.Services
{
    public class IndexationService : IIndexationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexationService> _logger;
        private readonly string _ipfsGateway;
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
            _ipfsGateway = _configuration["IpfsGateway"];

            var url = _configuration["RpcUrl"];
            // private key from nethereum exemple don't use it as personnal wallet
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var account = new Account(privateKey);
            _web3 = new Web3(account, url);
        }

        public async Task<bool> IndexData()
        {


            ulong toBlock = startBlock + increment;
            var erc721Service = new ERC721Service(_web3.Eth);

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

            Parallel.ForEach(allTransferEventsForContract, async (item) =>
            {
                var contractService = erc721Service.GetContractService(item.Log.Address);
                // get uri for the token id at the block event
                var uri = await contractService.TokenURIQueryAsync(item.Event.TokenId, new BlockParameter(item.Log.BlockNumber));
                var metadata = await GetMetadata(uri);
                _logger.LogInformation($"Erc721 token id {item.Event.TokenId} uri {uri} metadata {metadata}");
            });

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

        public async Task<Tuple<string, bool>> GetMetadata(string uri)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    string url = uri.Replace("ipfs://", _ipfsGateway);
                    var responseMessage = await client.GetAsync(url);


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseContent = await responseMessage.Content.ReadAsStringAsync();
                        return new Tuple<string, bool>(responseContent, true);
                    }
                    else
                    {
                        var responseContent = await responseMessage.Content.ReadAsStringAsync();
                        return new Tuple<string, bool>(responseContent, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetMetadata");
                return new Tuple<string, bool>(ex.Message, false);
            }


        }
    }
}

