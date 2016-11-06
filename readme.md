# Delete old files on FTP with Azure Function

## App Settings
In order for this Azure function to run properly, some app settings need to be set.

* FTPServerURI: The FTP Uri, ex. ftp://myftpserver/myfolder/...
* FTPUsername: The FTP username
* FTPPassword: The FTP users password 

## Continuous Integration
This repository contains a production branch. This branch can be used for auto-deployment to your azure function.