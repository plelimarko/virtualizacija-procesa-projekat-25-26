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
        private bool disposed = false;

        public FileWriter(string path)
        {
            writer = new StreamWriter(path, true);
        }

        ~FileWriter()
        {
            Dispose(false);
        }

        public void WriteLine(string line)
        {
            writer.WriteLine(line);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    writer?.Flush();
                    writer?.Close();
                    writer?.Dispose();
                }
                disposed = true;
            }
        }
    }
}
