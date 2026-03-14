
using System;

namespace NetCoreFileAccess.Models
{
    public class ConfigModel
    {
        public string Source { get; set; } = "Local";
        public FtpModel Ftp { get; set; } = new FtpModel();
        public GoogleDriveModel GoogleDrive { get; set; } = new GoogleDriveModel();
    }

    public class FtpModel
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 21;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PathFile { get; set; } = string.Empty;
    }

    public class GoogleDriveModel
    {
        public string PathFile { get; set; } = string.Empty;
        public string CCMng { get; set; } = string.Empty;        
    }
}