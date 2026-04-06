using NetCoreFileAccess;
using NetCoreFileAccess.SourceAccess;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using static NetCoreFileAccess.Config;

namespace SecureBunker.UnitTests
{
    [TestFixture]
    public class ConfigurationsTests
    {
        private readonly string _configDir = Path.Combine(AppContext.BaseDirectory, "config");
        private readonly string _configPath;

        public ConfigurationsTests()
        {
            _configPath = Path.Combine(_configDir, "config.json");
        }

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(_configDir))
                Directory.CreateDirectory(_configDir);


            // reset static config to defaults to avoid cross-test leakage            
            Config.sourceType = SourceType.None;

            Config.FTPConfig.Host = string.Empty;
            Config.FTPConfig.Port = 21;
            Config.FTPConfig.Username = string.Empty;
            Config.FTPConfig.Password = string.Empty;
            Config.FTPConfig.PathFile = string.Empty;

            Config.GoogleConfig.PathFile = string.Empty;            
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (File.Exists(_configPath))
                    File.Delete(_configPath);
            }
            catch { /* best-effort cleanup */ }
        }

        [Test]
        public void SelectTypeSource_Ftp_ParsesFtpSection()
        {
            var payload = new
            {
                Source = "Ftp",
                Ftp = new
                {
                    Host = "ftp.example.com",
                    Port = 2121,
                    UserName = "ftpuser",
                    Password = "ftppass",
                    PathFile = "/remote/data.fscr",
                    UseSsl = true
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);

            var result = Configurations.LoadConfigurationFile();

            Assert.That(result, Is.EqualTo(SourceType.Ftp));            
            Assert.That("ftp.example.com", Is.EqualTo(Config.FTPConfig.Host));
            Assert.That(2121, Is.EqualTo(Config.FTPConfig.Port));
            Assert.That("ftpuser", Is.EqualTo(Config.FTPConfig.Username));
            Assert.That("ftppass", Is.EqualTo(Config.FTPConfig.Password));
            Assert.That("/remote/data.fscr", Is.EqualTo(Config.FTPConfig.PathFile));
        }

        [Test]
        public void LoadConfiguration_FileMissing_CreatesDefaultAndReturnsLocal()
        {
            // Ensure file doesn't exist
            if (File.Exists(_configPath))
                File.Delete(_configPath);

            var result = Configurations.LoadConfigurationFile();

            Assert.That(result, Is.EqualTo(SourceType.Local));
            Assert.That(File.Exists(_configPath), Is.True, "Configuration file should be created by SetConfigFile()");
        }


        [Test]
        public void SaveConfiguration_PersistsCurrentConfig_ToFile()
        {
            // Ensure ConfigFile is initialized by calling LoadConfigurationFile (creates file and sets internal path)
            Configurations.LoadConfigurationFile();

            // Set a configuration to persist
            Config.sourceType = SourceType.Ftp;
            Config.FTPConfig.Host = "persist.example.com";
            Config.FTPConfig.Port = 9999;
            Config.FTPConfig.Username = "persistUser";
            Config.FTPConfig.Password = "persistPass";
            Config.FTPConfig.PathFile = "/persist/path.fscr";
            Config.GoogleConfig.PathFile = "/google/path";

            // Save to file
            Configurations.SaveConfigurationFile();

            // Read raw json and assert it contains persisted values
            var json = File.ReadAllText(_configPath);
            Assert.That(json, Does.Contain("\"Source\": \"Ftp\""));
            Assert.That(json, Does.Contain("\"Host\": \"persist.example.com\""));
            Assert.That(json, Does.Contain("\"Port\": 9999"));
            Assert.That(json, Does.Contain("\"Username\": \"persistUser\""));
            Assert.That(json, Does.Contain("\"PathFile\": \"/persist/path.fscr\""));
            Assert.That(json, Does.Contain("\"PathFile\": \"/google/path\""));
        }

        [Test]
        public void LoadConfiguration_GoogleDrive_ParsesGoogleSection()
        {
            var payload = new
            {
                Source = "GoogleDrive",
                GoogleDrive = new
                {
                    PathFile = "/google/data.fscr"
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);

            var result = Configurations.LoadConfigurationFile();

            Assert.That(result, Is.EqualTo(SourceType.GoogleDrive));
            Assert.That("/google/data.fscr", Is.EqualTo(Config.GoogleConfig.PathFile));
        }

        [Test]
        public void LoadConfiguration_InvalidJson_ReturnsNone()
        {
            // write invalid JSON
            File.WriteAllText(_configPath, "{ this is : invalid json ");

            var result = Configurations.LoadConfigurationFile();

            Assert.That(result, Is.EqualTo(SourceType.None));
        }

        [Test]
        public void SaveConfiguration_WhenConfigFileNull_DoesNotThrow()
        {
            // Use reflection to set the private static ConfigFile to null to hit the early-return branch in SaveConfigurationFile
            var fi = typeof(Configurations).GetField("ConfigFile", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(fi, Is.Not.Null, "Could not find private static field 'ConfigFile' via reflection");

            // Save original value to restore later
            var original = fi.GetValue(null);

            try
            {
                fi.SetValue(null, null);
                Assert.DoesNotThrow(() => Configurations.SaveConfigurationFile());
            }
            finally
            {
                // restore original
                fi.SetValue(null, original);
            }
        }
    }
}