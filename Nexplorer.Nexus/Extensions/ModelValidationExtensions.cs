using System;
using Nexplorer.Nexus.Assets.Models;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Extensions
{
    public static class ModelValidationExtensions
    {
        public static void Validate(this Asset asset)
        {
            if (asset == null)
                throw new ArgumentException("Asset is required");

            if (string.IsNullOrWhiteSpace(asset.Name) && string.IsNullOrWhiteSpace(asset.Data))
                throw new ArgumentException("Name and/or data is required");
        }

        public static void Validate(this Token token)
        {
            if (token == null)
                throw new ArgumentException("Token is required");

            if (string.IsNullOrWhiteSpace(token.Name) && string.IsNullOrWhiteSpace(token.Address))
                throw new ArgumentException("Name and/or address is required");
        }
    }
}