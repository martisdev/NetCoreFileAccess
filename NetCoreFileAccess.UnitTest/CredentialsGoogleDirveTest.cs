using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NetCoreFileAccess.UnitTests
{
    [TestFixture]
    public class CredentialsGoogleDirveTest
    {
        #region CONST

        const string APP_KEY = "Y0*@dJv6|np*^tRFEg8r&un3f%I2#ieZ";

        #endregion

        #region FIELDS
        string clientId = "XoslBk3DfrhJyUlKCQAAAATu6Z4rSvqpEl28qUirRW8y8FiCwKs4lQVmyXzm0BmTp/s2vPOo";
        string clientSecret = "khPn5U+Ep9pXPMJoWFFK8j1yGtLpkSqNNgJ";
        #endregion

        [SetUp]
        public void Setup()
        {

        }
        [Test]

        public void SetGoogleDriveCredentials_CorrectParameters_ReturnsBlock()
        {
            string block = Configurations.SetGoogleDriveCredentials(clientId, clientSecret, APP_KEY);

            Assert.That(block != null);
            Assert.That(block.Length > 0);
        }

        [Test]
        public void GetGoogleDriveCredentials_CorrectBlock_ReturnsCredentials()
        {
            string block = Configurations.SetGoogleDriveCredentials(clientId, clientSecret, APP_KEY);
            Configurations.GetGoogleDriveCredentials(block, APP_KEY);
            Assert.That(Config.GoogleConfig.ClientId, Is.EqualTo(clientId));
            Assert.That(Config.GoogleConfig.ClientSecret, Is.EqualTo(clientSecret));

        }
    }
}
