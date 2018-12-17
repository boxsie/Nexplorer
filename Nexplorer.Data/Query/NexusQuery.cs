using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Boxsie.DotNetNexusClient.Core;
using Boxsie.DotNetNexusClient.Response;
using Nexplorer.Data.Cache;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Query
{
    public class NexusQuery
    {
        private readonly INexusClient _nxsClient;
        private readonly GeolocationService _geolocationService;
        private readonly IMapper _mapper;

        public NexusQuery(INexusClient nxsClient, GeolocationService geolocationService, IMapper mapper)
        {
            _nxsClient = nxsClient;
            _geolocationService = geolocationService;
            _mapper = mapper;
        }

        public async Task<BlockDto> GetBlockAsync(string hash, bool includeTransactions)
        {
            var blockResponse = await _nxsClient.GetBlockAsync(hash);

            return await MapResponseToDto(blockResponse, includeTransactions);
        }

        public Task<bool> IsBlockHashOnChain(string hash)
        {
            return _nxsClient.IsBlockHashOnChainAsync(hash);
        }

        public async Task<BlockDto> GetBlockAsync(int? height, bool includeTransactions)
        {
            var blockHeight = height ?? await _nxsClient.GetBlockCountAsync();

            var blockHash = await _nxsClient.GetBlockHashAsync(blockHeight);

            if (blockHash == null)
                return null;

            var blockResponse = await _nxsClient.GetBlockAsync(blockHash);

            if (blockResponse.TransactionHash.Any(string.IsNullOrEmpty))
                throw new KeyNotFoundException($"Block height {blockResponse.Height} has a null transaction");
            
            return await MapResponseToDto(blockResponse, includeTransactions);
        }

        public async Task<TransactionDto> GetTransactionAsync(string hash, int? blockHeight)
        {
            var tx = await _nxsClient.GetTransactionAsync(hash);

            var retryCount = 0;

            while (tx == null && retryCount < 3)
            {
                retryCount++;

                await Task.Delay(TimeSpan.FromSeconds(1));

                tx = await _nxsClient.GetTransactionAsync(hash);
            }

            var txDto = tx != null 
                ? _mapper.Map<TransactionDto>(tx)
                : new TransactionDto
                {
                    Hash = hash,
                    Amount = 0,
                    BlockHeight = blockHeight ?? 0,
                    Confirmations = 0
                };

            txDto.BlockHeight = blockHeight ?? 0;
            txDto.Inputs.ForEach(x => x.TransactionInputOutputType = TransactionInputOutputType.Input);
            txDto.Outputs.ForEach(x => x.TransactionInputOutputType = TransactionInputOutputType.Output);

            return txDto;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync(string[] hashes, int? blockHeight)
        {
            var txs = new List<TransactionDto>();

            foreach (var hash in hashes)
            {
                var tx = await GetTransactionAsync(hash, blockHeight);

                txs.Add(tx);
            }

            return txs;
        }

        public async Task<int> GetBlockchainHeightAsync()
        {
            return await _nxsClient.GetBlockCountAsync();
        }

        public async Task<NexusInfoDto> GetInfoAsync()
        {
            var infoResponse = await _nxsClient.GetInfoAsync();

            return _mapper.Map<NexusInfoDto>(infoResponse);
        }

        public async Task<List<PeerInfoDto>> GetPeerInfo()
        {
            var peerResponses = await _nxsClient.GetPeerInfoAsync();
            
            var peerDtos = new List<PeerInfoDto>();

            foreach (var peerResponse in peerResponses)
            {
                var dto = _mapper.Map<PeerInfoDto>(peerResponse);

                dto.Geolocation = _mapper.Map<GeolocationDto>(await _geolocationService.GetGeolocation(dto.Address));

                peerDtos.Add(dto);
            }

            return peerDtos;
        }

        public async Task<MiningInfoDto> GetMiningInfoAsync()
        {
            var miningResponse = await _nxsClient.GetMiningInfoAsync();

            return _mapper.Map<MiningInfoDto>(miningResponse);
        }

        public async Task<SupplyRateDto> GetSupplyRate()
        {
            var supplyResponse = await _nxsClient.GetSupplyRatesAsync();

            return _mapper.Map<SupplyRateDto>(supplyResponse);
        }

        public async Task<List<TrustKeyResponseDto>> GetTrustKeys()
        {
            var trustKeyResponse = await _nxsClient.GetTrustKeysAsync();

            return trustKeyResponse.Keys.Select(x => _mapper.Map<TrustKeyAddressResponse, TrustKeyResponseDto>(x)).ToList();
        }
        
        private async Task<BlockDto> MapResponseToDto(BlockResponse blockResponse, bool includeTransactions)
        {
            var blockDto = _mapper.Map<BlockDto>(blockResponse);

            if (!includeTransactions)
                return blockDto;

            var txResponses = await GetTransactionsAsync(blockResponse.TransactionHash, blockResponse.Height);

            var txs = txResponses
                .Select(x => _mapper.Map<TransactionDto>(x))
                .ToList();

            for (var i = 0; i < txs.Count; i++)
            {
                var tx = txs[i];

                foreach (var txI in tx.Inputs)
                    txI.TransactionInputOutputType = TransactionInputOutputType.Input;

                foreach (var txI in tx.Outputs)
                    txI.TransactionInputOutputType = TransactionInputOutputType.Output;

                switch (i)
                {
                    case 0 when blockDto.Channel == (int)BlockChannels.PoS:
                        if (tx.Inputs.Any() && tx.Outputs.Any() && tx.Outputs.Count == 1)
                        {
                            tx.TransactionType = tx.Inputs.Any(x => tx.Outputs.First().AddressHash == x.AddressHash)
                                ? TransactionType.Coinstake
                                : TransactionType.CoinstakeGenesis;
                        }
                        break;
                    case 0:
                        tx.TransactionType = (BlockChannels) blockDto.Channel == BlockChannels.Hash
                            ? TransactionType.CoinbaseHash
                            : TransactionType.CoinbasePrime;
                        break;
                    default:
                        tx.TransactionType = TransactionType.User;
                        break;
                }
            }

            blockDto.Transactions = txs;

            return blockDto;
        }
    }
}
