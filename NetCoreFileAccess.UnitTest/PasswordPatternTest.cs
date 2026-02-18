using NUnit.Framework;
using NetCoreFileAccess;

namespace SecureBunker.UnitTests
{
    [TestFixture]
    public class PasswordPatternTest
    {
        [SetUp]
        public void Setup()
        {
            // Any necessary setup before each test
            CredentialsUtils.Pattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{4,20}$";
        }

        [Test]
        [TestCase("aA1!", true)] //Pass all conditions
        [TestCase("aA1", false)] //Fail, less than minimum characters long
        [TestCase("AaDdFt", false)]  //Fail, without number
        [TestCase("1234596", false)] //Fail, without letter
        [TestCase("Aa125", false)] //Fail, without special character
        [TestCase("1234596AaAhskdfgnkdasdlkjl", false)] //Fail, too long
        public void CheckPassword_CorrectPassword_Pass(string Password, bool pass)
        {            
            bool result = CredentialsUtils.CheckPassword(Password);
            Assert.That(result, Is.EqualTo(pass));
        }

    }
}
