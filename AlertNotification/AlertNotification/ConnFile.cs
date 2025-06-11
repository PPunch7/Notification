using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertNotification
{
    class ConnFile
    {
        string location;

        public ConnFile(string path)
        {
            this.location = path;
        }

        public void SetPath(string path) { this.location = path; }

        public List<string> GetAllFile()
        {
            List<string> data = new List<string>();
            StreamReader sr;
            string line;

            try
            {
                sr = new StreamReader(this.location);
                do
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        data.Add(line);
                    }
                } while (line != null);
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            return data;
        }

        public void WriteToFile(string text)
        {
            try
            {
                //StreamWriter sw = new StreamWriter(this.location);
                if (!File.Exists(this.location))
                {
                    using (StreamWriter sw = File.CreateText(this.location))
                    {
                        sw.WriteLine(text);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(this.location))
                    {
                        sw.WriteLine(text);
                        sw.Close();
                    }
                }
                //sw.WriteLine(text);
                //sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }
    }
}
