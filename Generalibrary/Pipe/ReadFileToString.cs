using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generalibrary.Pipe
{
    public class ReadFileToStream
    {
        private string _fileName;
        private StreamString _streamString;

        public ReadFileToStream(StreamString streamString, string filename)
        {
            _fileName = filename;
            _streamString = streamString;
        }

        public void Start()
        {
            string contents = File.ReadAllText(_fileName);
            _streamString.WriteString(contents);
        }
    }
}
