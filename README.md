# SiaSkynet C# SDK

This library enables all C# developers in the world to use SkyDB. 

One of the most exciting features of this SDK is the full compatibility with [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). Blazor allows you to create C# apps and run them entirely in the browser using WebAssembly. The app can then be uploaded to Skynet for hosting.

Combining the large **C#** ecosystem with the power of **SkyDB** and the speed of **WebAssembly** makes this solution ready for a decentralized future!

Try out the [Sample App](https://2g01e51a0vlfbhlq8oedvq2lvi482lld730jeglhtviifrjdh0aq08g.siasky.net/)
or watch the [Video demo](https://siasky.net/AADKNi9ltyZ9yjxk3xpfrJeXBfjhpdSlUeFBCEMXa7hyaw)

Also see [SkyDocs](https://github.com/michielpost/SkyDocs)

---

Sia Skynet client

- Upload and download files from Sia Skynet.
- Interact with the registry
- Use SkyDB

## How to use
See included test project and Blazor sample app.

Install: [SiaSkynet on NuGet](https://www.nuget.org/packages/SiaSkynet/)

Initialize the client
```cs
var _client = new SiaSkynetClient();
```

Upload a file
```cs
string fileName = "test.txt";
using (var fileStream = File.OpenRead(fileName))
{
    var response = await _client.UploadFileAsync(fileName, fileStream);
    var skylink = response.Skylink;
}
```

Download a file
```cs
string skylink = "AAAAQZg5XQJimI9FGR73pOiC2PnflFRh03Z4azabKz6bVw";
using (var response = await _client.DownloadFileAsStreamAsync(skylink))
{
    using (StreamReader sr = new StreamReader(response))
    {
        string text = await sr.ReadToEndAsync();
    }
}
```

Use these methods if you also want the original filename and content type.
```cs
_client.DownloadFileAsStringAsync(skylink);
_client.DownloadFileAsByteArrayAsync(skylink);
```

### SkyDB
Support for SkyDB
https://siasky.net/docs/#skydb

Set and get values on SkyDB:
```cs
var key = await SiaSkynetClient.GenerateKeys("my private key seed");

var success = await _client.SkyDbSetAsString(key.privateKey, key.publicKey, "datakey", "data");

string result = await _client.SkyDbGetAsString(key.publicKey, "datakey");

Assert.AreEqual("data", result);
```

### Registry
SkyDB uses the Skynet Registry, it's also possible to interact with the registry using API's:
- `siaSkynetClient.GetRegistry` 
- `siaSkynetClient.SetRegistry` 
- `siaSkynetClient.UpdateRegistry` which performs a get and a set with a new revision number

## Reference
- https://siasky.net
- https://sia.tech/docs/#skynet

## Apps that use this library:
Are you using SiaSkynet? Get your app listed here! Edit this page and send a pull request.

- [SkyDocs](https://github.com/michielpost/SkyDocs)
- [Stresstest and Speedtest by autisticvegan](https://github.com/autisticvegan/siahackathonsubmission)
