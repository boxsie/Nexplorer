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
    public class BlockAddCommand
    {
        private readonly NexusDb _nexusDb;
        private readonly TransactionInputOutputMapper _txInOutMapper;
        private readonly IMapper _mapper;

        public BlockAddCommand(NexusDb nexusDb, TransactionInputOutputMapper txInOutMapper, IMapper mapper)
        {
            _nexusDb = nexusDb;
            _txInOutMapper = txInOutMapper;
            _mapper = mapper;
        }

        public async Task AddBlocksAsync(List<BlockDto> blockDtos)
        {
            _txInOutMapper.Reset();

            foreach (var blockDto in blockDtos)
            {
                var block = _mapper.Map<BlockDto, Block>(blockDto);

                foreach (var txDto in blockDto.Transactions)
                {
                    var tx = block.Transactions.FirstOrDefault(x => x.Hash == txDto.Hash);

                    if (tx == null)
                        return;

                    tx.Inputs = await _txInOutMapper.MapTransactionInputOutput<TransactionInput>(_nexusDb, txDto.Inputs, block, tx);
                    tx.Outputs = await _txInOutMapper.MapTransactionInputOutput<TransactionOutput>(_nexusDb, txDto.Outputs, block, tx);
                }

                _nexusDb.Blocks.Add(block);
            }

            await _nexusDb.SaveChangesAsync();
        }

        public async Task AddOrphansAsync(List<BlockDto> orphanBlockDtos)
        {
            var orphans = orphanBlockDtos.Select(x => _mapper.Map<OrphanBlock>(x));

            await _nexusDb.OrphanBlocks.AddRangeAsync(orphans);
            await _nexusDb.SaveChangesAsync();
        }
    }
}
