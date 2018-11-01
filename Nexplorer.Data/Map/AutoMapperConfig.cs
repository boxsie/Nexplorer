using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Boxsie.DotNetNexusClient.Response;
using Nexplorer.Core;
using Nexplorer.Data.Api;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Entity.Orphan;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Domain.Enums;
using Nexplorer.Infrastructure.Geolocate;

namespace Nexplorer.Data.Map
{
    public class AutoMapperConfig
    {
        public IMapper GetMapper()
        {
            var config = new MapperConfiguration(x =>
            {
                x.CreateMap<Block, BlockDto>()
                    .ForMember(d => d.NextBlockHash, o => o.Ignore())
                    .ForMember(d => d.PreviousBlockHash, o => o.Ignore());
                x.CreateMap<Transaction, TransactionDto>()
                    .ForMember(d => d.BlockHeight, o => o.MapFrom(s => s.Block.Height))
                    .ForMember(d => d.Confirmations, o => o.Ignore())
                    .ForMember(d => d.Inputs, o => o.MapFrom(s => s.InputOutputs.Where(y => y.TransactionType == TransactionType.Input)))
                    .ForMember(d => d.Outputs, o => o.MapFrom(s => s.InputOutputs.Where(y => y.TransactionType == TransactionType.Output)));
                x.CreateMap<TransactionInputOutput, TransactionInputOutputLiteDto>()
                    .ForMember(d => d.AddressHash, o => o.MapFrom(s => s.Address.Hash));

                x.CreateMap<BlockDto, OrphanBlock>()
                    .ForMember(d => d.BlockId, o => o.Ignore());
                x.CreateMap<TransactionDto, OrphanTransaction>()
                    .ForMember(d => d.BlockHeight, o => o.MapFrom(s => s.BlockHeight))
                    .ForMember(d => d.OrphanBlock, o => o.Ignore());

                x.CreateMap<BlockResponse, BlockDto>()
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Time))
                    .ForMember(d => d.Transactions, o => o.Ignore());

                x.CreateMap<TransactionResponse, TransactionDto>()
                    .ForMember(d => d.Hash, o => o.MapFrom(s => s.TxId))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => Helpers.ToDateTime(s.Time)))
                    .ForMember(d => d.Inputs, o => o.MapFrom(s => MapTxInOutDto(s.Inputs)))
                    .ForMember(d => d.Outputs, o => o.MapFrom(s => MapTxInOutDto(s.Outputs)))
                    .ForMember(d => d.TransactionId, o => o.Ignore())
                    .ForMember(d => d.BlockHeight, o => o.Ignore());

                x.CreateMap<InfoResponse, NexusInfoDto>()
                    .ForMember(d => d.TimeStampUtc, o => o.MapFrom(s => Helpers.ToDateTime(s.TimeStamp)));

                x.CreateMap<PeerInfoResponse, PeerInfoDto>()
                    .ForMember(d => d.Address, o => o.MapFrom(s => s.IpAddress.Split(':', StringSplitOptions.None).FirstOrDefault() ?? "Unknown"))
                    .ForMember(d => d.VersionInfo, o => o.MapFrom(s => s.SubVersion))
                    .ForMember(d => d.ChainHeight, o => o.MapFrom(s => s.Height))
                    .ForMember(d => d.ConnectionTime, o => o.MapFrom(s => Helpers.ToDateTime(s.ConnectionTime)))
                    .ForMember(d => d.LastSendTime, o => o.MapFrom(s => Helpers.ToDateTime(s.LastSendTime)))
                    .ForMember(d => d.LastReceiveTime, o => o.MapFrom(s => Helpers.ToDateTime(s.LastReceiveTime)))
                    .ForMember(d => d.Geolocation, o => o.Ignore());

                x.CreateMap<GeolocateResponse, GeolocationDto>();

                x.CreateMap<MiningInfoResponse, MiningInfoDto>()
                    .ForMember(d => d.TimeStampUtc, o => o.MapFrom(s => Helpers.ToDateTime(s.TimeStampUtc)))
                    .ForMember(d => d.CreatedOn, o => o.MapFrom(s => DateTime.UtcNow));

                x.CreateMap<SupplyRatesResponse, SupplyRateDto>()
                    .ForMember(d => d.CreatedOn, o => o.MapFrom(s => DateTime.UtcNow));

                x.CreateMap<TrustKeyAddressResponse, TrustKeyResponseDto>()
                    .ForMember(d => d.TransactionHash, o => o.MapFrom(s => s.Key.TransactionHash))
                    .ForMember(d => d.Expired, o => o.MapFrom(s => s.Key.Expired))
                    .ForMember(d => d.GenesisBlockHash, o => o.MapFrom(s => s.Key.GenesisBlockHash))
                    .ForMember(d => d.TimeSinceLastBlock, o => o.MapFrom(s => s.Key.TimeSinceLastBlock))
                    .ForMember(d => d.TimeUtc, o => o.MapFrom(s => s.Key.TimeUtc))
                    .ForMember(d => d.TrustHash, o => o.MapFrom(s => s.Key.TrustHash))
                    .ForMember(d => d.TrustKey, o => o.MapFrom(s => s.Key.TrustKey))
                    .ForMember(d => d.TrustKeyAge, o => o.MapFrom(s => s.Key.TrustKeyAge));

                x.CreateMap<FavouriteAddress, FavouriteAddressDto>()
                    .ForMember(d => d.AddressDto, o => o.Ignore());

                x.CreateMap<AddressesCriteria, AddressFilterCriteria>()
                    .ForMember(d => d.OrderBy, o => o.MapFrom(s => s.OrderBy))
                    .ForMember(d => d.MinBalance, o => o.MapFrom(s => s.BalanceFrom))
                    .ForMember(d => d.MaxBalance, o => o.MapFrom(s => s.BalanceTo))
                    .ForMember(d => d.HeightFrom, o => o.MapFrom(s => s.LastBockHeightFrom))
                    .ForMember(d => d.HeightTo, o => o.MapFrom(s => s.LastBlockHeightTo));
                    ;
                x.CreateMap<AddressLiteDto, FilteredAddress>()
                    .ForMember(d => d.FirstBlockHeight, o => o.MapFrom(s => s.FirstBlockSeen))
                    .ForMember(d => d.LastBlockHeight, o => o.MapFrom(s => s.LastBlockSeen));
            });

            config.AssertConfigurationIsValid();

            return config.CreateMapper();
        }

        private static List<TransactionInputOutputLiteDto> MapTxInOutDto(IEnumerable<string> rawInputOutput)
        {
            var txIos = new List<TransactionInputOutputLiteDto>();

            if (rawInputOutput == null)
                return txIos;

            foreach (var s in rawInputOutput)
            {
                var split = s.Split(':');
                var hash = split.Length > 0 ? split[0] : "";
                var hasAmount = double.TryParse(split.Length > 1 ? split[1] : "", out var amountD);

                txIos.Add(new TransactionInputOutputLiteDto
                {
                    Amount = hasAmount ? amountD : 0,
                    AddressHash = hash
                });
            }

            return txIos;
        }
    }
}