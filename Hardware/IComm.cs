using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViciniServer.Comm
{
    public struct CommOpenResult {
        public bool IsValid;
        public bool IsOpen;
        public IComm Comm;

        public static CommOpenResult Open(IComm comm)
        {
            return new CommOpenResult { IsValid = true, IsOpen = true, Comm = comm };
        }

        public static CommOpenResult Unavailable()
        {
            return new CommOpenResult { IsValid = true, IsOpen = false, Comm = null };
        }

        public static CommOpenResult Invalid()
        {
            return new CommOpenResult { IsValid = false, IsOpen = false, Comm = null };
        }
    }

    public interface IComm: IDisposable
    {
        bool WriteLine(string data, int timeoutMillis);

        bool ReadLine(int timeoutMillis, out string line);

        string ReadAll();

        bool GetDetails(int timeoutMillis, out string chip, out string board);
    }
}
