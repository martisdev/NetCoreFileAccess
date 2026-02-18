using NetCoreFileAccess.Criptography;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCoreFileAccess
{
    public static class CredentialsUtils
    {
        #region CONSTS
        private static readonly string PATTERN_DEF = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{4,20}$";

        #endregion

        #region PROPERTIES

        private static string _Pattern = string.Empty;
        public static string Pattern
        {
            get
            {
                if(string.IsNullOrEmpty(_Pattern))
                    return PATTERN_DEF;
                else
                    return _Pattern;
            }
            set
            {
                _Pattern = value;
            }
        }

        #endregion

        public static bool CheckPassword(string v)
        {
            try
            {
                return Regex.IsMatch(v, Pattern);
            }
            catch
            {
                // Syntax error in the regular expression
                return false;
            }
        }

        /// <summary>
        /// Implement validation of credentials for local access if needed
        /// </summary>
        /// <param name="data">File Data</param>
        /// <param name="User">User to check</param>
        /// <param name="Password">Passord to check</param>
        /// <returns></returns>
        public static bool ValidateCredentials(MemoryStream data, string User, string Password)
        {

            data.Position = Cryptography.RAMDOM_LENGTH;
            byte[] keyIV = new byte[Cryptography.IV_LENGTH];
            int bytesRead = data.Read(keyIV, 0, keyIV.Length);

            Cryptography.KEY_IV = keyIV;

            // read length-prefixed encrypted user blob
            byte[] lenBytes = new byte[4];
            data.Read(lenBytes, 0, 4);
            int userCipherLen = BitConverter.ToInt32(lenBytes, 0);
            byte[] userCipher = new byte[userCipherLen];
            data.Read(userCipher, 0, userCipherLen);
            byte[]? outBytes = Cryptography.AESDecrypt(userCipher,
                Encoding.UTF8.GetBytes(Password.PadRight(Cryptography.USER_LENGTH)));

            if (outBytes == null)
                return false;

            string UserName = Encoding.UTF8.GetString(outBytes).Trim();

            if (UserName != User)
                return false;


            return true;
        }
    }
}

