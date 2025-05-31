using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generalibrary.Tcp
{
    public class ResponseEventArgs : EventArgsBase
    {
        public ResponseEventArgs(string message) : base(message)
        { }
    }
}
