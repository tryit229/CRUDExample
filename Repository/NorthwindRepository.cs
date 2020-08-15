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

        public async Task<Response<List<ProductsModel>>> GetProductList()
        {
            try
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

                    return new Response<List<ProductsModel>>()
                    {
                        Success = true,
                        Data = result.ToList()
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"GetProductList:{e.Message}");
                return new Response<List<ProductsModel>>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

        public async Task<Response<int>> InsertProduct(ProductsModel Product)
        {
            try
            {
                if (Product.UnitPrice < 0)
                {
                    //通常還會再拆一層邏輯層，專門處理業務邏輯。
                    //TODO 回傳原因價格不得為負數
                    return new Response<int>()
                    {
                        Success = false,
                        Message = "價格不得小於零"
                    };
                }

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
                        new { Name = Product.ProductName, Price = Product.UnitPrice }
                        );
                    return new Response<int>()
                    {
                        Success = result>0,
                        Data = result
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"InsertProduct:{e.Message}");
                return new Response<int>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

        public async Task<Response<int>> UpdatePrice(int ProductID, decimal Price)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var check = await cn.QueryAsync<string>(
                        @"SELECT [UnitPrice] FROM [Products] WHERE [ProductID] = @PID",
                        new { PID = ProductID }
                        );
                    if (check.ToList().Count != 1)
                    {
                        return new Response<int>()
                        {
                            Success = false,
                            Message = "查無此資料"
                        };
                    }
                    var result = await cn.ExecuteAsync(
                        @"UPDATE [Northwind].[dbo].[Products] SET [UnitPrice]=@Price WHERE [ProductID] = @PID",
                        new { PID = ProductID, Price = Price }
                         ).ConfigureAwait(continueOnCapturedContext: false);

                    return new Response<int>()
                    {
                        Success = result > 0,
                        Data = result
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"UpdatePrice:{e.Message}");
                return new Response<int>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

        public async Task<Response<int>> OffShelf(int ProductID)
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
                    if (check == 0)
                    {
                        return new Response<int>()
                        {
                            Success = false,
                            Message = "未刪除任何資料"
                        };
                    }
                    var result = await cn.ExecuteAsync(
                        @"  
	                        DELETE [Products] WHERE [ProductID] = @PID",
                        new { PID = ProductID }
                         ).ConfigureAwait(continueOnCapturedContext: false);

                    return new Response<int>()
                    {
                        Success = result > 0,
                        Data = result
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"OffShelf:{e.Message}");
                return new Response<int>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}