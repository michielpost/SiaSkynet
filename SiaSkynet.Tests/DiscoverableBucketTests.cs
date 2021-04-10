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
            var path = "crqa.hns/snew.hns/interactions/index.json";
            var bucket = new DiscoverableBucket(path);
            var dataKey = bucket.GetDataKey();

            var entry = new RegistryEntry() { Key = dataKey };
            var hexKey = entry.GetHexKey();

            Assert.IsNotNull(hexKey);
        }

    }
}
