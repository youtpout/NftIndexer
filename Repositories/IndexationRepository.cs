using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.BlockchainProcessing.BlockStorage.Repositories;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC721;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NftIndexer.Entities;
using NftIndexer.Interfaces;

namespace NftIndexer.Repositories
{
    public class IndexationRepository : IIndexationRepository
    {
        private const string addressZero = "0x0000000000000000000000000000000000000000";
        private readonly IConfiguration _configuration;
        private readonly Web3 _web3;
        private readonly string _ipfsGateway;
        private readonly ILogger<IndexationRepository> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Interfaces.IContractRepository _contractRepository;
        private readonly NftIndexerContext _dbContext;


        public IndexationRepository(IConfiguration configuration, NftIndexerContext dbContext, ILogger<IndexationRepository> logger,
            Interfaces.IContractRepository contractRepository)
        {
            _configuration = configuration;
            _ipfsGateway = _configuration["IpfsGateway"];
            _logger = logger;
            _dbContext = dbContext;
            _contractRepository = contractRepository;
            var url = _configuration["RpcUrl"];
            // private key from nethereum exemple don't use it as personnal wallet
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var account = new Account(privateKey);
            _web3 = new Web3(account, url);
        }


        public async Task<bool> SaveERC721(List<EventLog<TransferEventDTO>> events)
        {

            var listAddress = events.Select(x => x.Log.Address.ToLower()).Distinct();
            List<TokenHistory> histories = new List<TokenHistory>();
            var allContracts = await _dbContext.Contracts.Where(l => listAddress.Contains(l.Address)).ToListAsync();


            var newAddressContracts = listAddress.Where(l => !allContracts.Any(x => x.Address == l)).ToList();
            var erc721Service = new ERC721Service(_web3.Eth);

            var newTokens = new List<Token>();
            var updateTokens = new List<Token>();
            var newHistories = new List<TokenHistory>();
            var newContracts = new List<Entities.Contract>();

            await Parallel.ForEachAsync(newAddressContracts, async (address, canc) =>
             {
                 var contractService = erc721Service.GetContractService(address);
                 string name = string.Empty;
                 string symbol = string.Empty;
                 try
                 {
                     name = await contractService.NameQueryAsync();
                     symbol = await contractService.SymbolQueryAsync();
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, $"Error Get Name or Symbol contract {address}");
                 }
                 var findContract = new Entities.Contract() { Address = address, ContractType = "ERC721", Name = name, Symbol = symbol };
                 newContracts.Add(findContract);

             });


            // add new contract to database
            newContracts.ForEach(con =>
            {
                _dbContext.Contracts.Add(con);
                _dbContext.SaveChanges();
            });

            allContracts = await _dbContext.Contracts.Where(l => listAddress.Contains(l.Address)).ToListAsync();
            var allTokens = await _dbContext.Tokens.Where(l => listAddress.Contains(l.Contract.Address)).ToListAsync();


            await Parallel.ForEachAsync(events, async (item, canc) =>
            {
                var address = item.Log.Address.ToLower();
                var findContract = newContracts.Where(a => a.Address == address).FirstOrDefault();

                var contractService = erc721Service.GetContractService(address);

                var findToken = allTokens.Where(a => a.Contract.Address == address && a.TokenId == item.Event.TokenId).FirstOrDefault();
                if (findToken == null)
                {
                    var contract = allContracts.Where(x => x.Address == address).First();
                    findToken = new Token() { TokenId = item.Event.TokenId, ContractId = contract.Id, Contract = contract };

                    newTokens.Add(findToken);
                }
                else
                {
                    updateTokens.Add(findToken);
                }

                // get uri for the token id at the block event
                var uri = await contractService.TokenURIQueryAsync(item.Event.TokenId, new BlockParameter(item.Log.BlockNumber));
                findToken.Uri = uri;

                var history = new TokenHistory()
                {
                    Amount = 1,
                    BlockHash = item.Log.BlockHash,
                    BlockNumber = ((long)item.Log.BlockNumber.Value),
                    From = item.Event.From.ToLower(),
                    To = item.Event.To.ToLower(),
                    Time = DateTime.UtcNow,
                    LogIndex = ((long)item.Log.LogIndex.Value),
                    Uri = uri,
                    TransactionHash = item.Log.TransactionHash,
                    TransactionIndex = ((long)item.Log.TransactionIndex.Value),
                    Token = findToken
                };
                if (!string.IsNullOrWhiteSpace(uri))
                {

                    var metadata = await GetMetadata(uri);
                    if (metadata.Item2)
                    {
                        history.Metadatas = metadata.Item1;
                        findToken.Metadatas = metadata.Item1;
                    }
                    else
                    {
                        history.Error = metadata.Item1;
                    }
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
                history.Token = findToken;
                newHistories.Add(history);

                //_logger.LogInformation($"Erc721 token id {item.Event.TokenId} uri {uri} metadata {metadata}");
            });

            newTokens.ForEach(con =>
            {
                _dbContext.Tokens.Add(con);
                _dbContext.SaveChanges();
            });

            updateTokens.ForEach(con =>
            {
                _dbContext.Tokens.Update(con);
                _dbContext.SaveChanges();
            });

            newHistories.ForEach(con =>
            {
                var token = _dbContext.Tokens.Where(x => x.TokenId == con.Token.TokenId && x.Contract.Address == con.Token.Contract.Address).First();
                con.TokenId = token.Id;
                _dbContext.TokenHistories.Add(con);
                _dbContext.SaveChanges();
            });


            return true;
        }

        private async Task<Tuple<string, bool>> GetMetadata(string uri)
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
                _logger.LogError(ex, $"Error GetMetadata uri {uri}");
                return new Tuple<string, bool>(ex.Message, false);
            }


        }

    }
}

