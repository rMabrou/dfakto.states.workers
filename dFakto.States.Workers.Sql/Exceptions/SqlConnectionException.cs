using System;

namespace dFakto.States.Workers.Sql.Exceptions
{
    public class SqlConnectionException : WorkerException
    {
        public SqlConnectionException(Exception inner)
            : base("dFakto.SQL.ConnectionFailed", inner.Message, inner)
        {
            
        }
    }
}