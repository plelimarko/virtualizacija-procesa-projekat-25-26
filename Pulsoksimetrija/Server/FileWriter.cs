using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FileWriter : IDisposable
    {
        private StreamWriter writer;
        public FileWriter(string path) => writer = new StreamWriter(path, true);
        public void WriteLine(string line) { writer.WriteLine(line); writer.Flush(); }
        public void Dispose() { writer?.Close(); writer?.Dispose(); }
    }
}
