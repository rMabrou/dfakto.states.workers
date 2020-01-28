using System;

namespace dFakto.States.Workers.Sql.Exceptions
{
    public class SqlQueryException : WorkerException
    {
        public SqlQueryException(Exception inner)
            : base("dFakto.SQL.QueryFailed", inner.Message, inner)
        {
            
        }
    }
}