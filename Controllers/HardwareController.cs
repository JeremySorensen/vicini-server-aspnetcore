using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ViciniServer.Comm;
using ViciniServer.Requests;

namespace ViciniServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HardwareController : Controller
    {
        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            hardwareInfo = FakeSerial.Find(hardwareInfo);
            return HardwareResponse();
        }

        [HttpPut("{id}/open")]
        public JsonResult Open(string id)
        {
            var response = CheckId(id);
            if (response != null) { return response; }

            IComm comm;
            if (!hardware.TryGetValue(id, out comm)) {
                var result = FakeSerial.Open(id);
                if (result.Comm != null) {
                    hardwareInfo[id].Open = true;
                    comm = result.Comm;
                    hardware.Add(id, comm);
                } else if (result.IsValid) {
                    hardwareInfo[id] = new HardwareInfo(id, false);
                    return HardwareResponse(id);
                } else {
                    hardwareInfo.Remove(id);
                    return HardwareResponse(id);
                }
            }
            string chip, board;
            if (!comm.GetDetails(DefaultTimeout, out chip, out board)) {
                return TimeoutResponse(id, "getting details");
            }

            hardwareInfo[id].Details = hardwareInfo[id].Details ?? new Details();
            hardwareInfo[id].Details.Board = board;
            hardwareInfo[id].Details.Chip = chip;

            return HardwareResponse(id);
        }

        [HttpPut("{id}/send_command")]
        public JsonResult SendCommand(string id , [FromBody] CommandRequest command)
        {
            
            var response = CheckId(id);

            if (response != null) { return response; }

            var comm = hardware[id];
            var previous = comm.ReadAll();

            string sendString;
            if (command.command?.args?.Count > 0) {
                sendString = command.command.name + " " + string.Join(" ", command.command.args);
            } else {
                sendString = command.command.name;
            }

            var timeout = command.timeout ?? DefaultTimeout;
            var wait = command.wait ?? DefaultWait;

            if (!comm.WriteLine(sendString, timeout)) {
                return TimeoutResponse(id, "writing command");
            }

            string receiveString;
            if (wait > 0) {
                System.Threading.Thread.Sleep(wait);
                timeout = Math.Max(1, timeout - wait);

                receiveString = comm.ReadAll();

                if (!receiveString.Contains("\n")) {
                    string extraString;
                    if (!comm.ReadLine(timeout, out extraString)) {
                        return TimeoutResponse(id, "reading response");
                    }
                    receiveString += extraString;
                }
            } else {
                if (!comm.ReadLine(timeout, out receiveString)) {
                    return TimeoutResponse(id, "reading response");
                }
            }
            return Json(new {
                    id = id, 
                    serial = new { send = sendString, receive = receiveString, previous = previous}
                });
        }

        [HttpPut("{id}/close")]
        public JsonResult Close(string id)
        {
            var response = CheckId(id);
            if (response != null) { return response; }

            IComm comm;
            if (hardware.TryGetValue(id, out comm)) {
                comm.Dispose();
                hardwareInfo[id].Open = false;
                hardwareInfo[id].Details = null;
                hardware.Remove(id);
            }

            return HardwareResponse(id);
        }

        private JsonResult HardwareResponse(string id = null)
        {
            if (id == null) {
                return Json(new { hardware = hardwareInfo.Values.ToArray() });
            } else {
                return Json(new { id = id, hardware = hardwareInfo.Values.ToArray() });
            }
        }

        private JsonResult CheckId(string id)
        {
            return hardwareInfo.ContainsKey(id) ? null :
                 Json(new { id = id, status = "BadId", message = $"{id} is not a valid ID" });
        }

        private JsonResult TimeoutResponse(string id, string action)
        {
            return Json(new { id = id, status = "Timeout", message = $"Time out while {action}"});
        }

        private static Dictionary<string, HardwareInfo> hardwareInfo;
        private static Dictionary<string, IComm> hardware = new Dictionary<string, IComm>();
        private const int DefaultTimeout = 1000;
        private const int DefaultWait = 150;

    }
}
