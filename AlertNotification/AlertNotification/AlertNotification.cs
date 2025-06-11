using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AlertNotification
{
    public partial class AlertNotification : ServiceBase
    {
        ConnDB db;
        ConnFile log;
        Timer timer = new Timer();
        string url, token, sql, send_to_line, fixed_minutes, path_b, batch;
        int interval, next_alert;
        string[] minutes;
        public AlertNotification()
        {
            InitializeComponent();
            db = ConnDB.GetInstance;
            log = new ConnFile(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + DateTime.Now.ToString("yyyy_MM_dd") + ".log");
        }

        protected override void OnStart(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            log.WriteToFile("-----------------------------------------------------------------------------------------------------------------");
            log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Service Started.");

            log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Start reading config file.");
            ConnFile file = new ConnFile(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
            List<string> rl = file.GetAllFile();
            foreach (string s in rl)
            {
                if (s.Substring(0, 1) != "#") {
                    if (s.Contains("url"))
                    {
                        url = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("token"))
                    {
                        token = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("sql"))
                    {
                        sql = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("sendToLine"))
                    {
                        send_to_line = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("nextAlert"))
                    {
                        next_alert = Convert.ToInt32(s.Substring(s.IndexOf("=") + 1));
                    }
                    else if (s.Contains("fixedMinuteMode"))
                    {
                        fixed_minutes = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("minutes"))
                    {
                        minutes = s.Substring(s.IndexOf("=") + 1).Split(',');
                    }
                    else if (s.Contains("interval"))
                    {
                        interval = Convert.ToInt32(s.Substring(s.IndexOf("=") + 1));
                    }
                    else if (s.Contains("path_batch"))
                    {
                        path_b = s.Substring(s.IndexOf("=") + 1);
                    }
                    else if (s.Contains("batch_send_img"))
                    {
                        batch = s.Substring(s.IndexOf("=") + 1);
                    }
                    //log.WriteToFile(s);
                }
            }
            file = null;

            //RunBatch(path_b, batch);
            Run();

            // set INTERVAL
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = interval;
            timer.Enabled = true;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            log.SetPath(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + DateTime.Now.ToString("yyyy_MM_dd") + ".log");
            Run();
        }

        protected override void OnStop()
        {
            log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Service Stopped.");
            log.WriteToFile("-----------------------------------------------------------------------------------------------------------------");
        }

        private void Run()
        {
            bool run = false;
            string[] token_arr;
            token_arr = token.Split(',');
            if (fixed_minutes == "yes")
            {
                if (CheckMinute()) { run = true; }
            }
            else { run = true; }

            if (run)
            {
                //MouseOperations ins = new MouseOperations();
                //ins.DoMouseClick();
                //RunPython(@"E:\Notify_IMG\cap_rf.py");
                //run_cmd(@"E:\Notify_IMG\dist\cap_wl\cap_wl.exe");
                //string command;
                //command = @"python E:\Notify_IMG\cap_rf.py";
                //ExecuteCommand(command);
                //command = @"python E:\Notify_IMG\cap_wl.py";
                //ExecuteCommand(command);
                //command = @"python E:\Notify_IMG\cap_fl.py";
                //ExecuteCommand(command);
                //RunBatch(@"E:\Notify_IMG\", "batch_cap_all.bat");
                log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: ***************** STARTED *****************");
                log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Get data from database.");
                string result, msg_all;
                string[] msg;
                result = this.db.GetMsgFromDB(this.sql, this.next_alert);

                if (result.Length > 0)
                {
                    msg_all = "";
                    msg = result.Split('|');
                    //foreach (string m in msg)
                    for (int i=0; i<msg.Length; i++)
                    {
                        log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Send the message: " + msg[i]);
                        //if (this.send_to_line == "yes") { LineNotify(this.token, "\n" + m); }
                        msg_all += "\n" + msg[i];
                        if (i < msg.Length - 1) { msg_all += "\n" + "-------"; }
                    }
                    if (this.send_to_line == "yes")
                    {
                        //log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Capture all images");
                        //RunBatch(@"E:\Notify_IMG\", "batch_cap_all.bat");
                        //RunBatch(@"E:\Notify_IMG\", "batch_cap_all.bat");
                        log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Send the message via LINE");
                        //LineNotify(this.token, msg_all);
                        foreach (string t in token_arr) { LineNotify(t, msg_all); }
                        log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: Send all images");
                        //RunBatch(@"E:\Notify_IMG\", "batch_send_all_img.bat");
                        RunBatch(path_b, batch);
                    }
                }
                else { log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: No message on this time"); }

                log.WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + "INFO: ***************** FINISHED *****************");
            }
        }

        private bool CheckMinute()
        {
            bool match = false;
            foreach (string s in minutes)
            {
                if (Convert.ToInt32(s) == DateTime.Now.Minute) { match = true; break; }
            }
            return match;
        }

        private void LineNotify(string lineToken, string message)
        {
            try
            {
                //string message = System.Web.HttpUtility.UrlEncode(message, Encoding.UTF8);
                // var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var request = (HttpWebRequest)WebRequest.Create(this.url);
                var postData = string.Format("message={0}", message);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + lineToken);
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void LineNotify(string lineToken, string message, int stickerPackageID, int stickerID, string pictureUrl)
        {
            try
            {
                //string message = System.Web.HttpUtility.UrlEncode(message, Encoding.UTF8);
                //var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var request = (HttpWebRequest)WebRequest.Create(this.url);
                var postData = string.Format("message={0}", message);
                if (stickerPackageID > 0 && stickerID > 0)
                {
                    var stickerPackageId = string.Format("stickerPackageId={0}", stickerPackageID);
                    var stickerId = string.Format("stickerId={0}", stickerID);
                    postData += "&" + stickerPackageId.ToString() + "&" + stickerId.ToString();
                }
                if (pictureUrl != "")
                {
                    var imageThumbnail = string.Format("imageThumbnail={0}", pictureUrl);
                    var imageFullsize = string.Format("imageFullsize={0}", pictureUrl);
                    postData += "&" + imageThumbnail.ToString() + "&" + imageFullsize.ToString();
                }
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + lineToken);
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }

        private void RunBatch(string path, string filename)
        {
            string batDir = string.Format(@path);
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = batDir;
            proc.StartInfo.FileName = filename;
            proc.StartInfo.CreateNoWindow = false;
            proc.Start();
            proc.WaitForExit();
        }

        private void RunPython(string path)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Python39\python.exe";

            var script = path;
            psi.Arguments = $"\"{script}\"";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            var errors = "";
            var results = "";

            using (var process = Process.Start(psi))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }
        }

    }
}
