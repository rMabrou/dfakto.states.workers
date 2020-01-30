using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dFakto.States.Workers.Sql;
using dFakto.States.Workers.Sql.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace dFakto.States.Workers.Tests
{
    public class SqlQueryWorkerTests : BaseTests
    {
        private readonly string _tableName = StringUtils.Random(10);
        
        public SqlQueryWorkerTests()
        {
            CreateTable();
        }
        
        [Fact]
        public async Task TestScalarQuery()
        {
            Insert((1, "hello"), (2,"world"));
            
            var sql = Host.Services.GetService<SqlQueryWorker>();

            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var input = new SqlQueryInput();
                input.ConnectionName = database.Name;
                input.Query = "SELECT COUNT(*) FROM " + _tableName;
                input.Type = SqlQueryType.Scalar;
                var output = await sql.DoJsonWork<SqlQueryInput,SqlQueryOutput>(input);
                
                Assert.Equal(2L,((JsonElement)output.Scalar).GetDecimal());
            }
        }
        
        [Fact]
        public async Task TestNonQuery()
        {
            Insert((1, "hello"), (2,"world"));
            
            var sql = Host.Services.GetService<SqlQueryWorker>();

            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var input = new SqlQueryInput();
                input.ConnectionName = database.Name;
                input.Query = "TRUNCATE TABLE " + _tableName;
                input.Type = SqlQueryType.NonQuery;
                var output = await sql.DoJsonWork<SqlQueryInput,SqlQueryOutput>(input);
                
                Assert.Equal(0L,Count(database));
            }
        }
        
        [Fact]
        public async Task TestReader_NoParams()
        {
            Insert((1, "hello"), (2,"world"));
            
            var sql = Host.Services.GetService<SqlQueryWorker>();

            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var input = new SqlQueryInput();
                input.ConnectionName = database.Name;
                input.Query = $"SELECT * FROM {_tableName} ORDER BY COL1";
                input.Type = SqlQueryType.Reader;
                var output = await sql.DoJsonWork<SqlQueryInput,SqlQueryOutput>(input);
                
                Assert.Equal(2,output.Result.Count);
                Assert.Equal(1,((JsonElement)output.Result[0]["col1"]).GetInt32());
                Assert.Equal("hello",((JsonElement)output.Result[0]["col2"]).GetString());
                Assert.Equal(2,((JsonElement)output.Result[1]["col1"]).GetInt32());
                Assert.Equal("world",((JsonElement)output.Result[1]["col2"]).GetString());
            }
        }
        
        [Fact]
        public async Task TestReader_One_Param()
        {
            Insert((1, "hello"), (2,"world"));
            
            var sql = Host.Services.GetService<SqlQueryWorker>();

            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var input = new SqlQueryInput();
                input.ConnectionName = database.Name;
                input.Query = $"SELECT * FROM {_tableName} WHERE col1 = @p1";
                input.Type = SqlQueryType.Reader;

                var para = new SqlQueryParameter
                {
                    Name = "p1",
                    Value = JsonDocument.Parse("2").RootElement
                };
                
                input.Params = new [] {para};
                
                var output = await sql.DoJsonWork<SqlQueryInput,SqlQueryOutput>(input);
                
                Assert.Single(output.Result);
                Assert.Equal("world", ((JsonElement)output.Result[0]["col2"]).GetString());
                Assert.Equal(2, ((JsonElement)output.Result[0]["col1"]).GetInt32());
            }
        }

        public override void Dispose()
        {
            DropTable();
            base.Dispose();
        }

        private void CreateTable()
        {
            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var conn = database.CreateConnection();
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE TABLE {_tableName} (col1 INT , col2 VARCHAR(100))";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        private void DropTable()
        {
            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                var conn = database.CreateConnection();
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"DROP TABLE {_tableName}";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        private void Insert(params (int,string)[] values)
        {
            foreach (var database in Host.Services.GetServices<BaseDatabase>())
            {
                foreach (var value in values)
                {
                    var conn = database.CreateConnection();
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"INSERT INTO {_tableName} VALUES(@p1, @p2)";
                        var p = cmd.CreateParameter();
                        p.ParameterName = "p1";
                        p.Value = value.Item1;
                        cmd.Parameters.Add(p);
                        
                        var p2 = cmd.CreateParameter();
                        p2.ParameterName = "p2";
                        p2.Value = value.Item2;
                        cmd.Parameters.Add(p2);

                        cmd.ExecuteNonQuery();
                    }
                }

            }
        }

        private int Count(BaseDatabase database)
        {
            var conn = database.CreateConnection();
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM {_tableName}";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}