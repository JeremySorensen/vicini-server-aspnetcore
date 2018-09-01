using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace vicini_server_aspnetcore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder().Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder() {
            return new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .UseUrls("http://localhost:7651/");
        }
    }
}
