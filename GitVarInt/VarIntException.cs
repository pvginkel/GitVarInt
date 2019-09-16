using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitVarInt
{
    public class VarIntException : Exception
    {
        public VarIntException()
        {
        }

        public VarIntException(string message)
            : base(message)
        {
        }

        public VarIntException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
