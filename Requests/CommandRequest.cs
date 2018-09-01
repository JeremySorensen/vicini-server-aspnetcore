using System.Collections.Generic;

namespace ViciniServer.Requests
{
    public class Command {
        public string name { get; set; }
        public List<string> args { get; set; }
    }

    public class CommandRequest {
        public Command command { get; set; }
        public int? timeout { get; set; }
        public int? wait { get; set; }
    }
}