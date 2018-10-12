using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Web.Extensions;
using Nexplorer.Web.Models;
using Nexplorer.Web.Queries;

namespace Nexplorer.Web.Controllers
{
    [Authorize]
    public class FavouritesController : WebControllerBase
    {
        private readonly NexplorerDb _nexplorerDb;
        private readonly UserQuery _userQuery;
        private readonly AddressFavouriteCommand _addressFavourite;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavouritesController(NexplorerDb nexplorerDb, UserQuery userQuery, AddressFavouriteCommand addressFavourite, UserManager<ApplicationUser> userManager)
        {
            _nexplorerDb = nexplorerDb;
            _userQuery = userQuery;
            _addressFavourite = addressFavourite;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            return View(new FavouriteIndexViewModel
            {
                FavouriteAddresses = await _userQuery.GetFavouriteAddressesAsync(user.Id)
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAddressFavourite(int addressId)
        {
            if (!User.Identity.IsAuthenticated)
                return NotFound("User not found.");

            var user = await _userManager.GetUserAsync(User);

            if (await _userQuery.IsUserFavouriteAsync(addressId, user.Id))
                return Ok();

            return Ok(await _addressFavourite.CreateAsync(user.Id, addressId));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveAddressFavourite(int addressId)
        {
            if (!User.Identity.IsAuthenticated)
                return NotFound("User not found.");
            
            var user = await _userManager.GetUserAsync(User);

            if (!await _userQuery.IsUserFavouriteAsync(addressId, user.Id))
                return NotFound("Address favourite not found.");

            await _addressFavourite.RemoveAsync(user.Id, addressId);

            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetAddressAlias(int favouriteAddressId, string alias)
        {
            if (!User.Identity.IsAuthenticated)
                return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(alias))
                alias = null;
            else if (alias.Length > FavouriteAddress.AliasMaxLength)
                return BadRequest($"Alias must be less than {FavouriteAddress.AliasMaxLength} characters.");

            var user = await _userManager.GetUserAsync(User);

            await _addressFavourite.SetAliasAsync(user.Id, favouriteAddressId, alias);

            return Ok();
        }
    }
}