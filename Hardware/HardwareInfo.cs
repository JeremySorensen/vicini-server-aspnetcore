namespace ViciniServer.Comm
{
    public class Details {
        public string Chip { get; set; }
        public string Board { get; set; }
    }

    public class HardwareInfo {
        public string Id { get; set; }
        public bool Available { get; set; }
        public Details Details { get; set; } = null;
        public bool Open { get; set; } = false;
        public HardwareInfo (string id, bool isAvailable) {
            this.Id = id;
            this.Available = isAvailable;
        }
    }
}