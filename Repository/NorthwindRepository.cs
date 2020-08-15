using CRUDExample.Models;
using Dapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CRUDExample.Repository
{
    public class NorthwindRepository
    {
        private readonly string _connectionString;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public NorthwindRepository()
        {
            _connectionString = ConfigurationManager.AppSettings["DBSetting"];
        }

        private SqlConnection GetOpenConnection()
        {
            return new SqlConnection(this._connectionString);
        }

        public async Task<List<ProductsModel>> GetProductList()
        {
            using (var cn = GetOpenConnection())
            {
                var result = await cn.QueryAsync<ProductsModel>
                    (@" SELECT [ProductID]
                          ,[ProductName]
                          ,[SupplierID]
                          ,[CategoryID]
                          ,[QuantityPerUnit]
                          ,[UnitPrice]
                          ,[UnitsInStock]
                          ,[UnitsOnOrder]
                          ,[ReorderLevel]
                          ,[Discontinued]
                      FROM [Northwind].[dbo].[Products]
                      ORDER BY [ProductID] DESC  "
                     ).ConfigureAwait(continueOnCapturedContext: false);

                return result.ToList();
            }
        }

        public async Task<bool> InsertProduct(ProductsModel Products)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var result = await cn.ExecuteAsync(
                        @"INSERT INTO [Northwind].[dbo].[Products]
                          ( [ProductName]
                              ,[SupplierID]
                              ,[CategoryID]
                              ,[QuantityPerUnit]
                              ,[UnitPrice]
                              ,[UnitsInStock]
                              ,[UnitsOnOrder]
                              ,[ReorderLevel]
                              ,[Discontinued])
                          VALUES(@Name,1,1,99,@Price,99,99,99,99)",
                          //請原諒我偷懶不想用那麼多欄位作範例XD
                        new { Name = Products.ProductName, Price = Products.UnitPrice }
                        );
                    return result > 0;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"InsertProduct:{e.Message}");
            }
            return false;
        }

        public async Task<bool> UpdatePrice(int ProductID, decimal Price)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var check = await cn.QueryAsync<string>(
                        @"SELECT [UnitPrice] FROM [Products] WHERE [ProductID] = @PID",
                        new { PID = ProductID }
                        );
                    if (check.ToList().Count != 1) return false;
                    var result = await cn.ExecuteAsync(
                        @"UPDATE [Northwind].[dbo].[Products] SET [UnitPrice]=@Price WHERE [ProductID] = @PID",
                        new { PID = ProductID, Price = Price }
                         ).ConfigureAwait(continueOnCapturedContext: false);

                    return result > 0;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"UpdatePrice:{e.Message}");
            }
            return false;
        }
    }
}