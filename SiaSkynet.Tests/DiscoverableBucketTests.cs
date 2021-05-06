using Microsoft.VisualStudio.TestTools.UnitTesting;
using SiaSkynet.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Tests
{
    [TestClass]
    public class DiscoverableBucketTests
    {
        private SiaSkynetClient _client;
        private string _testSeed = "secret password";

        public DiscoverableBucketTests()
        {
            _client = new SiaSkynetClient();
        }

        [TestMethod]
        public void TestPathToDataKey()
        {
            //var path = "skyfeed.hns/preferences/ui.json";
            //var path = "crqa.hns/snew.hns/123"; 
            var path = "crqa.hns/snew.hns/newcontent/index.json"; 
            //var path = "crqa.hns/snew.hns/newcontent/page_0.json";

            var bucket = new DiscoverableBucket(path);
            RegistryEntry registry = new RegistryEntry(new RegistryKey(bucket));

            var hexKey = registry.Key.GetHexKey();
            Assert.IsNotNull(hexKey);
        }

        [TestMethod]
        public async Task TestSetRegistry_With_DiscoverableBucket()
        {
            var path = $"crqa.hns/snew.hns/newcontent/{Guid.NewGuid().ToString()}/index.json";
            var bucket = new DiscoverableBucket(path);
            var dataKey = new RegistryKey(bucket);

            int revision = 0;
            string data = "IADUs8d9CQjUO34LmdaaNPK_STuZo24rpKVfYW3wPPM2uQ"; //Sia logo

            var key = SiaSkynetClient.GenerateKeys(_testSeed);

            RegistryEntry reg = new RegistryEntry(dataKey);
            reg.SetData(data);
            reg.Revision = revision;

            var success = await _client.SetRegistry(key.privateKey, key.publicKey, reg);

            Assert.IsTrue(success);

            var result = await _client.GetRegistry(key.publicKey, dataKey);
            Assert.AreEqual(data, result.GetDataAsString());
        }

    }
}
