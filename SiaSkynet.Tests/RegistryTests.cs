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
        public void TestKeys()
        {
            var key = SiaSkynetClient.GenerateKeys(_testSeed);
            var key2 = SiaSkynetClient.GenerateKeys(_testSeed);

            Assert.IsNotNull(key.publicKey);
        }

        [TestMethod]
        public void TestDataKey()
        {
            var encodedDK = "7c96a0537ab2aaac9cfe0eca217732f4e10791625b4ab4c17e4d91c8078713b9";
            RegistryEntry r = new RegistryEntry(new RegistryKey("app"));
            var hex = r.Key.GetHexKey();

            Assert.AreEqual(encodedDK, hex.ToLowerInvariant());
        }

        [TestMethod]
        public async Task TestSetRegistry()
        {
            string dataKey = Guid.NewGuid().ToString();
            int revision = 0;
            string data = "IADUs8d9CQjUO34LmdaaNPK_STuZo24rpKVfYW3wPPM2uQ"; //Sia logo

            var key = SiaSkynetClient.GenerateKeys(_testSeed);

            RegistryEntry reg = new RegistryEntry(new RegistryKey(dataKey));
            reg.SetData(data);
            reg.Revision = revision;

            var success = await _client.SetRegistry(key.privateKey, key.publicKey, reg);

            Assert.IsTrue(success);
        }

        [TestMethod]
        public async Task TestGetRegistry()
        {
            var key = SiaSkynetClient.GenerateKeys(_testSeed);

            RegistryKey dataKey = new RegistryKey("t3");

            var result = await _client.GetRegistry(key.publicKey, dataKey);

            Assert.IsNotNull(result);
            Assert.AreEqual("IADUs8d9CQjUO34LmdaaNPK_STuZo24rpKVfYW3wPPM2uQ", result.GetDataAsString());
        }

        [TestMethod]
        public async Task TestRegistryUpdate()
        {
            RegistryKey dataKey = new RegistryKey("regtest");
            var key = SiaSkynetClient.GenerateKeys(_testSeed);

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
            RegistryKey dataKey = new RegistryKey("skydbtest-" + Guid.NewGuid());
            var key = SiaSkynetClient.GenerateKeys(_testSeed);

            var success = await _client.SkyDbSetAsString(key.privateKey, key.publicKey, dataKey, "update1");
            Assert.IsTrue(success);
            await Task.Delay(TimeSpan.FromSeconds(5));

            var success2 = await _client.SkyDbSetAsString(key.privateKey, key.publicKey, dataKey, "update2");
            await Task.Delay(TimeSpan.FromSeconds(5));

            string result = await _client.SkyDbGetAsString(key.publicKey, dataKey);

            Assert.IsTrue(success);
            Assert.IsTrue(success2);

            Assert.AreEqual("update2", result);

        }

        [TestMethod]
        public async Task TestSkyDbUpdateSimple()
        {
            string newData = Guid.NewGuid().ToString();
            var key = SiaSkynetClient.GenerateKeys("my private key seed");
            RegistryKey dataKey = new RegistryKey("datakey");


            var success = await _client.SkyDbSetAsString(key.privateKey, key.publicKey, dataKey, newData);

            string result = await _client.SkyDbGetAsString(key.publicKey, dataKey);

            Assert.IsTrue(success);
            Assert.AreEqual(newData, result);

        }

        [TestMethod]
        public async Task TestForcedRevision()
        {
            string data = Guid.NewGuid().ToString();
            var key = SiaSkynetClient.GenerateKeys("my private key seed");
            RegistryKey dataKey = new RegistryKey("datakey" + Guid.NewGuid());
            int revision = 5;

            var success = await _client.SkyDbSet(key.privateKey, key.publicKey, dataKey, Encoding.UTF8.GetBytes(data), revision);

            var result = await _client.SkyDbGet(key.publicKey, dataKey);

            Assert.IsTrue(success);
            Assert.AreEqual(revision, result.Value.registryEntry.Revision);


            var stringResult = Encoding.UTF8.GetString(result.Value.file);
            Assert.AreEqual(data, stringResult);

        }


    }
}
