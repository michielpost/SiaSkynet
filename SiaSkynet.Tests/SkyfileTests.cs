using Microsoft.VisualStudio.TestTools.UnitTesting;
using SiaSkynet;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SiaSkynet.Tests
{
    [TestClass]
    public class SkyfileTests
    {
        private SiaSkynetClient _client;

        public SkyfileTests()
        {
            _client = new SiaSkynetClient();
        }

        [TestMethod]
        public async Task UploadFileAsync()
        {
            string fileName = "test.txt";
            using (var fileStream = File.OpenRead(fileName))
            {
                var response = await _client.UploadFileAsync(fileName, fileStream);

                Assert.IsTrue(!string.IsNullOrEmpty(response.Skylink));
            }
        }

        [TestMethod]
        public async Task DownloadFileAsStreamAsync()
        {
            string skylink = "AAAAQZg5XQJimI9FGR73pOiC2PnflFRh03Z4azabKz6bVw";

            using (var response = await _client.DownloadFileAsStreamAsync(skylink))
            {
                using (StreamReader sr = new StreamReader(response))
                {
                    string text = await sr.ReadToEndAsync();
                    Assert.AreEqual("this is a test file", text);
                }
            }
        }

        [TestMethod]
        public async Task DownloadFileAsStringAsync()
        {
            string skylink = "AAAAQZg5XQJimI9FGR73pOiC2PnflFRh03Z4azabKz6bVw";

            var result = await _client.DownloadFileAsStringAsync(skylink);
            Assert.AreEqual("this is a test file", result.file);
            Assert.AreEqual("text/plain", result.contentType);
            Assert.AreEqual("test.txt", result.fileName);
        }

        [TestMethod]
        public async Task DownloadFileAsByteArrayAsync()
        {
            string skylink = "AAAAQZg5XQJimI9FGR73pOiC2PnflFRh03Z4azabKz6bVw";

            var result = await _client.DownloadFileAsByteArrayAsync(skylink);
            Assert.IsNotNull(result.file);
            Assert.AreEqual("text/plain", result.contentType);
            Assert.AreEqual("test.txt", result.fileName);
        }

        [TestMethod]
        public async Task GetFileMetaData()
        {
            string skylink = "AAAAQZg5XQJimI9FGR73pOiC2PnflFRh03Z4azabKz6bVw";

            var result = await _client.GetMetadata(skylink);
            Assert.AreEqual("test.txt", result.Filename);
        }
    }
}
