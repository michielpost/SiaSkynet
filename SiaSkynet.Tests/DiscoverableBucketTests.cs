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
            var hashKey = bucket.GetHashedKey();

            RegistryEntry registry = new RegistryEntry(hashKey);
            var hexKey = registry.GetHexKey();

            Assert.IsNotNull(hexKey);
        }

    }
}
