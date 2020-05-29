# SiaSkyNet
Sia Skynet client

Upload and download files from Sia Skynet

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


## Reference
- https://siasky.net
- https://sia.tech/docs/#skynet
