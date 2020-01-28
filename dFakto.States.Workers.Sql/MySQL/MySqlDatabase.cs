using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using dFakto.States.Workers.Sql.Common;
using MySql.Data.MySqlClient;

namespace dFakto.States.Workers.Sql.MySQL
{
    public class MySqlDatabase : BaseDatabase
    {
        public MySqlDatabase(DatabaseConfig config) : base(config)
        {
        }

        public override DbConnection CreateConnection()
        {
            return new MySqlConnection(Config.ConnectionString);
        }

        public override Task BulkInsert(IDataReader reader, string schemaName, string tableName, int timeout, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}