using Microsoft.VisualStudio.TestTools.UnitTesting;
using SiaSkynet;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SiaSkynet.Tests
{
    [TestClass]
    public class EncryptionTests
    {
        private SiaSkynetClient _client;

        public EncryptionTests()
        {
            _client = new SiaSkynetClient();
        }

        [TestMethod]
        public void BasicEncryptionTest()
        {
            var key = SiaSkynetClient.GenerateKeys("test");
            var key2 = SiaSkynetClient.GenerateKeys("bar");

            var testValue = "this is not encrypted";

            var encrypted = Utils.Encrypt(System.Text.Encoding.UTF8.GetBytes(testValue), key.privateKey);
            var eString = System.Text.Encoding.UTF8.GetString(encrypted);
            Assert.AreNotEqual(testValue, eString);


            var decrypt = Utils.Decrypt(encrypted, key.privateKey);
            var dString = System.Text.Encoding.UTF8.GetString(decrypt);

            Assert.AreEqual(testValue, dString);
        }


    }
}
