# SiaSkyNet
Sia Skynet client

- Upload and download files from Sia Skynet.
- Interact with the registry
- Use SkyDB

## How to use
See included test project.

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
