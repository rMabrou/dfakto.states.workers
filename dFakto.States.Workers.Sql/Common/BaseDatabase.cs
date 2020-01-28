using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace dFakto.States.Workers.Sql.Common
{
    public abstract class BaseDatabase
    {        
        protected readonly DatabaseConfig Config;

        protected BaseDatabase(DatabaseConfig config)
        {
            Config = config;
        }

        public string Name => Config.Name;
        
        public abstract DbConnection CreateConnection();

        public async Task TruncateTable(string schemaName, string tableName)
        {
            string fullTableName = string.IsNullOrEmpty(schemaName) ? tableName : $"{schemaName}.{tableName}";
            
            using (var conn = CreateConnection())
            using(var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE " + fullTableName;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public abstract Task BulkInsert(IDataReader reader, string schemaName, string tableName, int timeout, CancellationToken token);
    }
}