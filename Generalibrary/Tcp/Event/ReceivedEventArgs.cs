using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generalibrary.Tcp
{
    public class ReceivedEventArgs : EventArgsBase
    {
        public ReceivedEventArgs(string message) : base(message)
        { }
    }
}
