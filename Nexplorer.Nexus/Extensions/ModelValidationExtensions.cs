using System;
using Nexplorer.Nexus.Accounts.Models;
using Nexplorer.Nexus.Assets.Models;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Extensions
{
    public static class ModelValidationExtensions
    {
        public static void Validate(this NexusUserCredential userCredential)
        {
            if (userCredential == null)
                throw new ArgumentException("User credential is required");
            
            if (string.IsNullOrWhiteSpace(userCredential.Username))
                throw new ArgumentException("Username is required");

            if (string.IsNullOrWhiteSpace(userCredential.Password))
                throw new ArgumentException("Password is required");

            if (!userCredential.Pin.HasValue)
                throw new ArgumentException("PIN is required");
        }

        public static void Validate(this NexusUser user)
        {
            if (user == null)
                throw new ArgumentException("User is required");
            
            if (string.IsNullOrWhiteSpace(user.GenesisId?.Session))
                throw new ArgumentException("A session key is required");
        }
        
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