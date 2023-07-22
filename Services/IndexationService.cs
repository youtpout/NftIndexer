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
        private readonly IRepositoryBase<Entities.Contract> _contractRepository;
        private readonly IRepositoryBase<Token> _tokenRepository;
        private readonly IRepositoryBase<TokenHistory> _tokenHistoryRepository;
        private readonly ISyncRepository _syncInfoRepository;
        private readonly string _ipfsGateway;
        private static BigInteger lastBlock = 0;

        private const string addressZero = "0x0000000000000000000000000000000000000000";

        private static ulong startBlock = 0;
        private static ulong increment = 100;


        private readonly Web3 _web3;

        public IndexationService(IConfiguration configuration,
            ILogger<IndexationService> logger, IRepositoryBase<Entities.Contract> contractRepository,
            IRepositoryBase<Token> tokenRepository, IRepositoryBase<TokenHistory> tokenHistoryRepository, ISyncRepository syncInfoRepository)
        {
            _configuration = configuration;
            _logger = logger;

            increment = ulong.Parse(_configuration["BlockRange"]);
            _ipfsGateway = _configuration["IpfsGateway"];

            _contractRepository = contractRepository;
            _tokenRepository = tokenRepository;
            _tokenHistoryRepository = tokenHistoryRepository;
            _syncInfoRepository = syncInfoRepository;

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
                lastBlock = await _syncInfoRepository.GetLastBlockIndexed();
                ulong startBlock = ((ulong)lastBlock) + 1;
                ulong toBlock = startBlock + increment;

                var allContracts = await _contractRepository.FindAll();

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


                await SaveERC721(allContracts, allTransferEventsForContract);

                startBlock += increment;

                SyncInfo info = new SyncInfo() { FromBlock = ((long)startBlock), ToBlock = ((long)toBlock), Time = DateTime.UtcNow };
                await _syncInfoRepository.Create(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexData");
                SyncInfo info = new SyncInfo() { FromBlock = ((long)startBlock), ToBlock = 0, Time = DateTime.UtcNow, Error = ex.Message };
                await _syncInfoRepository.Create(info);
            }

            return true;
        }

        private async Task<bool> SaveERC721(IList<Entities.Contract> contracts, List<EventLog<TransferEventDTO>> events)
        {
            List<TokenHistory> histories = new List<TokenHistory>();
            var allTokens = await _tokenRepository.FindAll();

            var erc721Service = new ERC721Service(_web3.Eth);
            Parallel.ForEach(events, async (item) =>
            {
                var address = item.Log.Address.ToLower();
                var findContract = contracts.Where(a => a.Address == address).FirstOrDefault();

                var contractUpdated = new List<Entities.Contract>();

                var contractService = erc721Service.GetContractService(item.Log.Address);
                if (findContract == null)
                {
                    var name = await contractService.NameQueryAsync();
                    var symbol = await contractService.SymbolQueryAsync();
                    findContract = new Entities.Contract() { Address = address, ContractType = "ERC721", Name = name, Symbol = symbol };
                    contracts.Add(findContract);
                }

                contractUpdated.Add(findContract);

                var findToken = allTokens.Where(a => a.Contract.Address == address && a.TokenId == item.Event.TokenId).FirstOrDefault();
                if (findToken == null)
                {
                    findToken = new Token() { TokenId = item.Event.TokenId };
                    findContract.Tokens.Add(findToken);
                }

                // get uri for the token id at the block event
                var uri = await contractService.TokenURIQueryAsync(item.Event.TokenId, new BlockParameter(item.Log.BlockNumber));
                var metadata = await GetMetadata(uri);
                findToken.Uri = uri;

                var history = new TokenHistory()
                {
                    Amount = 1,
                    BlockHash = item.Log.BlockHash,
                    BlockNumber = ((long)item.Log.BlockNumber.Value),
                    From = item.Event.From.ToLower(),
                    To = item.Event.To.ToLower(),
                    Time = DateTime.Now,
                    LogIndex = ((long)item.Log.LogIndex.Value),
                    Uri = uri,
                    TransactionHash = item.Log.TransactionHash,
                    TransactionIndex = ((long)item.Log.TransactionIndex.Value)
                };
                if (metadata.Item2)
                {
                    history.Metadatas = metadata.Item1;
                    findToken.Metadatas = metadata.Item1;
                }
                else
                {
                    history.Error = metadata.Item1;
                }

                if (history.From == addressZero)
                {
                    history.EventType = "Mint";
                }
                else if (history.To == addressZero)
                {
                    history.EventType = "Burn";
                }
                else
                {
                    history.EventType = "Transfer";
                }

                findToken.TokenHistories.Add(history);

                contractUpdated.Where(c => c != null).ToList().ForEach(async (item) =>
                {
                    if (item.Id > 0)
                    {
                        await _contractRepository.Update(item);
                    }
                    else
                    {
                        await _contractRepository.Create(item);
                    }
                });

                _logger.LogInformation($"Erc721 token id {item.Event.TokenId} uri {uri} metadata {metadata}");
            });

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

