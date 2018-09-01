using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

// This will be replaced with a cross-platform solution, removing for now.
/*
namespace ViciniServer.Comm
{
    public class Serial : IComm
    {
        public string PortNumber { get { return portNumber; } }

        private Serial(string portNumber, int baudRate)
        {
            this.portNumber = portNumber;
            this.serialPort = new SerialPort(portNumber, baudRate);
        }

        public static CommOpenResult Open(string portNumber, int baudRate = 115200) {
            var serial = new Serial(portNumber, baudRate);
            try {
                serial.serialPort.Open();
                return CommOpenResult.Open(serial);
            } catch (IOException) {
                return CommOpenResult.Invalid();
            } catch (UnauthorizedAccessException) {
                return CommOpenResult.Unavailable();
            }
        }

        public static Dictionary<string, HardwareInfo> Find(Dictionary<string, HardwareInfo> prevHardware)
        {
            var newHardware = new Dictionary<string, HardwareInfo>();

            foreach (var portNumber in SerialPort.GetPortNames()) {
                var result = Open(portNumber);
                if (result.IsValid) {
                    HardwareInfo h = new HardwareInfo(portNumber, false);
                    prevHardware?.TryGetValue(portNumber, out h);
                    if (result.IsOpen) {
                        result.Comm.Dispose(); // close it
                        h.Available = true;
                        h.Open = false;
                    } else if (!h.Open) { // if it was open already that is the reason we couldn't open it
                        h.Available = false;
                    }
                    newHardware.Add(portNumber, h);
                }
            }

            return newHardware;
        }

        public bool WriteLine(string data, int timeoutMillis)
        {
            serialPort.WriteTimeout = timeoutMillis;
            try {
                serialPort.Write(data + "\n");
                return true;
            } catch (TimeoutException) {               
                return false;
            }
        }

        public bool ReadLine(int timeoutMillis, out string line)
        {
            serialPort.ReadTimeout = timeoutMillis;
            try {
                line = serialPort.ReadLine().Trim();
                return true;
            } catch (TimeoutException) {
                line = null;
                return false;
            }
        }

        public string ReadAll()
        {
            return serialPort.ReadExisting();
        }

        public bool GetDetails(int timeoutMillis, out string chip, out string board)
        {
            chip = null;
            board = null;
            if (!WriteLine("id", timeoutMillis)) {
                return false;
            }

            string result;
            if(!ReadLine(timeoutMillis, out result)) {
                return false;
            }

            var fields = result.Split(',', ' ');
            if (fields.Length >= 2) {
                chip = fields[0];
                board = fields[1];
                return true;
            } else {
                return false;
            }
        }

        public void Dispose()
        {
            serialPort.Close();
            ((IDisposable)serialPort).Dispose();
        }

        private string portNumber;
        private SerialPort serialPort;
    }
}

*/
