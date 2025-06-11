using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertNotification
{
    class ConnDB
    {
        string connection_string;
        SqlConnection cnn;
        private static ConnDB instance = null;

        public static ConnDB GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnDB();
                }
                return instance;
            }
        }

        private ConnDB()
        {
            this.connection_string = @"Data Source=[sorce];Initial Catalog=[db_name];User ID=[user];Password=[password]";
            cnn = new SqlConnection(connection_string);
        }

        public string GetMsgFromDB(string sql, int next_alert)
        {
            string result = "";
            string st_code, parameter, update_sql;
            string[] tmpStr;
            int alert, num_alert, time_now, time_alert, dt_row;
            DateTime dt_start, next_time;
            SqlCommand cmd;
            SqlDataReader data_reader;
            DataTable dt;

            ConnectDB();
            cmd = new SqlCommand(sql, cnn);
            data_reader = cmd.ExecuteReader();
            dt = new DataTable();
            dt.Load(data_reader);
            data_reader.Close();
            cmd.Dispose();
            CloseDB();

            dt_row = 0;

            while (dt_row < dt.Rows.Count)    //(data_reader.Read())
            {
                //st_code = data_reader.GetString(0);
                //parameter = data_reader.GetString(1);
                //dt_start = data_reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm");
                //alert = data_reader.GetInt32(3);
                //num_alert = data_reader.GetInt32(4);
                //next_time = data_reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm");
                st_code = (string)dt.Rows[dt_row][0];
                parameter = (string)dt.Rows[dt_row][1];
                dt_start = (DateTime)dt.Rows[dt_row][2];
                alert = (int)dt.Rows[dt_row][3];
                num_alert = (int)dt.Rows[dt_row][4];
                next_time = (DateTime)dt.Rows[dt_row][5];

                time_alert = (((((((next_time.Year * 12) + next_time.Month) * 30) + next_time.Day) * 24) + next_time.Hour) * 60) + next_time.Minute;
                time_now = (((((((DateTime.Now.Year * 12) + DateTime.Now.Month) * 30) + DateTime.Now.Day) * 24) + DateTime.Now.Hour) * 60) + DateTime.Now.Minute;

                if (time_now >= time_alert)
                //tmpStr = next_time.ToString("yyyy-MM-dd HH:mm").Split(' ');
                //if (DateTime.Now.ToString("yyyy-MM-dd") == tmpStr[0])       // CEHCK DATE
                {
                    //tmpStr = tmpStr[1].Split(':');
                    //time_alert = (Convert.ToInt32(tmpStr[0]) * 60) + Convert.ToInt32(tmpStr[1]);
                    //time_now = (Convert.ToInt32(DateTime.Now.ToString("HH")) * 60) + Convert.ToInt32(DateTime.Now.ToString("mm"));

                    //if (time_now >= time_alert)                         // CHECK TIME
                    //{
                        //result += data_reader.GetValue(6) + "|";
                        result += (string)dt.Rows[dt_row][6] + "|";

                        /** UPDATE alert status **/
                        update_sql = "";
                        if (alert == 1)             // CALCULATE next alert
                        {
                            update_sql = "update [Tbl_Alert_Notification] set num_alert=" + (num_alert + 1).ToString() + ", next_alert_time=dateadd(minute, " + next_alert.ToString() + ", '" + next_time.ToString("yyyy-MM-dd HH:mm") + "') " + ", last_update = getdate() " +
                                " where st_code='" + st_code + "' and parameter='" + parameter + "' and datetime_start='" + dt_start.ToString("yyyy-MM-dd HH:mm") + "'";
                        }
                        else if (alert == 2)        // alert finish
                        {
                            update_sql = "update [Tbl_Alert_Notification] set alert=0, num_alert=" + (num_alert + 1).ToString() + ", last_update = getdate() " +
                                " where st_code='" + st_code + "' and parameter='" + parameter + "' and datetime_start='" + dt_start.ToString("yyyy-MM-dd HH:mm") + "'";
                        }
                        if (update_sql != "") { ExecuteToDB(update_sql); }
                        
                    //}
                }

                dt_row++;
            }

            if (result.Length > 0) { result = result.Substring(0, result.Length - 1); }

            return result;
        }

        public string GetDataFromDB(string sql)
        {
            string result = "";
            SqlCommand cmd;
            SqlDataReader data_reader;

            ConnectDB();
            cmd = new SqlCommand(sql, cnn);
            data_reader = cmd.ExecuteReader();
            while (data_reader.Read())
            {
                result += data_reader.GetValue(0);
            }
            data_reader.Close();
            cmd.Dispose();
            CloseDB();

            return result;
        }

        public void ExecuteToDB(string sql)
        {
            SqlCommand cmd;
            SqlDataAdapter adp = new SqlDataAdapter();

            ConnectDB();
            cmd = new SqlCommand(sql, cnn);
            adp.InsertCommand = cmd;
            adp.InsertCommand.ExecuteNonQuery();
            adp.Dispose();
            cmd.Dispose();
            CloseDB();
        }

        private void ConnectDB() { if (cnn.State == System.Data.ConnectionState.Closed) { cnn.Open(); } }
        private void CloseDB() { if (cnn.State == System.Data.ConnectionState.Open) { cnn.Close(); } }
    }
}
