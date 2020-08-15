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
                      FROM [Northwind].[dbo].[Products]"
                     ).ConfigureAwait(continueOnCapturedContext: false);

                return result.ToList();
            }
        }

    }
}