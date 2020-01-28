namespace dFakto.States.Workers.Sql.Common
{
    public class DatabaseConfig
    {
        public string Name { get; set; }
        public SqlDatabaseType Type { get; set; }
        public string ConnectionString { get; set; }
    }
}