﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace UltimateChanger
{
    public class FileOperator
    {
        private Label lblGenie;
        private Label lblOasis;
        private Label lblExpressFit;
        private ComboBox cmbMarket;
        private List<CheckBox> checkBoxList;
        private List<string> marketIndex;
        private DataBaseManager dataBase;

        private Image imgOticon;
        private Image imgBernafon;
        private Image imgSonic;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genie">Label opisujacy market dla Genie</param>
        /// <param name="oasis">Label opisujacy market dla Oasis</param>
        /// <param name="expressFit">Label opisujacy market dla ExpressFit</param>
        /// <param name="cmbMarket">Combobox z lista marketow</param>
        public FileOperator(DataBaseManager dataBase, Label genie, Label oasis, Label expressFit, ComboBox cmbMarket, List<CheckBox> checkBoxList, List<string> marketIndex, Image imgOticon, Image imgBernafon, Image imgSonic)
        {
            this.dataBase = dataBase;
            this.lblGenie = genie;
            this.lblOasis = oasis;
            this.lblExpressFit = expressFit;
            this.cmbMarket = cmbMarket;
            this.checkBoxList = checkBoxList;
            this.marketIndex = marketIndex;

            this.imgOticon = imgOticon;
            this.imgBernafon = imgBernafon;
            this.imgSonic = imgSonic;
        }


        public bool Get_run_with_Application() // czytanie z pliku czy chcemy zeby byla apka uruchamiana 
        {
            if (File.Exists(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt"))
                    {
                        String line = sr.ReadToEnd();
                        if (line == "1")
                        {
                            sr.Close();
                            return true; // jezeli jest 1 to chceymy zeby sie uruchamiał z windowsem czyli kopiujemy exe do folderu startup
                        }
                        else
                        {
                            sr.Close();
                            return false;
                        }
                        
                    }
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    return false;
                }
            }
            else {
                try
                {
                    File.Create(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt");
                }
                catch (Exception)
                {
                    if(!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW"))
                    Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW");
                    if(!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer"))
                    Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer");
                    File.Create(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt");
                }
                
                return false;
            } 
        }




        public bool Get_delete_logs_file() // czytanie z pliku czy chcemy zeby byly usuwane log mody
        {
            if (File.Exists(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt"))
                    {
                        String line = sr.ReadToEnd();
                        if (line == "1")
                        {
                            sr.Close();
                            return true; // jezeli jest 1 to chceymy zeby usuwaly pliki
                        }
                        else
                        {
                            sr.Close();
                            return false;
                        }

                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    File.Create(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt");
                    return false;
                }
            }
            else
            {

                File.Create(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt");
                return false;
            }
        }



        public static long GetDirectorySize(string parentDirectory) //zwraca rozmiar directory w bajtach
        {
            return new DirectoryInfo(parentDirectory).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
        }

        private String GetData(string name)
        {
            String line = String.Empty;
            int counter = 0;
            if (File.Exists(name))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(name))
                    {
                        while (counter != 4)
                        {
                            line = sr.ReadLine();
                            counter++;
                        }
                        if (line[15] == 'e')
                        {
                            return "Defukt";
                        }
                        return $"{line[14]}{line[15]}";
                    }
                }
                catch (FileNotFoundException ex)
                {
                    dataBase.LogException(ex.ToString(), "getData File Not Found");
                    return "";
                }
                catch (DirectoryNotFoundException ee)
                {
                    dataBase.LogException(ee.ToString(), "getData Directory Not Found");
                    return "";
                }
                catch (NullReferenceException eee)
                {
                    dataBase.LogException(eee.ToString(), "getData NullReference");
                    return "";
                }
            }
            else
            {
                return "File is missing";
            }
        }

        public void UpdateLabels()
        {
            lblGenie.Foreground = new SolidColorBrush(Colors.Black);
            lblOasis.Foreground = new SolidColorBrush(Colors.Black);
            lblExpressFit.Foreground = new SolidColorBrush(Colors.Black);
            lblGenie.Content = GetData("C:/ProgramData/Oticon/Common/ManufacturerInfo.XML");
            lblOasis.Content = GetData("C:/ProgramData/Bernafon/Common/ManufacturerInfo.XML");
            lblExpressFit.Content = GetData("C:/ProgramData/Sonic/Common/ManufacturerInfo.XML");
        }

        public void HandleSelectedMarket()
        {
            int counter = 0;
            bool show = false;
            string[] markets = new string[3];
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    markets[counter] = GetData($"C:/ProgramData/{checkbox.Name}/Common/ManufacturerInfo.XML");
                    counter++;
                }
            }
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked && counter == 1)
                {
                    cmbMarket.SelectedIndex = marketIndex.IndexOf(GetData($"C:/ProgramData/{checkbox.Name}/Common/ManufacturerInfo.XML"));
                }
                else if (counter > 1)
                {
                    for (int i = 0; i < counter; ++i)
                    {
                        if (markets[i] == markets[counter - 1])
                        {
                            show = true;
                        }
                        else
                        {
                            show = false;
                            break;
                        }
                    }
                    if (show)
                    {
                        cmbMarket.SelectedIndex = marketIndex.IndexOf(GetData($"C:/ProgramData/{checkbox.Name}/Common/ManufacturerInfo.XML"));
                    }
                    else
                    {
                        cmbMarket.SelectedIndex = 0;
                    }
                }
                else if (counter == 0)
                {
                    cmbMarket.SelectedIndex = 0;
                }
            }
        }

        public string GetDataFromAbout(string Brand)
        {
            List<string> statement = new List<string>();
            String line = String.Empty;
            string tmp_ip = "";
            int counter = 0;
            if (File.Exists(Brand))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(Brand))
                    {
                        while (counter != 7)
                        {
                            line = sr.ReadLine();

                            if (counter == 3)
                            {
                                statement.Add(line[11].ToString());
                                statement.Add(".");
                            }

                            if (counter == 4)
                            {
                                statement.Add(line[11].ToString());
                                statement.Add(".");
                            }

                            if (counter == 5)
                            {
                                int tmp_counter = 11;
                                while (line[tmp_counter].ToString() != "<")
                                {

                                    tmp_ip = line[tmp_counter].ToString();
                                    statement.Add(tmp_ip);
                                    tmp_counter++;

                                }
                                statement.Add(".");
                            }

                            if (counter == 6)
                            {
                                int tmp_counter = 14;
                                while (line[tmp_counter].ToString() != "<")
                                {
                                    tmp_ip = line[tmp_counter].ToString();
                                    statement.Add(tmp_ip);
                                    tmp_counter++;
                                }
                            }
                            counter++;
                        }
                        string dogCsv = string.Join("", statement.ToArray());

                        return dogCsv;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    dataBase.LogException(ex.ToString(), "GetDataFromAbout File Not Found File Not Found");
                    return "";
                }
                catch (DirectoryNotFoundException ee)
                {
                    dataBase.LogException(ee.ToString(), "GetDataFromAbout Directory Not Found Directory Not Found");
                    return "";
                }
                catch (NullReferenceException eee)
                {
                    dataBase.LogException(eee.ToString(), "GetDataFromAbout NullReference Null Reference");
                    return "";
                }
            }
            return "";

        }

        public void GetIPFromAbout()
        {
            string Ver_Bernafon = GetDataFromAbout("C:/ProgramData/Bernafon/Oasis2/ApplicationVersion.XML");
            string Ver_Sonic = GetDataFromAbout("C:/ProgramData/Sonic/EXPRESSfit2/ApplicationVersion.XML");
            string Ver_Oticon = GetDataFromAbout("C:/ProgramData/Oticon/Genie2/ApplicationVersion.XML");

            imgOticon.ToolTip = $"About: {Ver_Oticon}";
            imgBernafon.ToolTip = $"About: {Ver_Bernafon}";
            imgSonic.ToolTip = $"About: {Ver_Sonic}";
        }

        public List<string> IPFromAbout(List<string> statement2)
        {
            List<string> nazwy = new List<string>();
            List<string> statement = statement2;
            string Ver_Bernafon = GetDataFromAbout("C:/ProgramData/Bernafon/Oasis2/ApplicationVersion.XML");
            string Ver_Sonic = GetDataFromAbout("C:/ProgramData/Sonic/EXPRESSfit2/ApplicationVersion.XML");
            string Ver_Oticon = GetDataFromAbout("C:/ProgramData/Oticon/Genie2/ApplicationVersion.XML");

            string[] Kolekcja_VER = { Ver_Oticon, Ver_Bernafon, Ver_Sonic };
            imgOticon.ToolTip = "About: " + Ver_Oticon;
            imgBernafon.ToolTip = "About: " + Ver_Bernafon;
            imgSonic.ToolTip = "About: " + Ver_Sonic;

            string TMP = Ver_Oticon;

            int licznik = 0;
            foreach (var Ver in Kolekcja_VER)
            {
                int i = 0;
                TMP = Ver;
                if (Ver == "")
                {
                    licznik++;
                }
                else
                {
                    foreach (var item in statement)
                    {
                        if (item == TMP)
                        {
                            int ktory_rekord = i / 12;
                            string tmp_ktorybuild_directory = statement[(ktory_rekord * 12) + 2];

                            nazwy.Add(statement[(ktory_rekord * 12) + 5]);
                            nazwy.Add(statement[(ktory_rekord * 12) + 6]);
                            nazwy.Add(statement[(ktory_rekord * 12) + 7]);

                            if (licznik == 0)
                            {
                                imgOticon.ToolTip = $"Dir: { nazwy[0]}\nAbout: {Ver_Oticon}";
                            }
                            if (licznik == 1)
                            {
                                imgBernafon.ToolTip = $"Dir: { nazwy[1]}\nAbout: {Ver_Oticon}";
                            }
                            if (licznik == 2)
                            {
                                imgSonic.ToolTip = $"Dir: { nazwy[2]}\nAbout: {Ver_Oticon}";
                            }
                        }
                        i++;
                    }
                    licznik++;
                }
            }
            return nazwy;
        }

    }
}
