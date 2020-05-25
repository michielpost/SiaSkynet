using Microsoft.VisualStudio.TestTools.UnitTesting;
using SiaSkynet;

namespace SiaSkynet.Tests
{
    [TestClass]
    public class FileUploadTests
    {
        private ISiaSkynetApi _client;

        public FileUploadTests()
        {
            _client = SiaSkynetClient.GetClient();
        }

        [TestMethod]
        public void UploadFile()
        {

        }
    }
}
