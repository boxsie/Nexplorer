using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Command
{
    public class AddressAggregateUpdateCommand
    {
        private readonly Dictionary<int, AddressAggregate> _addressAggregates;

        public AddressAggregateUpdateCommand()
        {
            _addressAggregates = new Dictionary<int, AddressAggregate>();
        }
        
        public async Task UpdateAsync(NexusDb context, int addressId, TransactionType txType, double amount, Block nextBlock)
        {
            AddressAggregate addAgg;

            if (_addressAggregates.ContainsKey(addressId))
            {
                addAgg = _addressAggregates[addressId];

                addAgg.ModifyAggregateProperties(txType, amount, nextBlock);
            }
            else
            {
                addAgg = await context.AddressAggregates
                    .Include(x => x.LastBlock)
                    .FirstOrDefaultAsync(x => x.AddressId == addressId);

                if (addAgg == null)
                {
                    addAgg = new AddressAggregate { AddressId = addressId };

                    await context.AddressAggregates.AddAsync(addAgg);
                }

                addAgg.ModifyAggregateProperties(txType, amount, nextBlock);

                _addressAggregates.Add(addAgg.AddressId, addAgg);
            }
        }

        public void Reset()
        {
            _addressAggregates.Clear();
        }
    }
}