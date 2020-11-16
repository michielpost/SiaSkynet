using Microsoft.VisualStudio.TestTools.UnitTesting;
using SiaSkynet.Responses;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Tests
{
    [TestClass]
    public class RegistryTests
    {
        private SiaSkynetClient _client;
        private string _testSeed = "secret password";

        public RegistryTests()
        {
            _client = new SiaSkynetClient();
        }

        [TestMethod]
        public async Task TestKeys()
        {
            var key = await SiaSkynetClient.GenerateKeys(_testSeed);

            Assert.IsNotNull(key.publicKey);
        }

        [TestMethod]
        public async Task TestSetRegistry()
        {
            string dataKey = "t3";
            int revision = 0;
            string data = "IADUs8d9CQjUO34LmdaaNPK_STuZo24rpKVfYW3wPPM2uQ"; //Sia logo

            var key = await SiaSkynetClient.GenerateKeys(_testSeed);

            RegistryEntry reg = new RegistryEntry();
            reg.Key = dataKey;
            reg.SetData(data);
            reg.Revision = revision;

            var success = await _client.SetRegistry(key.privateKey, key.publicKey, reg);

            Assert.IsTrue(success);
        }

        [TestMethod]
        public async Task TestGetRegistry()
        {
            var key = await SiaSkynetClient.GenerateKeys(_testSeed);

            string dataKey = "t3";

            var result = await _client.GetRegistry(key.publicKey, dataKey);

            Assert.IsNotNull(result);
            Assert.AreEqual("IADUs8d9CQjUO34LmdaaNPK_STuZo24rpKVfYW3wPPM2uQ", result.GetDataAsString());
        }

        [TestMethod]
        public async Task TestRegistryUpdate()
        {
            string dataKey = "regtest";
            var key = await SiaSkynetClient.GenerateKeys(_testSeed);

            var success = await _client.UpdateRegistry(key.privateKey, key.publicKey, dataKey, "update1");
            await Task.Delay(TimeSpan.FromSeconds(5));

            var success2 = await _client.UpdateRegistry(key.privateKey, key.publicKey, dataKey, "update2");
            await Task.Delay(TimeSpan.FromSeconds(5));

            var result = await _client.GetRegistry(key.publicKey, dataKey);

            Assert.IsTrue(success);
            Assert.IsTrue(success2);

            Assert.AreEqual("update2", result.GetDataAsString());

        }

        [TestMethod]
        public async Task TestSkyDbUpdate()
        {
            string dataKey = "skydbtest";
            var key = await SiaSkynetClient.GenerateKeys(_testSeed);

            var success = await _client.SkyDbSet(key.privateKey, key.publicKey, dataKey, "update1");
            await Task.Delay(TimeSpan.FromSeconds(5));

            var success2 = await _client.SkyDbSet(key.privateKey, key.publicKey, dataKey, "update2");
            await Task.Delay(TimeSpan.FromSeconds(5));

            string result = await _client.SkyDbGetAsString(key.publicKey, dataKey);

            Assert.IsTrue(success);
            Assert.IsTrue(success2);

            Assert.AreEqual("update2", result);

        }

        [TestMethod]
        public async Task TestSkyDbUpdate()
        {
            var key = await SiaSkynetClient.GenerateKeys("my private key seed");

            var success = await _client.SkyDbSet(key.privateKey, key.publicKey, "datakey", "data");

            string result = await _client.SkyDbGetAsString(key.publicKey, "datakey");

            Assert.AreEqual("data", result);

        }


    }
}
