using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Entity.User;

namespace Nexplorer.Data.Command
{
    public class AddressFavouriteCommand
    {
        private readonly NexplorerDb _nexplorerDb;
        private readonly NexusDb _nexusDb;

        public AddressFavouriteCommand(NexplorerDb nexplorerDb, NexusDb nexusDb)
        {
            _nexplorerDb = nexplorerDb;
            _nexusDb = nexusDb;
        }

        public async Task<int> CreateAsync(string userId, int addressId)
        {
            var user = await _nexplorerDb.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new ApplicationException($"Address watch failed because user {userId} cannot be found.");

            var address = await _nexusDb.Addresses.Where(x => x.AddressId == addressId).Select(x => x.AddressId).FirstOrDefaultAsync();

            if (address == 0)
                throw new ApplicationException($"Address watch failed because address {addressId} cannot be found.");
            
            await _nexplorerDb.FavouriteAddresses.AddAsync(new FavouriteAddress
            {
                User = user,
                AddressId = address
            });

            return await _nexplorerDb.SaveChangesAsync();
        }

        public async Task RemoveAsync(string userId, int addressId)
        {
            var watcher = await _nexplorerDb.FavouriteAddresses
                .Include(x => x.User)
                .Where(x => x.User.Id == userId && x.AddressId == addressId).FirstOrDefaultAsync();

            if (watcher != null)
            {
                _nexplorerDb.FavouriteAddresses.Remove(watcher);

                await _nexplorerDb.SaveChangesAsync();
            }
        }

        public async Task SetAliasAsync(string userId, int favouriteAddressId,  string alias)
        {
            var favourite = await _nexplorerDb
                .FavouriteAddresses
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.FavouriteAddressId == favouriteAddressId && x.User.Id == userId);

            if (favourite == null)
                throw new ApplicationException($"Set alias failed because favourite address { favouriteAddressId } cannot be found.");

            favourite.Alias = alias;

            await _nexplorerDb.SaveChangesAsync();
        }
    }
}
