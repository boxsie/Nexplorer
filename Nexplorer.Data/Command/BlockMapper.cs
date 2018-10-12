using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Entity.Orphan;

namespace Nexplorer.Data.Command
{
    public class BlockMapper
    {
        private readonly TransactionInputOutputMapper _txInOutMapper;
        private readonly IMapper _mapper;

        public BlockMapper(TransactionInputOutputMapper txInOutMapper, IMapper mapper)
        {
            _txInOutMapper = txInOutMapper;
            _mapper = mapper;
        }

        public async Task<List<Block>> MapBlocksAsync(IEnumerable<BlockDto> blockDtos)
        {
            _txInOutMapper.Reset();

            var blocks = new List<Block>();

            foreach (var blockDto in blockDtos)
            {
                var block = _mapper.Map<BlockDto, Block>(blockDto);

                foreach (var txDto in blockDto.Transactions)
                {
                    var tx = block.Transactions.FirstOrDefault(x => x.Hash == txDto.Hash);

                    if (tx == null)
                        throw new NullReferenceException("Block mapper transaction is null");

                    tx.Inputs = await _txInOutMapper.MapTransactionInputOutput<TransactionInput>(txDto.Inputs, block, tx);
                    tx.Outputs = await _txInOutMapper.MapTransactionInputOutput<TransactionOutput>(txDto.Outputs, block, tx);
                }

                blocks.Add(block);
            }

            return blocks;
        }

        public List<OrphanBlock> MapOrphansAsync(IEnumerable<BlockDto> orphanBlockDtos)
        {
            return orphanBlockDtos.Select(x => _mapper.Map<OrphanBlock>(x)).ToList();
        }
    }
}
