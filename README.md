# SiaSkynet C# SDK

The SkyDB part of this library was created for *[The SkyDB Debut](https://gitcoin.co/hackathon/skydb/)* hackathon.

SkyDB gives you access to key value storage on Sia Skynet. For the hackathon, a JavaScript SDK was provided to interact with SkyDB. 

This new library enables all C# developers in the world to use SkyDB. 

One of the most exciting features is the full compatibility with [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). Blazor allows you to create C# apps and run them entirely in the browser using WebAssembly. The app can then be uploaded to Skynet for hosting.

Combining the large **C#** ecosystem with the power of **SkyDB** and the speed of **WebAssembly** makes this solution ready for a decentralized  future!

**NOTE:** The sample app only gives a basic demonstrations of what's possible with this new SDK. The *SDK itself* is the submission for the hackathon, not the app.

Try out the sample: https://siasky.net/FAC82APHRUPV5_G2iNW1Qi_FcM3zwatoPvSFqnGCBRp5qw

Video demo: https://siasky.net/AADKNi9ltyZ9yjxk3xpfrJeXBfjhpdSlUeFBCEMXa7hyaw

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

var success = await _client.SkyDbSet(key.privateKey, key.publicKey, "datakey", "data");

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
