# vicini-server-aspnetcore
Server for the Vicini arduino web interface, implemented in ASP.NET core 2.1

**Note** Pay attention to the license, it may change. If you want to use the latest version you will have to comply with the latest license.

**Note 2** Currently this version of the server doesn't actually support serial communication, it exposes a fake AnalogDevices eval board for the LTC2668 DAC for testing purposes. 

To Build and run:
1. Install [Visual Studio Code](https://code.visualstudio.com/download)
2. Install [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1)
3. Install the [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) in Visual Studio Code
   * Just search C# in the extension manager and make sure it is from Microsoft, powered by omnisharp
4. Clone this repo
5. run `code vicini-server-aspnetcore` to open the folder in Visual Studio Code
6. Open Program.cs
7. Wait for some notifications from Visual Studio Code
8. Choose "Yes" for `Required assets to build and debug are missing from 'vicini-web-server-aspnetcore'. Add them?`
9. Choose "Restore" for `There are unresolved dependencies from 'vicini-server-aspnetcore.csproj'. Please exectute the restore command to continue.`
10. Hit `F5` to run the server.
11. Follow the directions to setup and run [vicini-web](https://github.com/gregoryjjb/vicini-web)
8. Be amazed.
