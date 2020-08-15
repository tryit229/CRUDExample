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
                //TODO Handle Error
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
                //TODO Handle Error
            }
            return false;
        }

        public async Task<bool> OffShelf(int ProductID)
        {
            try
            {
                /* 
                    我不喜歡"真正的"刪除任何資料，以避免日後無法追溯問題，刪除的方式有幾種
                    1. 多一個欄位"IsDelete"來判斷資料是否存在
                    2. 將刪掉的資料搬到另外一張表作紀錄 (我偏好這種，因為可以加速主表的搜尋速度)
                    
                    [Products_Delete] 這張表我除了原先的[Northwind].[dbo].[Products]欄位之外，也加入CreateTime並預設getdate()，資料被異動及搬移的時間都是重要的紀錄
                  
                 */
                using (var cn = GetOpenConnection())
                {
                    var check = await cn.ExecuteAsync(
                        @"	INSERT INTO [Products_Delete](
	                                [ProductID]
                                  ,[ProductName]
                                  ,[SupplierID]
                                  ,[CategoryID]
                                  ,[QuantityPerUnit]
                                  ,[UnitPrice]
                                  ,[UnitsInStock]
                                  ,[UnitsOnOrder]
                                  ,[ReorderLevel]
                                  ,[Discontinued])  
	                            SELECT * FROM [Products] WHERE [ProductID] = @PID",
                        new { PID = ProductID }
                        );
                    if (check == 0) return false;
                    var result = await cn.ExecuteAsync(
                        @"  
	                        DELETE [Products] WHERE [ProductID] = @PID",
                        new { PID = ProductID }
                         ).ConfigureAwait(continueOnCapturedContext: false);

                    return result > 0;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"OffShelf:{e.Message}");
                //TODO Handle Error
            }
            return false;
        }
    }
}