# NetCoreFileAccess

    Backend for access to files from different sources 
	
## Description
    
## Dependencies

    NetCore 9.0
## Help
    Configuration file: Placed in the sub folder "conf", with the name "config.json". The content should be like this:
    
    {
        "Source": "XXXXX",
        "Ftp": {
            "Host": "Host_name_or_IP",
            "Port": 000,
            "Username": "my_user",
            "Password": "my_password_",
            "PathFile": "PathFolder/my_filedata.fscr"
        },
        "GoogleDrive": {
            "PathFile": "PathFolder/my_filedata.fscr"
        }
    }
    
    Where:
        Source - Source of the file, can be "Local", "Ftp" or "GoogleDrive"


## Authors
    Martí Soler - https://github.com/martisdev

    
## Version History

    1.0.0.0
        Initial Release
        Sources: Local, Ftp, GoogleDrive

## License

	GNU Affero General Public License v3.0