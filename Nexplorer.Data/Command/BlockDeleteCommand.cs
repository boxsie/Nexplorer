using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nexplorer.Config;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;

namespace Nexplorer.Data.Command
{
    public class BlockDeleteCommand
    {
        private const string TxInOutDeleteSql = @"
            DELETE tIo 
            FROM [dbo].[TransactionInputOutput] tIo
            INNER JOIN [dbo].[Transaction] t on t.[TransactionId] = tIo.[TransactionId]
            WHERE t.[BlockHeight] = @Height";

        private const string AddressDeleteSql = @"
            DELETE FROM [dbo].[Address] WHERE [dbo].[Address].[AddressId] = @AddressId";

        private const string BlockDeleteSql = @"
            DELETE FROM [dbo].[Block] WHERE [dbo].[Block].[Height] = @Height";

        private const string AddressSelectSql = @"
            SELECT 
                *
            FROM [dbo].[Address] a
            WHERE a.[AddressId] = @AddressId";

        private const string AddressFirstBlockSql = @"
            SELECT 
                MIN(t.[BlockHeight])
            FROM [dbo].[TransactionInputOutput] tIo
            INNER JOIN [dbo].[Transaction] t on t.[TransactionId] = tIo.[TransactionId]
            WHERE t.[BlockHeight] <> @Height
            AND tIo.[AddressId] = @AddressId";

        private const string AddressUpdateSql = @"
            UPDATE [dbo].[Address]  
            SET                  
                [dbo].[Address].[FirstBlockHeight] = @FirstBlockHeight
            WHERE [dbo].[Address].[AddressId] = @AddressId";

        public async Task DeleteBlockAsync(BlockDto blockDto)
        {
            using (var con = new SqlConnection(Settings.Connection.GetNexusDbConnectionString()))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    await con.ExecuteAsync(TxInOutDeleteSql, new { blockDto.Height }, trans);

                    await DeleteOrUpdateAddressesAsync(con, trans, blockDto);

                    await con.ExecuteAsync(BlockDeleteSql, new { blockDto.Height }, trans);

                    trans.Commit();
                }
            }
        }

        private static async Task DeleteOrUpdateAddressesAsync(IDbConnection con, IDbTransaction trans, BlockDto blockDto)
        {
            if (blockDto?.Transactions == null || !blockDto.Transactions.Any())
                return;

            var addressIds = blockDto.Transactions
                .SelectMany(x => x.Inputs.Concat(x.Outputs)
                    .Select(y => y.AddressId));

            foreach (var addressId in addressIds)
            {
                var address = (await con.QueryAsync<Address>(AddressSelectSql, new { AddressId = addressId }, trans)).FirstOrDefault();

                if (address == null || address.FirstBlockHeight != blockDto.Height)
                    continue;

                var newFirstBlockHeight = (await con.QueryAsync<int?>(AddressFirstBlockSql, new { blockDto.Height, AddressId = addressId }, trans)).FirstOrDefault();

                if (newFirstBlockHeight.HasValue)
                {
                    address.FirstBlockHeight = newFirstBlockHeight.Value;
                    await con.ExecuteAsync(AddressUpdateSql, new { address.FirstBlockHeight, AddressId = addressId }, trans);
                }
                else
                {
                    await con.ExecuteAsync(AddressDeleteSql, new { AddressId = addressId }, trans);
                }
            }
        }
    }
}