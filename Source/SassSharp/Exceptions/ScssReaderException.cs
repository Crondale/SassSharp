using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Exceptions
{
    class ScssReaderException : Exception
    {
        public ScssReaderException(string message, string filename, int linenumber)
            : base(message + " - at " + filename + ":" + linenumber)
        {
        }
    }
}