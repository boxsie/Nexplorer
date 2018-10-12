using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Data.Context;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Queries
{
    public class UserQuery
    {
        private readonly NexplorerDb _nexplorerDb;
        private readonly AddressQuery _addressQuery;
        private readonly IMapper _mapper;

        public UserQuery(NexplorerDb nexplorerDb, AddressQuery addressQuery, IMapper mapper)
        {
            _nexplorerDb = nexplorerDb;
            _addressQuery = addressQuery;
            _mapper = mapper;
        }

        public async Task UpdateAccount(string userId, Currency currency)
        {
            var acc = await _nexplorerDb.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (acc == null)
                return;

            acc.Currency = currency;

            await _nexplorerDb.SaveChangesAsync();
        }

        public async Task<List<FavouriteAddressDto>> GetFavouriteAddressesAsync(string userId)
        {
            var faveAdds = await _nexplorerDb
                .FavouriteAddresses
                .Include(x => x.User)
                .Where(x => x.User.Id == userId)
                .Select(x => _mapper.Map<FavouriteAddressDto>(x))
                .ToListAsync();

            foreach (var faveAdd in faveAdds)
                faveAdd.AddressDto = await _addressQuery.GetAddressLiteAsync(faveAdd.AddressId, null);

            return faveAdds;
        }
        
        public async Task<bool> IsUserFavouriteAsync(int addressId, string userId)
        {
            return await _nexplorerDb
                .FavouriteAddresses
                .Include(x => x.User)
                .AnyAsync(x => x.User.Id == userId && x.AddressId == addressId);
        }
    }
}
