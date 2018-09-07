using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViciniServer.Comm
 {
    class FakeSerial : IComm {

        private string buffer = "LTC2668,FAKE enter 'help' for commands";

        public void Dispose() {
            
        }

        public static Dictionary<string, HardwareInfo> Find(
            Dictionary<string, HardwareInfo> prevHardware,
            int maxArduinos = int.MaxValue)
        {
            if ((prevHardware?.Count ?? 0) == 0) {
                return new Dictionary<string, HardwareInfo> {
                    { "COM0", new HardwareInfo("COM0", false) },
                    { ComPortNumber, new HardwareInfo(ComPortNumber, true) },
                };
            } else {
                return prevHardware;
            }
        }

        public static CommOpenResult Open(string portNumber, int baudRate = 115200) {
            if (portNumber == ComPortNumber) {
                return CommOpenResult.Open(new FakeSerial());
            } else {
                return CommOpenResult.Unavailable();
            }
        }

        public FakeSerial() {
            Handlers = new Dictionary<string, Func<string, string>>() {
                {"id", (s) => "LTC2668,FAKE" },
                {"global_toggle", (s) => {
                    var (ok, level) = Get1Field(s);
                    if (!ok) { return level; }
                    if (level != "low" && s != "high") { return "Error [bad_arg]: toggle must be high or low"; }
                    return $"Set global toggle to {level}";
                }},
                {"mux", (s) => {
                    var (ok, channel) = Get1Field(s);
                    if (!ok) { return channel; }

                    if (channel == "disable") {
                        return $"Monitor MUX disabled";
                    }

                    if (channel == "enable") {
                        return $"Monitor MUX enabled for channel {muxChannel}";
                    }

                    if (!int.TryParse(channel, out muxChannel)) {
                        return "Error [bad_arg]: expected a channel number, 'enable' or 'disable'";
                    }

                    return $"Monitor MUX enabled for channel {muxChannel}";
                }},
                {"power_down", (s) => {
                    var (ok, ch) = Get1Field(s);
                    if (!ok) { return ch; }

                    if (ch == "all") {
                        return "All DACs powered down";
                    }
                    
                    int channel;
                    if (!int.TryParse(ch, out channel)) {
                        return "Error [bad_arg]: Expected a channel number or 'all'";
                    }

                    return $"DAC {channel} powered down";
                }},
                {"ramp", (s) => "Set a ramp of codes accross all DACs"},
                {"reference", (s) => {
                    var (ok, reference) = Get1Field(s);
                    if (!ok) { return reference; }
                    if (reference != "external" && reference != "internal") {
                        return "Error [bad_arg]: Expected 'internal or 'external'";
                    } else {
                        return $"Reference set to {reference}";
                    }
                }},
                {"select_bits", (s) => {
                    var (ok, setOrClear, bits) = Get2Fields(s);

                    if (ok) {
                        var result = HandleSelectBitArgs(setOrClear, bits);
                        if (result != null) { return result; }
                    } else {
                        var message = setOrClear;
                        var (ok2, sc1, b1, sc2, b2) = Get4Fields(s);
                        if (!ok2) {
                            return message;
                        }
                        var result = HandleSelectBitArgs(sc1, b1);
                        if (result != null) { return result; }
                        result = HandleSelectBitArgs(sc2, b2);
                        if (result != null) { return result; }
                    }

                    var sb = new StringBuilder();
                    sb.Append("Thiese bits are set: ");
                    for (int i = 0; i < selectBits.Length; ++i) {
                        if (selectBits[i]) {
                            sb.Append(i);
                            sb.Append(", ");
                        }
                    }
                    return sb.Remove(sb.Length - 2, 2).ToString();
                }},
                {"span", (s) => {
                    var (ok, ch, sp) = Get2Fields(s);
                    if (!ok) { return ch; }
                    var spans = new Dictionary<string, string>() {
                        {"5", "0-5V"},
                        {"10", "0-10V"},
                        {"+-5", "+/-5V"},
                        {"+-10", "+/-10V"},
                        {"+-2.5", "+/-2.5V"}
                    };
                    string span;
                    if (!spans.TryGetValue(sp, out span)) {
                        return "Error [bad_arg]: Bad span";
                    } 
                    if (ch == "all") {
                        return "All DACs span set to " + span;
                    } else {
                        int channel;
                        if (!int.TryParse(ch, out channel)) {
                            return "Error [bad_arg]: Bad channel";
                        }
                        return "DAC 5 span set to " + span;
                    }
                }},
                {"update", (s) => {
                    var (ok, ch) = Get1Field(s);
                    if (!ok) { return ch; }
                    return $"Channel {ch} updated";
                }},
                {"write", (s) => {
                    var (ok, units, ch, value) = Get3Fields(s);
                    if (!ok) { return units; }
                    string suffix;
                    if (units == "volts") {
                        suffix = $"set to 12345 ({value} volts)";
                    } else {
                        suffix = $"set to {value} (1.5 volts)";
                    }
                    string prefix;
                    if (ch == "all") {
                        prefix = "All DACs ";
                    } else {
                        int channel;
                        if (!int.TryParse(ch, out channel)) {
                            return "Error [bad_arg]: Expected channel number or 'all'";
                        }
                        prefix = $"DAC {channel} ";
                    }
                    return prefix + suffix;
                }},
                {"write_update", (s) => {
                    return Handlers["write"](s) + " and updated";
                }},
                {"help", HelpResponse},
            };
        }

        public bool GetDetails(int timeoutMillis, out string chip, out string board) {
            chip = "LTC2668";
            board = "FAKE";
            return true;
        }

        public bool ReadLine(int timeoutMillis, out string line) {
            if (string.IsNullOrEmpty(buffer)) {
                line = string.Empty;
                return false;
            }

            (line, buffer) = HeadAndTail(buffer, '\n');
            line += '\n';
            return true;
        }

        public string ReadAll() {
            var result = buffer;
            buffer = string.Empty;
            return result;
        }

        public bool WriteLine(string data, int timeoutMillis) {
            var (head, tail) = HeadAndTail(data, ' ');
            Func<string, string> fun;
            if (!Handlers.TryGetValue(head, out fun)) {
                buffer = "Error [bad_command]: Command not found\n";
                return false;
            }
            buffer = fun(tail) + '\n';
            return true;
        }

        private Tuple<string, string> HeadAndTail(string s, char delim) {
            var headAndTail = s.Split(delim, 2);
            if (headAndTail.Length == 1) {
                return Tuple.Create(headAndTail[0], string.Empty);
            } else {
                return Tuple.Create(headAndTail[0], headAndTail[1]);
            }
        }

        private Tuple<bool, string> Get1Field(string s) {
            var fields = s.ToLower().Split(' ');
            if (fields.Length != 1) {
                return Tuple.Create(
                    false,
                     $"Error [bad_num_args]: Expected 1 arg, got {fields.Length}");
            } else {
                return Tuple.Create(true, fields[0]);
            }
        }

        private Tuple<bool, string, string> Get2Fields(string s) {
            var fields = s.ToLower().Split(' ');
            if (fields.Length != 2) {
                return Tuple.Create(
                    false,
                    $"Error [bad_num_args]: Expected 1 arg, got {fields.Length}",
                    string.Empty);
            } else {
                return Tuple.Create(true, fields[0], fields[1]);
            }
        }

        private Tuple<bool, string, string, string> Get3Fields(string s) {
            var fields = s.ToLower().Split(' ');
            if (fields.Length != 3) {
                return Tuple.Create(
                    false,
                    $"Error [bad_num_args]: Expected 1 arg, got {fields.Length}",
                    string.Empty,
                    string.Empty);
            } else {
                return Tuple.Create(true, fields[0], fields[1], fields[2]);
            }
        }

        private Tuple<bool, string, string, string, string> Get4Fields(string s) {
            var fields = s.ToLower().Split(' ');
            if (fields.Length != 3) {
                return Tuple.Create(
                    false,
                    $"Error [bad_num_args]: Expected 1 arg, got {fields.Length}",
                    string.Empty,
                    string.Empty,
                    string.Empty);
            } else {
                return Tuple.Create(true, fields[0], fields[1], fields[2], fields[3]);
            }
        }

        private string HandleSelectBitArgs(string setOrClear, string bits) {
            if (setOrClear == "set") {
                if (!SetBits(bits)) {
                    return "Error [bad_arg]: Set bits not right";
                }
                return null;
            } else if (setOrClear == "clear") {
                if (!SetBits(bits, false)) {
                    return "Error [bad_arg]: Clear bits not right";
                }
                return null;
            } else {
                return "Error [bad_arg]: Expected set or clear";
            }
        }

        private bool SetBits(string bits, bool set = true) {
            var fields = bits.Split(',');
            foreach (var f in fields) {
                int bit;
                if (!int.TryParse(f, out bit) || bit < 0 || bit > 15) {
                    return false;
                }
                selectBits[bit] = set;
            }
            return true;
        }

        Dictionary<string, Func<string, string>> Handlers;

        private string HelpResponse(string s) {
                return "Commands:\n" +
                "* help [COMMAND] - Print help for COMMAND or all commands\n" +
                "* id - show part name and eval board name\n" +
                "* global_toggle (high | low) - Set global toggle bit high or low\n" +
                "* mux (CH | enable | disable) - Set mux to monitor a channel or disable it.\n" +
                "    CH is 0-15\n" +
                "    if 'enable' is passed previously enabled channel is reenabled\n" +
                "    Example: mux 1\n" +
                "    Example: mux enable\n" +
                "    Example: mux disable\n" +
                "\n" +
                "* power_down CH - Power down DAC channel(s).\n" +
                "    CH is 0-16, or 'all'\n" +
                "    Example: power_down 3\n" +
                "* ramp - Set a ramp over all channels, each channel has a higher code than the previous\n" +
                "* reference internal | external - Set the reference to be internal or external\n" +
                "    Example: reference internal\n" +
                "* select_bits [set SET_BITS] [clear CLEAR_BITS] - Set and unset channel select bits\n" +
                "    SET_BITS is comma separated list of bit indices to set or 'all'\n" +
                "    CLEAR_BITS is comma separated list of bit indices to clear or 'all'\n" +
                "    must have 'set SET_BITS' or 'clear CLEAR_BITS' or both\n" +
                "    Example: select_bits set 1,3,7 clear 2,6\n" +
                "    Example: select_bits set all\n" +
                "    Example: select_bits clear 5\n" +
                "* span CH SPAN - Set the span for the channel(s).\n" +
                "    CH is 0-15 or 'all'\n" +
                "    SPAN is 5, 10, +-5, +-10, or +-2.5\n" +
                "    Example: span all 10\n" +
                "* update CH - Update and power up DAC channel(s).\n" +
                "    CH is 0-16, or 'all'\n" +
                "    Example: update 0\n" +
                "* write (volts | code) CH VALUE - Write input register(s).\n" +
                "    CH is 0-16, or 'all'\n" +
                "    VALUE is ADC code to write or Volts to set.\n" +
                "    Example: write_input volts 2 1.23\n" +
                "* write_update (volts | code) CH VALUE - Write input register(s) and update output(s).\n" +
                "    CH is 0-16 or 'all' \n" +
                "    VALUE is ADC code to write or Volts to set.\n" +
                "    Example: write_update volts 2 1.23";
        }

            private int muxChannel = 0;
            private bool[] selectBits = new bool[16];

            private const string ComPortNumber = "COM3";

    }
}
