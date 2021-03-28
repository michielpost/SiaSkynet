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
            byte[] nonce = key.privateKey[0..24];
            byte[] priv = key.privateKey[0..32];
            byte[] pub = key.publicKey[0..32];

            var encrypted = Chaos.NaCl.XSalsa20Poly1305.Encrypt(System.Text.Encoding.UTF8.GetBytes(testValue), priv, nonce);
            var eString = System.Text.Encoding.UTF8.GetString(encrypted);
            Assert.AreNotEqual(testValue, eString);


            var decrypt = Chaos.NaCl.XSalsa20Poly1305.TryDecrypt(encrypted, priv, nonce);
            var dString = System.Text.Encoding.UTF8.GetString(decrypt);

            Assert.AreEqual(testValue, dString);
        }


    }
}
