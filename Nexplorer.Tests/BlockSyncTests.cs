using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace Nexplorer.Tests
{
    public class BlockSyncTests : IClassFixture<BlockSyncFixture>
    {
        private readonly BlockSyncFixture _fixture;
        private readonly ITestOutputHelper _output;

        public BlockSyncTests(BlockSyncFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }
        
        [Fact]
        public async Task BlockSync_InsertAndRevertRandomBlock_InsertsAndRevertsBlock()
        {
            // GET BLOCK
            var randomHash = await GetRandomBlockHashAsync();
            var randomBlock = await _fixture.NexusQuery.GetBlockAsync(randomHash, true);
            
            Assert.True(randomBlock != null);

            _output.WriteLine($"Block {randomBlock.Height} was returned");

            // INSERT BLOCK
            var block = await _fixture.InsertCommand.InsertBlockAsync(randomBlock);

            var blockDto = await _fixture.BlockQuery.GetBlockAsync(block.Height, true);

            Assert.True(block != null);
            Assert.True(block.Height == blockDto.Height);
            Assert.True(block.Hash == blockDto.Hash);
            Assert.True(block.Transactions.Count == blockDto.Transactions.Count);
            Assert.True(block.Transactions.All(x => blockDto.Transactions.Any(y => x.Hash == y.Hash)));
            
            _output.WriteLine($"Block {block.Height} was inserted successfully");

            // AGGREGATE ADDRESS
            await AggregateAddresses(block, blockDto);

            // REVERT ADDRESS AGGREGATE
            await RevertAddressAggregate(block, blockDto);

            // DELETE BLOCK
            await _fixture.DeleteCommand.DeleteBlockAsync(blockDto);

            blockDto = await _fixture.BlockQuery.GetBlockAsync(block.Height, true);

            Assert.True(blockDto == null);

            _output.WriteLine($"Block {block.Height} was deleted");
        }

        private async Task<string> GetRandomBlockHashAsync()
        {
            return await _fixture.Client.GetBlockHashAsync(
                new Random().Next(
                    await _fixture.Client.GetBlockCountAsync()));
        }

        private async Task<List<AddressDto>> GetAddressesAsync(Block block)
        {
            var addressIds = block.Transactions
                .SelectMany(x => x.InputOutputs
                    .Select(y => y.AddressId)
                    .Distinct())
                .ToList();

            var addresses = new List<AddressDto>();

            foreach (var addressId in addressIds)
                addresses.Add(await _fixture.AddressQuery.GetAddressAsync(addressId));

            return addresses;
        }

        private async Task AggregateAddresses(Block block, BlockDto blockDto)
        {
            var addressesBefore = await GetAddressesAsync(block);

            await _fixture.AddressAggregator.AggregateAddressesAsync(blockDto);

            var addressesAfter = await GetAddressesAsync(block);

            CompareAddresses(blockDto, addressesBefore, addressesAfter);
        }

        private async Task RevertAddressAggregate(Block block, BlockDto blockDto)
        {
            var addressesBefore = await GetAddressesAsync(block);

            await _fixture.AddressAggregator.RevertAggregate(blockDto);

            var addressesAfter = await GetAddressesAsync(block);

            CompareAddresses(blockDto, addressesBefore, addressesAfter);
        }

        private void CompareAddresses(BlockDto blockDto, List<AddressDto> addressesBefore, List<AddressDto> addressesAfter)
        {
            foreach (var before in addressesBefore)
            {
                var after = addressesAfter.FirstOrDefault(x => x.AddressId == before.AddressId);

                Assert.True(after != null);

                var balanceDelta = blockDto.Transactions
                    .Select(x => x.Inputs.Concat(x.Outputs)
                        .Where(y => y.AddressId == before.AddressId)
                        .Sum(y => y.TransactionInputOutputType == TransactionInputOutputType.Input
                            ? -y.Amount
                            : y.Amount))
                    .Sum();

                var diff = Math.Abs(before.Balance - after.Balance);

                _output.WriteLine($"\r\nAddress ID {before.AddressId}:");
                _output.WriteLine($"prev: {before.Balance}");
                _output.WriteLine($"txs: {balanceDelta}");
                _output.WriteLine($"new: {after.Balance}");
                _output.WriteLine($"diff: {diff}");
                _output.WriteLine($"\r\n");

                Assert.True(Math.Abs(diff - Math.Abs(balanceDelta)) < 0.000000001);
            }
        }
    }
}
