using System;
using System.Runtime.Serialization;

namespace dFakto.States.Workers.FileStores
{
    public class InvalidFileTokenException : Exception
    {
        public InvalidFileTokenException()
        {
        }

        protected InvalidFileTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidFileTokenException(string message) : base(message)
        {
        }

        public InvalidFileTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}