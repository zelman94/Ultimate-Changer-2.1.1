using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql;
using System.Diagnostics;
using System.Windows.Forms;

namespace UltimateChanger
{
    /// <summary>
    /// Matko bosko... w tej klasie jest taki syf, że o Jezus.
    /// </summary>
    public class DataBaseManager
    {
        public MySqlConnection SQLConnection;
        private ClickCounter clickCounter;
        public string pathsToUpdate="";
        public bool DB_connection ; //jezeli jest polaczenie z BD 
        private Stopwatch time;
        public string APPversion;
        public string GetInformationFromDataBase()
        {

            string info = "";
            if (DB_connection)
            {
                try
                {
                    SQLConnection.Open();

                    MySqlDataReader myReader;
                    using (MySqlCommand myCommand = new MySqlCommand($"SELECT information FROM information", SQLConnection))
                    {
                        myReader = myCommand.ExecuteReader();
                        if (myReader.Read())
                        {
                            info = myReader["information"].ToString();
                        }
                        myReader.Close();
                    }
                    SQLConnection.Close();
                }
                catch (Exception ee)
                {

                    LogException(ee.ToString(), "GetInformationFromDataBase");
                }
            }
            return info;
        }

        public string GetActualVersion(string brand)
        {
            string IPVersion = "";
            string PreBrand = "";
            if (DB_connection)
            {
                switch (brand)
                {
                    case "Oticon":
                        IPVersion = "GenieIP";
                        PreBrand = "Genie_";
                        break;
                    case "Bernafon":
                        IPVersion = "Oasis_IP";
                        PreBrand = "Oasis_";
                        break;
                    case "Sonic":
                        IPVersion = "EF_IP";
                        PreBrand = "ExpressFit_";
                        break;
                }

                SQLConnection.Open();
                MySqlDataReader myReader;
                try
                {
                    using (MySqlCommand myCommand = new MySqlCommand($"SELECT {IPVersion} FROM BD_FOR_MultiChanger_DGS WHERE actual = 1", SQLConnection))
                    {
                        myReader = myCommand.ExecuteReader();
                        if (myReader.Read())
                        {
                            PreBrand = $"{PreBrand}{myReader[IPVersion].ToString()}";
                        }
                        myReader.Close();
                    }
                }
                catch (Exception)
                {
                    return "";
                    SQLConnection.Close();
                }               

                SQLConnection.Close();
            }
            return PreBrand;
        }


        public string GetIPVersion(string brand, string about)
        {
            string IPVersion = "";
            string PreBrand = "";
            if (DB_connection)
            {
                switch (brand)
                {
                    case "Genie":
                        IPVersion = "GenieIP";
                        PreBrand = "Genie_";
                        brand = "Oticon";
                        break;
                    case "Oasis":
                        IPVersion = "Oasis_IP";
                        PreBrand = "Oasis_";
                        brand = "Bernafon";
                        break;
                    case "ExpressFit":
                        IPVersion = "EF_IP";
                        PreBrand = "ExpressFit_";
                        brand = "Sonic";
                        break;
                }




                
                MySqlDataReader myReader;
                try
                {
                    SQLConnection.Open();
                    using (MySqlCommand myCommand = new MySqlCommand($"SELECT {IPVersion} FROM BD_FOR_MultiChanger_DGS WHERE About_{brand} ={about}", SQLConnection))
                    {
                        myReader = myCommand.ExecuteReader();
                        if (myReader.Read())
                        {
                            PreBrand = $"{PreBrand}{myReader[IPVersion].ToString()}";
                        }
                        myReader.Close();
                    }
                }
                catch (Exception)
                {
                    SQLConnection.Close();                    
                }
              

                SQLConnection.Close();
            }
            return PreBrand;
        }



        public string GetDirectoryName(string about, string brand)
        {
            string IPVersion = "";
            string directory = "Sorry, no info :<";
            string AboutVersion = "";
            if (DB_connection)
            {
                switch (brand)
                {
                    case "Genie":
                        IPVersion = "GenieIP";
                        AboutVersion = "About_Oticon";
                        break;
                    case "Oasis":
                        IPVersion = "Oasis_IP";
                        AboutVersion = "About_Bernafon";
                        break;
                    case "EF":
                        IPVersion = "EF_IP";
                        AboutVersion = "About_Sonic";
                        break;
                }
                try
                {
                    SQLConnection.Open();
                }
                catch (Exception ee)
                {
                    LogException(ee.ToString(), "GetDirectoryName");
                }



                MySqlDataReader myReader;
                using (MySqlCommand myCommand = new MySqlCommand($"SELECT {IPVersion} FROM BD_FOR_MultiChanger_DGS WHERE {AboutVersion} = {about}", SQLConnection))
                {
                    try
                    {
                        myReader = myCommand.ExecuteReader();
                        if (myReader.Read())
                        {
                            directory = myReader[IPVersion].ToString();
                        }
                        myReader.Close();
                        SQLConnection.Close();
                    }
                    catch (Exception ee2)
                    {
                        LogException(ee2.ToString(), "GetDirectoryName");

                    }

                }
            }
            return directory;
        }

        private string FormatElementOfDate(string dateElement)
        {
            if (int.Parse(dateElement) < 10)
            {
                return $"0{dateElement}";
            }
            else
            {
                return dateElement;
            }
        }

        private string GetFormattedTime()
        {
            DateTime now = DateTime.Now.ToLocalTime();
            return $"{FormatElementOfDate(now.Day.ToString())}.{FormatElementOfDate(now.Month.ToString())}.{now.Year} {FormatElementOfDate(now.Hour.ToString())}:{FormatElementOfDate(now.Minute.ToString())}:{FormatElementOfDate(now.Second.ToString())}";
        }

        public DataBaseManager(ClickCounter clickCounter, Stopwatch time)
        {
            this.clickCounter = clickCounter;
            this.time = time;

            SQLConnection = ConnectToDB();
            try
            {
                SQLConnection.Open();
                SQLConnection.Close();
                DB_connection = true;
            }
            catch (Exception)
            {
                DB_connection = false;
                MessageBox.Show("no acess to DB");

            }


             
        }

        private MySqlConnection ConnectToDB()
        {
            try
            {
                //string tmp = "server=zadanko-z-zutu.cba.pl;" +
                //                    "database=zelman;" +
                //                   "uid=zelman;" +
                //                   "password=Santiego94;";



                string tmp = "server=10.128.64.19;" +
                                    "database=zelman;" +
                                   "uid=changer;" +
                                   "password=changer;";

                MySqlConnection sqlConn = new MySqlConnection(tmp);
                sqlConn.Open();
                sqlConn.Close();
                return sqlConn;
            }

            catch (Exception e)
            {
                Console.WriteLine("Wystąpił nieoczekiwany błąd!");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public void LogToDB()
        {
            if (DB_connection)
            {
                try
                {
                    time.Stop();
                    TimeSpan elapsedMs = time.Elapsed;
                    int check_ = elapsedMs.Seconds;
                    string tmp_time = "";
                    if (elapsedMs.Minutes >= 1 && elapsedMs.Minutes < 60 && elapsedMs.Hours < 1)
                    {
                        tmp_time = ($" {elapsedMs.Minutes.ToString()}.{ elapsedMs.Seconds.ToString()}  min");
                    }
                    if (elapsedMs.Hours >= 1)
                    {
                        tmp_time = ($" {elapsedMs.Hours.ToString()}.{ elapsedMs.Minutes.ToString()}  h");
                    }
                    if (elapsedMs.Minutes < 1 && elapsedMs.Hours < 1)
                    {
                        tmp_time = ($" { elapsedMs.Seconds.ToString()}.{ elapsedMs.Milliseconds.ToString()}  s");
                    }

                    if (SQLConnection != null)
                    {
                        SQLConnection.Open();
                        DateTime now = DateTime.Now.ToLocalTime();
                        string date_tmp = GetFormattedTime();
                        MySqlCommand myCommand = new MySqlCommand($"INSERT INTO Logs VALUES ('{Environment.UserName}','{tmp_time}',{clickCounter.SumOfClicks},'{date_tmp}')", SQLConnection);
                        myCommand.ExecuteNonQuery();
                        myCommand = new MySqlCommand($"INSERT INTO Advance_logs VALUES ({clickCounter.Clicks[(int)Buttons.All]},{clickCounter.Clicks[(int)Buttons.StartHAttori]},{clickCounter.Clicks[(int)Buttons.UninstallFittingSoftware]},{clickCounter.Clicks[(int)Buttons.UpdateMarket]},{clickCounter.Clicks[(int)Buttons.DeleteLogs]},{clickCounter.Clicks[(int)Buttons.UpdateMode]},'{date_tmp}','{Environment.UserName}','{clickCounter.Clicks[(int)Buttons.InstallFittingSoftware]}','{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}')", SQLConnection);
                        myCommand.ExecuteNonQuery();
                        SQLConnection.Close();
                    }
                }
                catch (Exception ee)
                {
                    LogException(ee.ToString(), "set_logs_to_DB");
                    SQLConnection.Close();
                }
                SQLConnection.Close();
            }
        }     

        public void LogException(string log, string funkcja)
        {
         
            if (DB_connection)
            {
                try
                {
                    string tmp_time = "";
                    if (SQLConnection != null)
                    {
                        TimeSpan elapsedMs = time.Elapsed;
                        int check_ = elapsedMs.Seconds;

                        if (elapsedMs.Minutes >= 1 && elapsedMs.Minutes < 60 && elapsedMs.Hours < 1)
                        {
                            tmp_time = ($" {elapsedMs.Minutes.ToString()}.{ elapsedMs.Seconds.ToString()}  min");
                        }
                        if (elapsedMs.Hours >= 1)
                        {
                            tmp_time = ($" {elapsedMs.Hours.ToString()}.{ elapsedMs.Minutes.ToString()}  h");
                        }
                        if (elapsedMs.Minutes < 1 && elapsedMs.Hours < 1)
                        {
                            tmp_time = ($" { elapsedMs.Seconds.ToString()}.{ elapsedMs.Milliseconds.ToString()}  s");
                        }

                        string TodayDate = GetFormattedTime();
                        SQLConnection.Open();
                        MySqlCommand myCommand = new MySqlCommand($"INSERT INTO Wyjatki VALUES ('{tmp_time}','{Environment.UserName}','{log}','{funkcja}','{TodayDate}')", SQLConnection);
                        myCommand.ExecuteReader();
                        SQLConnection.Close();
                    }
                }
                catch (Exception)
                {
                    SQLConnection.Close();
                }
            }
        }

        public bool getInformation_DB()
        {
            if (DB_connection)
            {
                List<string> Kolumna = new List<string>();
                try
                {
                    if (SQLConnection != null)
                    {
                        SQLConnection.Open();

                        MySqlCommand myCommand = new MySqlCommand("SELECT * FROM information", SQLConnection);
                        MySqlDataReader myReader;
                        myReader = myCommand.ExecuteReader();
                        myReader.Read();
                        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                        Kolumna.Add(myReader.GetString(0)); //update 1 = true
                        Kolumna.Add(myReader.GetString(1)); // path update string
                        Kolumna.Add(myReader.GetString(2)); // info 1 = true
                        Kolumna.Add(myReader.GetString(3)); // information string 
                        Kolumna.Add(myReader.GetString(4)); // information version update

                        string tmp = Kolumna[4];
                        //-----------------------------------
                        int[] ver = new int[3]; // wersja z srvera
                        int.TryParse(tmp[0].ToString(), out ver[0]);
                        int.TryParse(tmp[2].ToString(), out ver[1]);
                        int.TryParse(tmp[4].ToString(), out ver[2]);
                        //-----------------------------------------
                        //wersja apki
                        int[] ver_apki = new int[3];

                        int.TryParse(version[0].ToString(), out ver_apki[0]);
                        int.TryParse(version[2].ToString(), out ver_apki[1]);
                        int.TryParse(version[4].ToString(), out ver_apki[2]);
                        APPversion = version.ToString();
                        bool message = false;
                        for (int i = 0; i < 3; i++)
                        {
                            if (ver_apki[i] < ver[i] && message == false)
                            {
                                MessageBox.Show($"Update available: {Kolumna[1]}");
                                pathsToUpdate = Kolumna[1];
                                message = true;
                            }
                        }
                        if (message)
                        {
                            SQLConnection.Close();
                            return true;
                        }
                        else
                        {
                            SQLConnection.Close();
                            return false;
                        }
                        
                    }
                }

                catch (Exception ee)
                {

                    LogException(ee.ToString(), "getInformation_DB");
                    SQLConnection.Close();
                    return false;
                }
                SQLConnection.Close();
            }
            return false;
        }

    }
}
