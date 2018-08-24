using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Data.SqlClient;
using MySql;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms.Integration;
using System.Text.RegularExpressions;

[assembly: System.Reflection.AssemblyVersion("2.1.1.0")]
namespace UltimateChanger
{
    public partial class MainWindow : Window
    {        
        TrashCleaner Cleaner;
        FileOperator fileOperator;
        ClickCounter clickCounter;
        ClockManager clockManager;
        DataBaseManager dataBaseManager;
        DispatcherTimer dispatcherTimer;
        DispatcherTimer uninstallTimer;
        string Ver_Bernafon;
        string Ver_Sonic;
        string Ver_Oticon;
        string OEMname = "";

        //DATA BASE
        //__________________________________________

        SortedDictionary<string, string> market;
        SortedDictionary<string, string> mode;
        SortedDictionary<string, string> Settings;
        SortedDictionary<string, string> brands;
        SortedDictionary<string, int> builde;
        Dictionary<string, string> BrandtoSoft;
        List<string> marketIndex;
        List<CheckBox> checkBoxList;
        public List<string> FoldersInIP = new List<string>();
        string Releases_prereleases ="";
        string Directory_toIntall = "";
        DirectoryInfo[] nameFolders;
        DoubleAnimation blinkAnimation;
        Stopwatch start_time = Stopwatch.StartNew();
        public int counter2 = 0;
        string[] marki = { "Genie", "Oasis", "EXPRESSfit" };
        string[] procesy_uninstall = { "GenieOticon", "OasisBernafon", "ExpressFitSonic" };

        public MainWindow()
        {
            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists) // jezeli wiecej niz 1 instancja to nie uruchomi sie
            {
                System.Environment.Exit(1);
            }

            try
            {


            if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW"))
            {
                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW");
                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer");
            }
            //Tworzenie list/słowników elementów itd.
            InitializeComponent();
            initializeElements();
            bindMarketDictionary();

            //__________Inicjalizacja klas
            clickCounter = new ClickCounter(8);
            dataBaseManager = new DataBaseManager(clickCounter, start_time);
            Cleaner = new TrashCleaner(BrandtoSoft, dataBaseManager);
            fileOperator = new FileOperator(dataBaseManager, lblG, lblO, lblE, cmbMarket, checkBoxList, marketIndex, imgOticon_Copy, imgBernafon, imgSonic);

            //TIMER
            dispatcherTimer = new DispatcherTimer();
            uninstallTimer = new DispatcherTimer();
            initializeTimers();
            //=====
            
            if (dataBaseManager.getInformation_DB())
            {
                lblVersion.Content = dataBaseManager.APPversion;
                infoUpdate.Content = "update is available";
                infoUpdate.ToolTip = dataBaseManager.pathsToUpdate;
            }
            else
            {
                lblVersion.Content = dataBaseManager.APPversion;
                infoUpdate.Content = "";
                string tmp;
                try
                {
                     tmp = lblVersion.Content.ToString();
                    lblVersion.Content = tmp + " Application is up-to-date";
                }
                catch (Exception)
                {
                    tmp = "";
                    lblVersion.Content = tmp + "";
                }
               
                
            }


            Ver_Bernafon = fileOperator.GetDataFromAbout("C:/ProgramData/Bernafon/Oasis2/ApplicationVersion.XML");
            Ver_Sonic = fileOperator.GetDataFromAbout("C:/ProgramData/Sonic/EXPRESSfit2/ApplicationVersion.XML");
            Ver_Oticon = fileOperator.GetDataFromAbout("C:/ProgramData/Oticon/Genie2/ApplicationVersion.XML");




            //======================TESTOWANIE DATY
            clockManager = new ClockManager();
            lblTime.Content = clockManager.GetTime();



            fileOperator.UpdateLabels();
           // fileOperator.GetIPFromAbout();
         //   nazwy_IP_zDirectory = fileOperator.IPFromAbout();
            verifyInstalledBrands();
             
            bindlogmode();


            //worker = new BackgroundWorker();
            //worker.DoWork += updateUI;
            //worker.RunWorkerAsync();
        
            cbindBrandsToInstall();

            string path = Directory.GetCurrentDirectory();
            imgSonic.Source = new BitmapImage(new Uri($"{path}/sonic2.png", UriKind.Absolute));
            imgOticon.Source = new BitmapImage(new Uri($"{path}/oticon2.png", UriKind.Absolute));
            imgBernafon.Source = new BitmapImage(new Uri($"{path}/bernafon2.png", UriKind.Absolute));
            
            btnDelete.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnLogMode.IsEnabled = false;
            btnHattori.IsEnabled = false;
            btnuninstal.IsEnabled = false;
            btnDeletelogs.IsEnabled = false;
            btnFS.IsEnabled = false;
            RBnormal.IsChecked = true;
            rbnStartwithWindows.IsChecked = false;
            rbnNotStartwithWindows.IsChecked = true;
            rbnholdlogs.IsChecked = true;
            cmbLogSettings.SelectedIndex = -1;




                if (fileOperator.Get_run_with_Application())
            {
                rbnStartwithWindows.IsChecked = true;
                rbnNotStartwithWindows.IsChecked = false;
            }
            else
            {
                rbnNotStartwithWindows.IsChecked = true;
                rbnStartwithWindows.IsChecked = false;
            }

            //  SetLogSettings(); //ustawienia log mode  zrobic jak przy autostarcie apki radiobuttony i tutaj if 




            if (dataBaseManager.DB_connection)
            {
                cmbBrandstoinstall.SelectedIndex = 6; //z difolta dajmy sobie ze genie bedzie wybrane przy inicjalizacji
                infoConnectToBaza.Content = "";
            }
            else
            {
                infoConnectToBaza.Foreground = new SolidColorBrush(Colors.Red);
                infoConnectToBaza.Content = "Connection with Data Base is lost";
            }
            
            TemporaryToolTipMethod();
            info.Text = dataBaseManager.GetInformationFromDataBase();



            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }



            
        }
        //________________________________________________________________________________________________________________________________________________
        void initializeTimers()
        {
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 15);
            dispatcherTimer.Start();

            uninstallTimer.Tick += checkUninstallation_Tick;
            uninstallTimer.Interval = new TimeSpan(0, 0, 5);
        }   

        void TemporaryToolTipMethod()
        {
            List<string> brands = new List<String>()
            {
                "C:/ProgramData/Bernafon/Oasis2/ApplicationVersion.XML",
                "C:/ProgramData/Sonic/EXPRESSfit2/ApplicationVersion.XML",
                "C:/ProgramData/Oticon/Genie2/ApplicationVersion.XML"
            };
            Image[] images = { imgBernafon, imgSonic, imgOticon_Copy };
            String[] brandely = { "Oasis", "EF", "Genie" };
           
            string about;
            string directory;
            int i = 0;
            foreach (string brand in brands)
            {
                about = fileOperator.GetDataFromAbout(brand);
                directory = dataBaseManager.GetDirectoryName($"'{about}'", brandely[i]);
                if (about=="")
                {
                    images[i].ToolTip = "no data";
                }
                else
                images[i].ToolTip = $"about: {about}\ndir: {directory}";
                i++;
            }
        }

        void bindMarketDictionary()
        {
            market = new SortedDictionary<string, string>
            {
                { "Australia (AU)", "AU"},
                { "Denmark (DK)", "DK"},
                { "Germany (DE)", "DE"},
                { "United Kingdom (UK)", "UK"},
                { "United States (US)", "US"},
                { "Canada (CA)", "CA"},
                { "Spain (ES)", "ES"},
                { "New Zeland (NZ)", "NZ"},
                { "Switzerland (CH)", "CH"},
                { "Finland (FI)", "FI"},
                { "France (FR)", "FR"},
                { "Italy (IT)", "IT"},
                { "Japan (JP)", "JP"},
                { "Korea (KR)", "KR"},
                { "Norway (NO)", "NO"},
                { "Nederland (NL)", "NL"},
                { "Brazil (BR)", "BR"},
                { "Poland (PL)", "PL"},
                { "Portugal (PT)", "PT"},
                { "Sweden (SE)", "SE"},
                { "Singapore (SG)", "SG"},
                { "PRC China (CN)", "CN"},
                { "South Africa (ZA)", "ZA"},
                { "", "NA"}
            };

            marketIndex = new List<string>()
            {
                {"NA"},
                {"AU"},
                {"BR"},
                {"CA"},
                {"DK"},
                {"FI"},
                {"FR"},
                {"DE"},
                {"IT"},
                {"JP"},
                {"KR"},
                {"NL"},
                {"NZ"},
                {"NO"},
                {"PL"},
                {"PT"},
                {"CN"},
                {"SG"},
                {"ZA"},
                {"ES"},
                {"SE"},
                {"CH"},
                {"UK"},
                {"US"},
                {"he"}
            };

            BrandtoSoft = new Dictionary<string, string>()
            {
                {"Oticon", "Genie"},
                {"Bernafon", "Oasis"},
                {"Sonic", "ExpressFit"},
                {"Genie", "Oticon"},
                {"Oasis", "Bernafon"},
                {"EXPRESSFit", "Sonic"},
                {"Genie_N", "Oticon"},
                {"Oasis_N", "Bernafon"},
                {"EXPRESSfit_N", "Sonic"}
            };

            cmbMarket.ItemsSource = market;
            cmbMarket.DisplayMemberPath = "Key";
            cmbMarket.SelectedValuePath = "Value";
        }

        void initializeElements()
        {
            checkBoxList = new List<CheckBox>()
            {
                Oticon,
                Bernafon,
                Sonic
            };

            string[] sources =
            {
                "C:/ProgramData/Oticon/Common/ManufacturerInfo.XML",
                "C:/ProgramData/Bernafon/Common/ManufacturerInfo.XML",
                "C:/ ProgramData/Sonic/Common/ManufacturerInfo.XML"
            };
        }

        //________________________________________________________________________________________________________________________________________________

        private bool checkInstallationStatus() // true jezeli nie instaluje sie
        {
           
                int ile_tru = 0;
                bool endInstall = false;
               
                    btninstal.IsEnabled = false;
                    for (int counter = 0; counter < 3; counter++)
                    {
                        endInstall = this.checkRunningProcess(procesy_uninstall[counter]);
                        if (endInstall == true)
                        {
                            ile_tru++;
                        }
                    }
         


                    if (ile_tru == 3)
                    {      
                        btninstal.IsEnabled = true;
                    }
                    else
                    {
                        btninstal.IsEnabled = false;
                    }
 

            return endInstall;


        }

        private void Window_Closing_1(object sender, CancelEventArgs e) // closing window by X button
        {


            dataBaseManager.LogToDB();
        }

        List<string> Get_Data_Log_mode() // Oticon / Bernafon / Sonic
        {
            string Ver_Bernafon = "C:/Program Files (x86)/Bernafon/Oasis/Oasis2/Configure.log4net";
            string Ver_Sonic = "C:/Program Files (x86)/Sonic/ExpressFit/EXPRESSfit2/Configure.log4net";
            string Ver_Oticon = "C:/Program Files (x86)/Oticon/Genie/Genie2/Configure.log4net";
            int counter = 0;
            string[] sources = { Ver_Oticon , Ver_Bernafon, Ver_Sonic };
            List<string> dd = new List<string>();
            List<string> modes_log = new List<string>();
            foreach (var item in sources)
            {
                try
                {
                    string[] oldFile;
                    if (File.Exists(item))
                    {
                        oldFile = File.ReadAllLines(item);
                        using (StreamReader sr = new StreamReader(item))
                        {
                            foreach (var line in oldFile)
                            {
                                if (counter == 23) //tryb logow
                                {
                                    string tmp = line;
                                    int i = 20;
                                    while (line[i] != '"')
                                    {
                                        dd.Add(line[i].ToString());
                                        i++;
                                    }
                                    string dogCsv = string.Join("", dd.ToArray());
                                    modes_log.Add(dogCsv);
                                }
                                counter++;
                            }
                        }
                    }
                    else
                    {
                        modes_log.Add("");
                    }
                }
                catch (Exception ee)
                {
                    dataBaseManager.LogException(ee.ToString(), "Get_Data_Log_mode");
                }

               
            }

            return modes_log;

        }
   
        void bindlogmode()
        {
            mode = new SortedDictionary<string, string>
            {
                { "All", "ALL"},
                { "Debug", "DEBUG"},
                { "Error", "ERROR"}
            };

            cmbLogMode.ItemsSource = mode;
            cmbLogMode.DisplayMemberPath = "Key";
            cmbLogMode.SelectedValuePath = "Value";

            Settings = new SortedDictionary<string, string>
            {
                { "Default", "Default"},
                 { "New file with Start FS", "DeleteWithWtart"},
                { "One file 100 MB", "OneF100"}
               
            };
            cmbLogSettings.ItemsSource = Settings;
            cmbLogSettings.DisplayMemberPath = "Key";
            cmbLogSettings.SelectedValuePath = "Value";


        }

        void getNamesInstallationFolders(string DirectoryName)
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(DirectoryName);
                nameFolders = di.EnumerateDirectories().ToArray();
            }
            catch (Exception)
            {
                //MessageBox.Show("directody doesnt exist");
            }

        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        void cbindBuild(string path)
        {
            DirectoryInfo najnowszeDirectory;
            getNamesInstallationFolders(path);

           builde = new SortedDictionary<string, int>();
           SortedDictionary<string,int> builde_with_data = new SortedDictionary<string, int>();



            if (!(cmbBrandstoinstall.SelectedIndex == 11 || cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8))
            {

                if ((cmbBrandstoinstall.SelectedIndex == 0 || cmbBrandstoinstall.SelectedIndex == 1 || cmbBrandstoinstall.SelectedIndex == 2)) //Nightly
                {
                    najnowszeDirectory = new DirectoryInfo(path).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();
                    System.IO.DirectoryInfo di = new DirectoryInfo(path);
                    DateTime dt;
                    int licznik = 0;
                    for (int i = 0; i < nameFolders.Length; i++)
                    {
                        if (!IsDirectoryEmpty(path+ nameFolders[i].ToString()))
                        {                        
                        dt = Directory.GetCreationTime(path + nameFolders[i].ToString());
                        builde.Add(nameFolders[i].ToString(), licznik);
                        builde_with_data.Add(nameFolders[i].ToString() + " Create date: "+ dt.ToString(), licznik);
                        licznik++;
                        }
                    }
                    cmbBuild.ItemsSource = builde_with_data;
                            cmbBuild.SelectedIndex = builde[najnowszeDirectory.ToString()];

                }
                else
                {
                    najnowszeDirectory = new DirectoryInfo(path).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();
                    DateTime dt;
                    int licznik = 0;
                    if (nameFolders.Length < 500)
                    {
                        for (int i = 0/*(nameFolders.Length - nameFolders.Length / 80)*/; i < nameFolders.Length; i++)
                        {
                            if (!IsDirectoryEmpty(path + nameFolders[i].ToString()))
                            {
                                dt = Directory.GetCreationTime(path + nameFolders[i].ToString());
                                builde.Add(nameFolders[i].ToString(), licznik);
                                builde_with_data.Add($"{nameFolders[i].ToString()} Create date: {dt.ToString()}", licznik);
                                licznik++;
                            }
                        }
                        cmbBuild.ItemsSource = builde_with_data;
                        cmbBuild.SelectedIndex = builde[najnowszeDirectory.ToString()];
                    }
                    else
                    {

                        var directory = new DirectoryInfo(path);
                        DirectoryInfo[] myFile = (from f in directory.GetDirectories()
                                      orderby f.LastWriteTime descending
                                      select f).Take(35).ToArray();
                        List < string > theLastPreDirectorys = new List<string>();

                        for (int i = 0; i < myFile.Length; i++)
                        {
                            dt = Directory.GetCreationTime(path + myFile[i].ToString());
                            theLastPreDirectorys.Add(myFile[i].ToString());
                            builde.Add(myFile[i].ToString(), i);
                            builde_with_data.Add($"{myFile[i].ToString()} Create date: {dt.ToString()}", i);
                        }


                        //for (int i = (nameFolders.Length - nameFolders.Length / 80); i < nameFolders.Length; i++)
                        //{
                        //    if (!IsDirectoryEmpty(path + nameFolders[i].ToString()))
                        //    {
                        //        dt = Directory.GetCreationTime(path + nameFolders[i].ToString());
                        //        builde.Add(nameFolders[i].ToString(), licznik);
                        //        builde_with_data.Add($"{nameFolders[i].ToString()} Create date: {dt.ToString()}", licznik);
                        //        licznik++;
                        //    }
                        //}
                        cmbBuild.ItemsSource = builde_with_data;
                        cmbBuild.SelectedIndex = builde[najnowszeDirectory.ToString()];
                    }
                    //else
                    //    for (int i = 0; i < nameFolders.Length; i++)
                    //    {
                    //        builde.Add(nameFolders[i].ToString(), i);
                    //    }
                    //cmbBuild.ItemsSource = builde;
                }
            }
            else
            {
                string pathToPRE_releases = path;
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Directory doesnt exist");
                   
                    cmbBuild.ItemsSource = null;
                    return;
                }
                getNamesInstallationFolders(pathToPRE_releases);
                DirectoryInfo[] nameFolders2 = nameFolders;
                FoldersInIP.Clear();
               

                    foreach (var item in nameFolders2)
                    {
                        getNamesInstallationFolders($"{pathToPRE_releases}{item}");
                        for (int i = 0; i < nameFolders.Length; i++)
                        {
                            FoldersInIP.Add($"{nameFolders[i].ToString()} {item}");
                        }
                    }
                    for (int i = 0; i < FoldersInIP.Count; i++)
                    {
                        try
                        {
                          builde.Add(FoldersInIP[i].ToString(), i);
                        }
                        catch (Exception)
                        {
                         
                        }                    
                    }
                cmbBuild.ItemsSource = builde;
            }
            cmbBuild.DisplayMemberPath = "Key";
            cmbBuild.SelectedValuePath = "Value";
  
            string actual = dataBaseManager.GetActualVersion(cmbBrandstoinstall.SelectedValue.ToString());
            if (actual != "")
            {
                try
                {
                    cmbBuild.SelectedIndex = builde[actual];
                }
                catch (Exception)
                {

                }
                
            }

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        void cbindBrandsToInstall()
        {
            brands = new SortedDictionary<string, string>
            {
                { "Genie", "Oticon"},
                { "Oasis", "Bernafon"},
                { "EXPRESSfit", "Sonic"},
                { "Genie PRE", "Genie"},
                { "Oasis PRE", "Oasis"},
                { "EXPRESSfit PRE", "EXPRESSFit"},
                { "Genie Composition", "GenieComposition"},
                { "Oasis Composition", "OasisComposition"},
                { "_Nightly_Genie", "Genie_N"}, //\\demant.com\data\KBN\RnD\SWS\Build\Arizona\Phoenix\Nightly-18.1
                { "_Nightly_Oasis", "Oasis_N"},
                { "_Nightly_EXPRESSfit", "EXPRESSfit_N"},
                { "EXPRESSfit Composition", "EXPRESSfitComposition"}
            };

            cmbBrandstoinstall.ItemsSource = brands;
            cmbBrandstoinstall.DisplayMemberPath = "Key";
            cmbBrandstoinstall.SelectedValuePath = "Value";
        }

        bool checkBoxes()
        {
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    return true;
                }
            }
            return false;
        }

        void changeMarket(string source)
        {
            string[] oldFile;
            int counter = 0;

            try
            {
                oldFile = File.ReadAllLines(source);
                using (StreamWriter sw = new StreamWriter(source))
                {
                    foreach (var line in oldFile)
                    {
                        if (counter == 3)
                        {
                            sw.WriteLine($"  <MarketName>{cmbMarket.SelectedValue}</MarketName>");
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                        counter++;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                dataBaseManager.LogException(ex.ToString(), "changeMarket File Not Found");
            }
            catch (DirectoryNotFoundException ee)
            {
                dataBaseManager.LogException(ee.ToString(), "changeMarket Directory Not Found");
            }
            catch (NullReferenceException e)
            {
                dataBaseManager.LogException(e.ToString(), "changeMarket Null Reference");
            }
        }
        void UpdateLogModeOnUI()
        {
            List<string> mode = new List<string>() { "ALL", "DEBUG", "ERROR" };
            int numberOfChecks = 0;
            string[] selectedModes = new string[3];
            bool AreEqual = true;

            if (Oticon.IsChecked == true)
            {
                selectedModes[numberOfChecks] = GetLogMode(@"C:\Program Files (x86)\Oticon\Genie\Genie2\Configure.log4net");
                numberOfChecks++;
            }
            if (Bernafon.IsChecked == true)
            {
                selectedModes[numberOfChecks] = GetLogMode(@"C:\Program Files (x86)\Bernafon\Oasis\Oasis2\Configure.log4net");
                numberOfChecks++;
            }
            if (Sonic.IsChecked == true)
            {
                selectedModes[numberOfChecks] = GetLogMode(@"C:\Program Files (x86)\Sonic\ExpressFit\ExpressFit2\Configure.log4net");
                numberOfChecks++;
            }

            for (int i=0; i<numberOfChecks-1; ++i)
            {
                if (selectedModes[i] != selectedModes[i+1])
                {
                    AreEqual = false;
                }
            }

            if (AreEqual)
            {
                cmbLogMode.SelectedIndex = mode.IndexOf(selectedModes[0]);
            }
            else
            {
                cmbLogMode.SelectedIndex = -1;
            }

        }

        string GetLogMode(string source)
        {
            string line = "";
            if (File.Exists(source))
            {
                try
                {



                    using (StreamReader sr = new StreamReader(source))
                    {
                        for (int i = 0; i < 23; ++i)
                        {
                            sr.ReadLine();
                        }
                        line = sr.ReadLine();
                        string[] subLine = line.Split('"');
                        return subLine[1];
                    }
                }
                catch (Exception ee)
                {
                    dataBaseManager.LogException(ee.ToString(), "GetLogMode");
                    return "";
                }
            }
            else
            {
                return "";
            }

        }

        void SetLogSettings(bool mode) // powinno dzialac // ustawienia od TOOJ co do logow FS cos z tym ze same sie usuwaja po ponownym wlaczeniu 
        {
            string[] oldFile;
            int counter = 0;
            string source;
            int count3 = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsEnabled)
                {
                    try
                    {
                        source = $"C:/Program Files (x86)/{checkbox.Name}/{marki[count3]}/{marki[count3]}{"2"}/Configure.log4net";
                        count3++;
                        oldFile = File.ReadAllLines(source);
                        using (StreamWriter sw = new StreamWriter(source))
                        {
                            foreach (var line in oldFile)
                            {
                                if (mode)
                                {
                                    if (counter == 36)
                                    {
                                        sw.WriteLine($"      <appendToFile value=\"{false.ToString().ToLower()}\"/>");
                                    }
                                    if (counter == 38)
                                    {
                                        sw.WriteLine($"      <staticLogFileName value=\"{false.ToString().ToLower()}\"/>");
                                    }
                                }
                                else
                                {
                                    if (counter == 36)
                                    {
                                        sw.WriteLine($"      <appendToFile value=\"{true.ToString().ToLower()}\"/>");
                                    }
                                    if (counter == 38)
                                    {
                                        sw.WriteLine($"      <staticLogFileName value=\"{true.ToString().ToLower()}\"/>");
                                    }
                                }



                                if (counter != 36 && counter != 38)
                                {
                                    sw.WriteLine(line);
                                }
                                counter++;
                            }
                        }

                    }
                    catch (Exception ee)
                    {
                        dataBaseManager.LogException(ee.ToString(), "SetLogSettings ");
                    }
                }

            }


 

        }
        
        bool changeLog_Mode(string source)
        {
            string[] oldFile;
            int counter = 0;
            bool message=false;

            
            try
            {
                oldFile = File.ReadAllLines(source);
                using (StreamWriter sw = new StreamWriter(source))
                {
                    foreach (var line in oldFile)
                    {
                        if (counter == 23) //tryb logow
                        {
                            sw.WriteLine($"      <level value=\"{cmbLogMode.SelectedValue}\"/>");     
                        }
                        if (counter == 33 && cmbLogSettings.SelectedIndex == 2)
                        {
                            sw.WriteLine($"      <rollingStyle value=\"Once\"/>");
                        }
                        if (counter == 33 && cmbLogSettings.SelectedIndex != 2)
                        {
                            sw.WriteLine(line);
                        }

                        if (counter == 34) //rozmiar plikow
                        {
                            if (cmbLogSettings.SelectedIndex == 0 || cmbLogSettings.SelectedIndex == 1) //0 - wybrany default
                            {
                                if (cmbLogMode.SelectedValue.ToString() == "ERROR")
                                {
                                    sw.WriteLine($"      <maximumFileSize value=\"{5}MB\"/>");
                                }
                                if (cmbLogMode.SelectedValue.ToString() == "DEBUG")
                                {
                                    sw.WriteLine($"      <maximumFileSize value=\"{10}MB\"/>");
                                }
                                if (cmbLogMode.SelectedValue.ToString() == "ALL")
                                {
                                    sw.WriteLine($"      <maximumFileSize value=\"{20}MB\"/>");
                                }
                            } else
                            { //(cmbLogSettings.SelectedIndex == 2)

                                sw.WriteLine($"      <maximumFileSize value=\"{100}MB\"/>");

                            }
                        }
                        if (counter == 36)
                        {
                            if (cmbLogSettings.SelectedIndex == 1 && cmbLogMode.SelectedValue.ToString() == "ALL")
                            {
                                sw.WriteLine($"      <appendToFile value=\"{false.ToString().ToLower()}\"/>");
                            }
                            else
                            sw.WriteLine($"      <appendToFile value=\"{true.ToString().ToLower()}\"/>");
                        }

                        if (counter == 37 ) //ilosc plikow
                        {
                            if (cmbLogSettings.SelectedIndex == 0 || cmbLogSettings.SelectedIndex == 1) //0 - wybrany default
                            {
                                if (cmbLogMode.SelectedValue.ToString() == "ERROR")
                                {
                                    sw.WriteLine($"      <maxSizeRollBackups value=\"{5}\"/>");
                                }
                                if (cmbLogMode.SelectedValue.ToString() == "DEBUG")
                                {
                                    sw.WriteLine($"      <maxSizeRollBackups value=\"{10}\"/>");
                                }
                                if (cmbLogMode.SelectedValue.ToString() == "ALL")
                                {
                                    sw.WriteLine($"      <maxSizeRollBackups value=\"{20}\"/>");
                                }
                            }
                            else
                            {
                                sw.WriteLine($"      <maxSizeRollBackups value=\"{1}\"/>");
                            }
                        }
                        
                        if (counter != 23 && counter != 37 && counter != 34 && counter != 36 && counter != 33)
                        {
                            sw.WriteLine(line);
                        }
                        counter++;
                    }
                }
                
                message = true;
                return message;
            }
            catch (FileNotFoundException ee)
            {
                dataBaseManager.LogException(ee.ToString(), "changeLog_Mode File Not Found");
                MessageBox.Show("File Not Found");
                return message;
            }
            catch (DirectoryNotFoundException ex)
            {
                dataBaseManager.LogException(ex.ToString(), "changeLog_Mode Directory Not Found");
                return message;
            }
            catch (NullReferenceException e)
            {
                dataBaseManager.LogException(e.ToString(), "changeLog_Mode Null Reference");
                return message;
            }
        }

        bool verifyInstanceOfExec(string name)
        {
            foreach (CheckBox checkbox in checkBoxList)
            {
                if (checkbox.Name == name)
                {     
                    if (File.Exists($"C:/Program Files (x86)/{name}/{BrandtoSoft[checkbox.Name]}/{BrandtoSoft[checkbox.Name]}2/{BrandtoSoft[checkbox.Name]}.exe"))
                    {
                        return true;
                    }
                    else return false;
                }
            }
            return false;
        }
        
        //[Obsolete]
        void verifyInstalledBrands()
        {
           // if (!File.Exists(@"C:/Program Files (x86)/Oticon/Genie/Genie2/Genie.exe"))
           if (!Directory.Exists(@"C:\ProgramData\Oticon"))
            {
                Oticon.IsEnabled = false;
                lblG.Foreground = new SolidColorBrush(Colors.Red);
                lblG.Content = "FS not installed";
                Oticon.IsChecked = false;
                oticonRectangle.Opacity = 0.3;
            }
            else
            {
                Oticon.IsEnabled = true;
                oticonRectangle.Opacity = 1.0;
            }
            // if (!File.Exists(@"C:/Program Files (x86)/Bernafon/Oasis/Oasis2/Oasis.exe"))
            if (!Directory.Exists(@"C:\ProgramData\Bernafon"))
            {
                Bernafon.IsEnabled = false;
                lblO.Foreground = new SolidColorBrush(Colors.Red);
                lblO.Content = "FS not installed";
                Bernafon.IsChecked = false;
                bernafonRectangle.Opacity = 0.3;
            }
            else
            {
                Bernafon.IsEnabled = true;
                bernafonRectangle.Opacity = 1.0;
            }
            //if (!File.Exists(@"C:/Program Files (x86)/Sonic/ExpressFit/ExpressFit2/ExpressFit.exe"))
            if (!Directory.Exists(@"C:\ProgramData\Sonic"))
            {
                Sonic.IsEnabled = false;
                lblE.Foreground = new SolidColorBrush(Colors.Red);
                lblE.Content = "FS not installed";
                Sonic.IsChecked = false;
                sonicnRectangle.Opacity = 0.3;
            }
            else
            {
                Sonic.IsEnabled = true;
                sonicnRectangle.Opacity = 1.0;
            }
        }

        bool checkRunningProcess(string name)
        {
            Process[] proc = Process.GetProcessesByName(name);
            Process[] localAll = Process.GetProcesses();

            foreach (Process item in localAll)
            {
                string tmop = item.ProcessName;
                if (tmop == name)
                {
                    return false;
                }
            }
            return true;
        }

        void startAnimation()
        {
            blinkAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            if (Oticon.IsChecked == true)   oticonRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
            if (Bernafon.IsChecked == true) bernafonRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
            if (Sonic.IsChecked == true)    sonicnRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
        }

        void stopAnimation()
        {
            blinkAnimation = new DoubleAnimation();
            if (Oticon.IsChecked == false)   oticonRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
            if (Bernafon.IsChecked == false) bernafonRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
            if (Sonic.IsChecked == false)    sonicnRectangle.BeginAnimation(Rectangle.OpacityProperty, blinkAnimation);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            bool message = false;
            int count3 = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    if (checkRunningProcess(marki[count3]))
                    {
                        changeMarket($"C:/ProgramData/{checkbox.Name}/Common/ManufacturerInfo.XML");
                    }
                    else
                    {
                        message = true;
                    }
                }
                count3++;
            }
            if (message)
            {
                MessageBox.Show("Close fitting software", "Brand", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            fileOperator.UpdateLabels();
            verifyInstalledBrands();

            clickCounter.AddClick((int)Buttons.UpdateMarket);
        }

        void deleteLogs()
        {
            foreach (CheckBox checkbox in checkBoxList)
            {
                int brandCounter = 0;
                if ((bool)checkbox.IsChecked) //analiza => jeden zaznaczony dwa nie 
                {
                    if (checkRunningProcess(marki[brandCounter]))
                    {
                        Cleaner.DeleteLogs(checkbox.Name.ToString());
                        MessageBox.Show($" Deleted logs for {checkbox.Name}");
                    }
                    else
                    {
                        MessageBox.Show("Close fitting software", "Brand", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                brandCounter++;
            }
        }

        private void deletesmieci()
        {
            try
            {
                bool fined = false;

                //procesy_uninstall
                if ((bool)Oticon.IsChecked && checkRunningProcess("Genie") && !verifyInstanceOfExec("Oticon") && checkRunningProcess(procesy_uninstall[0]))
                {
                    if (Directory.Exists("C:/ProgramData/Oticon"))
                    {
                        Directory.Delete("C:/ProgramData/Oticon", true);
                    }

                    if (Directory.Exists("C:/Program Files (x86)/Oticon"))
                    {
                        Directory.Delete("C:/Program Files (x86)/Oticon", true);
                    }

                    if (Directory.Exists("C:/Program Files/DGS - PAZE & MIBW/Oticon"))
                    {
                        Directory.Delete("C:/Program Files/DGS - PAZE & MIBW/Oticon", true);
                    }

                    fined = true;
                }
                if ((bool)Bernafon.IsChecked && checkRunningProcess("Oasis") && !verifyInstanceOfExec("Bernafon") && checkRunningProcess(procesy_uninstall[1]))
                {
                    if (Directory.Exists("C:/ProgramData/Bernafon"))
                    {
                        Directory.Delete("C:/ProgramData/Bernafon", true);
                    }

                    if (Directory.Exists("C:/Program Files (x86)/Bernafon"))
                    {
                        Directory.Delete("C:/Program Files (x86)/Bernafon", true);
                    }

                    if (Directory.Exists("C:/Program Files/DGS - PAZE & MIBW/Bernafon"))
                    {
                        Directory.Delete("C:/Program Files/DGS - PAZE & MIBW/Bernafon", true);
                    }

                    fined = true;
                }
                if ((bool)Sonic.IsChecked && checkRunningProcess("EXPRESSfit") && !verifyInstanceOfExec("Sonic") && checkRunningProcess(procesy_uninstall[2]))
                {

                    if (Directory.Exists("C:/ProgramData/Sonic"))
                    {
                        Directory.Delete("C:/ProgramData/Sonic", true);
                    }

                    if (Directory.Exists("C:/Program Files (x86)/Sonic"))
                    {
                        Directory.Delete("C:/Program Files (x86)/Sonic", true);
                    }

                    if (Directory.Exists("C:/Program Files/DGS - PAZE & MIBW/Sonic"))
                    {
                        Directory.Delete("C:/Program Files/DGS - PAZE & MIBW/Sonic", true);
                    }
                    fined = true;
                }
                if (!fined)
                {
                    MessageBox.Show("Unionstall FS first.", "Brand", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Trash deleted successfully!", "deleteTrash", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                fileOperator.UpdateLabels();
                verifyInstalledBrands();
            }
            catch (Exception ee)
            {
                dataBaseManager.LogException(ee.ToString(), "deletesmieci");
            }
            if (dispatcherTimer.IsEnabled == false)
            {
                dispatcherTimer.Start();
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            deletesmieci();
        }

        private void btnFS_Click(object sender, RoutedEventArgs e)
        {
            int counter_proc = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked && File.Exists($"C:/Program Files (x86)/{checkbox.Name}/{BrandtoSoft[checkbox.Name]}/{BrandtoSoft[checkbox.Name]}2/{BrandtoSoft[checkbox.Name]}.exe") && checkRunningProcess(marki[counter_proc]))
                {
                 Process.Start($"C:/Program Files (x86)/{checkbox.Name}/{BrandtoSoft[checkbox.Name]}/{BrandtoSoft[checkbox.Name]}2/{BrandtoSoft[checkbox.Name]}.exe");
                }
                counter_proc++;
            }
            verifyInstalledBrands();

            clickCounter.AddClick((int)Buttons.StartFittingSoftware);
        }

        private void btnHattori_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked && File.Exists($"C:/Program Files (x86)/{checkbox.Name}/{BrandtoSoft[checkbox.Name]}/FirmwareUpdater/FirmwareUpdater.exe"))
                 {
                    Process.Start(($"C:/Program Files (x86)/{checkbox.Name}/{BrandtoSoft[checkbox.Name]}/FirmwareUpdater/FirmwareUpdater.exe"));
                 }
           
                if ((bool)Oticon.IsChecked)
                {
                     if (File.Exists("C:/Program Files (x86)/Oticon/FirmwareUpdater/FirmwareUpdater/FirmwareUpdater.exe"))
                      {
                         Process.Start("C:/Program Files (x86)/Oticon/FirmwareUpdater/FirmwareUpdater/FirmwareUpdater.exe");
                      }
                 }
            }

            fileOperator.UpdateLabels();
            verifyInstalledBrands();

            clickCounter.AddClick((int)Buttons.StartHAttori);
        }

        private void btnuninstal_Click(object sender, RoutedEventArgs e)
        {
            byte counter_checkbox = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {               
                if ((bool)checkbox.IsChecked)
                {
                    counter_checkbox++;
                }
            }
            if (counter_checkbox > 1)
            {
                MessageBox.Show("can not uninstall more than one");
                return;
            }
                int counter_proc = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                
                if ((bool)checkbox.IsChecked )
                {
                    string tmp = $"\"C:\\Program Files\\DGS - PAZE & MIBW\\{checkbox.Name}\\\"";

                    if (!checkRunningProcess(marki[counter_proc]))
                    {
                        MessageBox.Show("Please, close FS");
                        return;
                    }

                    string tttt = $"\"C:\\Program Files\\DGS - PAZE & MIBW\\{checkbox.Name}\\Setup.exe\"";
                    if (File.Exists($"C:/Program Files/DGS - PAZE & MIBW/{checkbox.Name}/Setup.exe"))
                    {
                        if (RBnormal.IsChecked.Value)
                        {
                            Process.Start(tttt, "/uninstall ");                          
                        }
                        else
                        {
                            Process.Start(tttt, "/uninstall /quiet");
                            MessageBox.Show("FS will be uninstalled ASAP");
                        }
                        dispatcherTimer.Stop();
                        checkbox.IsEnabled = false;
                        Sonic.IsEnabled = false;
                    }
                    else //jeżeli nie było instalowane z apki
                    {
                        // string actual = dataBaseManager.GetActualVersion(checkbox.Name);
                        //sciezka szczecin full

                        string ver_about="" ;
                        // dodac numerek z abouta 


                        switch (checkbox.Name)
                        {
                            case "Oticon":
                                ver_about =Ver_Oticon;
                                break;
                            case "Bernafon":
                                ver_about =Ver_Bernafon;
                                break;
                            case "Sonic":
                                ver_about =Ver_Sonic;
                                break;
                        }
                        string actual = dataBaseManager.GetIPVersion($"{BrandtoSoft[checkbox.Name]}", $"'{ver_about}'");

                        string path = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\"
                                    + BrandtoSoft[checkbox.Name] + @"\"
                                    + actual + @"\" + "Full" + @"\"
                                    + checkbox.Name + @"\Setup.exe";

                        Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + checkbox.Name);                       
                        string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + checkbox.Name + @"\";
                        try
                        {
                            string[] tm2p = Directory.GetFiles(destinationDirectory);
                            if (tm2p.Length==0)
                            File.Copy(path, destinationDirectory + System.IO.Path.GetFileName(path));
                        }
                        catch (Exception ee)
                        {
                            dataBaseManager.LogException(ee.ToString(), "btnuninstal_Click_file copy");
                        }

                        string[] instalkaName = Directory.GetFiles(destinationDirectory);

                        if (File.Exists(instalkaName[0]))
                        {       

                            if (RBnormal.IsChecked.Value)
                            {
                                Process.Start(instalkaName[0], "/uninstall ");
                            }
                            else
                            {
                                Process.Start(instalkaName[0], " /uninstall /quiet");
                                MessageBox.Show("FS will be uninstalled ASP");
                            }
                            checkbox.IsEnabled = false;
                            Sonic.IsEnabled = false;
                        }
                    }
                    try
                    {
                        if (!RBnormal.IsChecked.Value)
                        {
                            //worker2 = new BackgroundWorker();
                            //worker2.DoWork += checkUninstallationStatus;
                            //worker2.RunWorkerAsync();
                            uninstallTimer.Start();
                            dispatcherTimer.Stop();
                            btnuninstal.IsEnabled = false;
                        }
                    }
                    catch (Exception ee)
                    {
                        dataBaseManager.LogException(ee.ToString(), "btnuninstal_Click");

                    }

                }
                counter_proc++;
            }
            fileOperator.UpdateLabels();
            verifyInstalledBrands();            
            clickCounter.AddClick((int)Buttons.UninstallFittingSoftware);
        }

        private void Brand_Unchecked(object sender, RoutedEventArgs e)
        {
            fileOperator.HandleSelectedMarket();
            if (!checkBoxes())
            {
                btnHattori.IsEnabled = false;
                btnFS.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnUpdate.IsEnabled = false;
                btnLogMode.IsEnabled = false;
                btnDeletelogs.IsEnabled = false;
                btnuninstal.IsEnabled = false;
            }
            UpdateLogModeOnUI();
            stopAnimation();

            byte counter_checkbox = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    counter_checkbox++;
                }
            }
            if (counter_checkbox == 1)
            {
                btnuninstal.IsEnabled = true;
                return;
            }
        }

        private void Brand_Checked(object sender, RoutedEventArgs e)
        {
            fileOperator.HandleSelectedMarket();
            btnHattori.IsEnabled = true;
            btnFS.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnUpdate.IsEnabled = true;
            btnLogMode.IsEnabled = true;
            btnDeletelogs.IsEnabled = true;
            btnuninstal.IsEnabled = true;
            startAnimation();
            UpdateLogModeOnUI();
            if (cmbLogMode.SelectedIndex < 0)
            {
                btnLogMode.IsEnabled = false;
            }

            byte counter_checkbox = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    counter_checkbox++;
                }
            }
            if (counter_checkbox > 1)
            {
                btnuninstal.IsEnabled = false;
                return;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxes())
            {
                foreach (CheckBox checkbox in checkBoxList)
                {
                    int tmp = counter2 % 2;
                    if (tmp == 0) {
                        if (checkbox.IsEnabled == true)
                        {
                            checkbox.IsChecked = true;
                        }
                        
                    }
                    else
                    {
                        if (checkbox.IsEnabled == true)
                        {
                            checkbox.IsChecked = false;
                        }
                    }
                }
               
            }
            else
            {
                byte x = 0;
                foreach (CheckBox checkbox in checkBoxList)
                {
                    if (checkbox.IsEnabled == true)
                    {
                        checkbox.IsChecked = true;
                        x++;
                    }
                }
                if (x>1)
                {
                    btnuninstal.IsEnabled = false;
                }
                else
                {
                    btnuninstal.IsEnabled = true;
                }
                
            }
            counter2++;

            clickCounter.AddClick((int)Buttons.All);           
        }

        private void cmbMarket_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void btnChange_mode_log(object sender, RoutedEventArgs e)
        {
            bool message = false;
            bool changed = false;
            string[] marki = { "Genie", "Oasis", "EXPRESSfit" };
            int count3 = 0;
            foreach (CheckBox checkbox in checkBoxList)
            {
                if ((bool)checkbox.IsChecked)
                {
                    if (checkRunningProcess(marki[count3]))
                    {
                        changed = changeLog_Mode($"C:/Program Files (x86)/{checkbox.Name}/{marki[count3]}/{marki[count3]}{"2"}/Configure.log4net"); 
                    }
                    else
                    {
                        message = true;
                    }
                }
                count3++;
            }
            if (message)
            {
                MessageBox.Show("Close fitting software", "Brand", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (changed && !message)
            {
                MessageBox.Show("Updated");
            }
            if(!changed && !message)
            {
                MessageBox.Show("Error, check file");
            }
            fileOperator.UpdateLabels();
            verifyInstalledBrands();

            clickCounter.AddClick((int)Buttons.UpdateMode);
        }

        private void btnDelete_logs(object sender, RoutedEventArgs e)
        {
            deleteLogs();
            fileOperator.UpdateLabels();
            verifyInstalledBrands();
            clickCounter.AddClick((int)Buttons.DeleteLogs);
        }

        private void cmbLogMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLogMode.SelectedIndex < 0)
            {
                btnLogMode.IsEnabled = false;
                cmbLogSettings.SelectedIndex = 0;
            }
            else
            {
                btnLogMode.IsEnabled = true;
            }
        }

        private void btninstal_Click(object sender, RoutedEventArgs e)
        {
            string firstHalf = cmbBuild.Text.ToString().Split(new char[] { ' ' }, 2)[0]; ;
            string[] marki = { "EXPRESSfit", "Genie", "Oasis", "ExpressFit","", "", "Genie", "", "", "Oasis", "", "" };
            string[] marki_dun = { "EXPRESSfit", "Genie", "Oasis", "ExpressFit", "", "", "Genie", "", "", "Oasis", "", "" };
            bool installation_done = false;
            if (cmbBrandstoinstall.SelectedIndex > -1 && cmbBuild.SelectedIndex > -1 && checkInstallationStatus() && (cmbBrandstoinstall.SelectedIndex == 3 || cmbBrandstoinstall.SelectedIndex == 6 || cmbBrandstoinstall.SelectedIndex == 9))

            {
                if (!verifyInstanceOfExec(cmbBrandstoinstall.SelectedValue.ToString()))

                {
                    try
                    {
                        //sciezka szczecin full
                        string path;
                         
                        if (OEMname != "")
                        {
                            path = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\"
                                   + marki[cmbBrandstoinstall.SelectedIndex] + @"\"
                                   + firstHalf + @"\" + "Full" + @"\"
                                   + OEMname + @"\Setup.exe";
                        } else if (txtCompositionPart2.Text!="18.1") {
                            path = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\20"+txtCompositionPart2.Text.ToString()+@"\"
                                   + firstHalf + @"\" + "Full" + @"\"
                                  + cmbBrandstoinstall.SelectedValue.ToString() + @"\Setup.exe";
                        }
                        else
                         path = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\"
                                    + marki[cmbBrandstoinstall.SelectedIndex] + @"\"
                                    + firstHalf + @"\" + "Full" + @"\"
                                    + cmbBrandstoinstall.SelectedValue.ToString() + @"\Setup.exe";
                        if (File.Exists(path))
                        {
                            installation_done = FSInstaller.InstallBrand(path, RBnormal.IsChecked.Value);
                            Directory_toIntall = path;
                        }
                        

                        if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString()))
                        {
                            Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString());
                            string tmp = path;// + "Setup.exe";
                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString() + @"\";
                            File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                            Directory_toIntall = tmp;
                        }
                        else
                        {
                            string tmp = path;// + "Setup.exe";
                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString() + @"\";
                            if (!File.Exists(tmp))
                            {
                                File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                Directory_toIntall = tmp;
                            }
                        }

                    }
                    catch (Exception ee)
                    {
                        dataBaseManager.LogException(ee.ToString(), "btninstal_Click for SSC Full path");
                        // sciezka szczecin mini
                        // string path = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\" + marki[cmbBrandstoinstall.SelectedIndex] + @"\" + cmbBuild.SelectedValue.ToString() + @"\" + "Mini" + @"\";
                        string path = $@"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\
                                        {marki[cmbBrandstoinstall.SelectedIndex]}
                                        \{firstHalf}
                                        \\Mini\";
                        try
                        {
                            string message = "Are you sure, you want install Mini version ?";
                            string caption = "Build Version Choice";
                            MessageBoxButton buttons = MessageBoxButton.YesNo;

                            if (Directory.Exists(path))
                            {
                                MessageBoxResult result_choice = MessageBox.Show(this, message, caption, buttons);

                                if (result_choice.ToString() == "Yes")
                                {
                                    installation_done = true;
                                    Process.Start(path + "Media.exe");
                                    Directory_toIntall = path;
                                }
                                else
                                {
                                    return; // wyjscie z metody ?? No.
                                }
                            }
                        }
                        catch (Exception e1) {
                            //sciezka do dunskiego // sprawdzic czy cala ok.
                            dataBaseManager.LogException(e1.ToString(), "btninstal_Click for SSC Mini path");
                            try
                            {
                                path = @"\\demant.com\data\KBN\RnD\SWS\Build\Projects\" + marki_dun[cmbBrandstoinstall.SelectedIndex] + @"\" + firstHalf + @"\" + "Full" + @"\" + cmbBrandstoinstall.SelectedValue.ToString() + @"\" + "Setup.exe";

                                installation_done = FSInstaller.InstallBrand(path + "Setup.exe", RBnormal.IsChecked.Value);
                                if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString()))
                                {
                                    Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString());
                                    string tmp = path + "Setup.exe";
                                    string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString() + @"\";
                                    File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                    Directory_toIntall = tmp;
                                }
                                else
                                {
                                    string tmp = path + "Setup.exe";
                                    string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + cmbBrandstoinstall.SelectedValue.ToString() + @"\";
                                    File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                    Directory_toIntall = tmp;
                                }
                                
                            }
                            catch (Exception e2)
                            {
                                dataBaseManager.LogException(e2.ToString(), "btninstal_Click for DKK Full path");
                                path = @"\\demant.com\data\KBN\RnD\SWS\Build\Projects\" + marki_dun[cmbBrandstoinstall.SelectedIndex] + @"\" + firstHalf + @"\" + "Mini" + @"\" + "Media.exe";
                                try
                                {
                                    string message = "Are you sure, you want install Mini version ?";
                                    string caption = "Build Version Choice";
                                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                                    if (Directory.Exists(path))
                                    {
                                        MessageBoxResult result_choice = MessageBox.Show(this, message, caption, buttons);
                                        if (result_choice.ToString() == "Yes")
                                        {
                                            installation_done = true;
                                            Process.Start(path + "Media.exe");
                                            Directory_toIntall = path;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                                catch (Exception e3)
                                {
                                    dataBaseManager.LogException(e3.ToString(), "btninstal_Click for DKK Mini path");
                                }
                            }
                        }
                    }
                    if (!installation_done)
                    {
                        MessageBox.Show("No Access to File Or Doesnt Exist ");
                    }
                    btninstal.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Brand already installed");
                }
            }
            else if ((cmbBrandstoinstall.SelectedIndex == 4 || cmbBrandstoinstall.SelectedIndex == 7 || cmbBrandstoinstall.SelectedIndex == 10)) // jezeli composition to kopiowac na dysk 
            {
                string[] marki2 = { "EXPRESSfit", "EXPRESSfitComposition", "", "Genie", "GenieComposition", "", "Oasis", "OasisComposition", "" };

                string path = $"//demant.com/data/KBN/RnD/FS/DevResults/Phoenix/Dev_16.1/{marki2[cmbBrandstoinstall.SelectedIndex]}/{firstHalf}/";

                if (!Directory.Exists("C:/Program Files/DGS - PAZE & MIBW/Compositions"))
                {
                    Directory.CreateDirectory("C:/Program Files/DGS - PAZE & MIBW/Compositions");
                }

                string destDirName = "C:/Program Files/DGS - PAZE & MIBW/Compositions";

                DirectoryCopy(path, $"{destDirName}/{firstHalf}", true);
                Directory_toIntall = path;
            }

            else if ((cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8 || cmbBrandstoinstall.SelectedIndex == 11) && !verifyInstanceOfExec(BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
            {
                string[] marki2 = { "","","","EXPRESSfit", "EXPRESSfitComposition", "EXPRESSFit", "Genie", "GenieComposition", "Genie", "Oasis", "OasisComposition", "Oasis" };
                string szukany_build = firstHalf;
                string path = @"\\demant.com\data\KBN\RnD\FS_Programs\Fitting Applications\" + marki2[cmbBrandstoinstall.SelectedIndex] + "\\20" + txtCompositionPart2.Text.ToString() + "\\Pre-releases\\";


                string pathToPRE_releases = path;
                getNamesInstallationFolders(pathToPRE_releases);
                DirectoryInfo[] nameFolders2 = nameFolders;
                List<string> FoldersInIP = new List<string>();

                foreach (var item in nameFolders2)
                {
                    getNamesInstallationFolders($"{pathToPRE_releases}{item}");
                    for (int i = 0; i < nameFolders.Length; i++)
                    {
                        if (szukany_build == nameFolders[i].ToString())
                        {
                            string path_to_build = @pathToPRE_releases + item + "\\" + nameFolders[i] + "\\" + "items";
                            string[] MediumInstallers = { "GenieMediumOticon.exe", "OasisMediumBernafon.exe", "EXPRESSfitMediumSonic.exe" };
                            string[] MiniInstallers = { "GenieMiniOticon.exe", "OasisMini.exe", "EXPRESSfitMini.exe" };
                            string[] OEMInstallers = { $"GenieMini{OEMname}.exe", $"OasisMini{OEMname}.exe", $"EXPRESSfitMini{OEMname}.exe", $"GenieMedium{OEMname}.exe", $"OasisMedium{OEMname}.exe", $"EXPRESSfitMedium{OEMname}.exe" };
                            if (OEMname!="")
                            {
                                foreach (var item2 in OEMInstallers)
                                {
                                    if (File.Exists(@path_to_build + "\\" + item2))
                                    {


                                        // Process.Start(@path_to_build + "\\" + item2);
                                        installation_done = FSInstaller.InstallBrand(@path_to_build + "\\" + item2, RBnormal.IsChecked.Value);
                                        if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                                        {
                                            Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                            string tmp = @path_to_build + "\\" + item2;
                                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                            File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                            Directory_toIntall = tmp;
                                        }
                                        else
                                        {
                                            string tmp = @path_to_build + "\\" + item2;
                                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                            if (File.Exists(tmp))
                                            {
                                                if (!File.Exists(destinationDirectory + item2))
                                                    File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                                Directory_toIntall = tmp;
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                foreach (var item2 in MediumInstallers)
                                {
                                    if (File.Exists(@path_to_build + "\\" + item2))
                                    {


                                        // Process.Start(@path_to_build + "\\" + item2);
                                        installation_done = FSInstaller.InstallBrand(@path_to_build + "\\" + item2, RBnormal.IsChecked.Value);
                                        if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                                        {
                                            Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                            string tmp = @path_to_build + "\\" + item2;
                                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                            File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                            Directory_toIntall = tmp;
                                        }
                                        else
                                        {
                                            string tmp = @path_to_build + "\\" + item2;
                                            string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                            if (File.Exists(tmp))
                                            {
                                                if (!File.Exists(destinationDirectory + item2))
                                                    File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                                Directory_toIntall = tmp;
                                            }
                                        }

                                    }
                                }

                                if (!installation_done)
                                    foreach (var item2 in MiniInstallers)
                                    {
                                        if (File.Exists(@path_to_build + "\\" + item2))
                                        {

                                            Process.Start(@path_to_build + "\\" + item2);
                                            installation_done = true;
                                            if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                                            {
                                                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                                string tmp = @path_to_build + "\\" + item2;
                                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                                File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                                Directory_toIntall = tmp;
                                            }
                                            else
                                            {
                                                string tmp = @path_to_build + "\\" + item2;
                                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                                if (File.Exists(tmp))
                                                {
                                                    File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                                    Directory_toIntall = tmp;
                                                }
                                            }

                                        }
                                    }
                            }
                        }
                    }
                }
                if (!installation_done)
                {
                    MessageBox.Show("No Access to File Or Doesnt Exist ");
                }
            } else if ((cmbBrandstoinstall.SelectedIndex == 0 || cmbBrandstoinstall.SelectedIndex == 1 || cmbBrandstoinstall.SelectedIndex == 2) && !verifyInstanceOfExec(BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
            {
               // string firstHalf = cmbBuild.Text.ToString().Split(new char[] { ' ' }, 2)[0];
                string path_to_build = @"\\demant.com\data\KBN\RnD\SWS\Build\Arizona\Phoenix\Nightly-18.1\" + firstHalf + @"\MediumInstaller\" + marki[cmbBrandstoinstall.SelectedIndex]+@"\";
                if (!Directory.Exists(path_to_build))
                {
                    MessageBox.Show("Cannot install - no installation file (Medium/Mini)");
                }
                string[] MediumInstallers = { "GenieMediumOticon.exe", "OasisMediumBernafon.exe", "EXPRESSfitMediumSonic.exe" };
                string[] MiniInstallers = { "GenieMiniOticon.exe", "OasisMini.exe", "EXPRESSfitMini.exe" };
                string[] OEMInstallers = { $"GenieMini{OEMname}.exe", $"OasisMini{OEMname}.exe", $"EXPRESSfitMini{OEMname}.exe", $"GenieMedium{OEMname}.exe", $"OasisMedium{OEMname}.exe", $"EXPRESSfitMedium{OEMname}.exe" };

                if (OEMname != "")
                {
                    foreach (var item2 in OEMInstallers)
                    {
                        if (File.Exists(@path_to_build + "\\" + item2))
                        {
                            // Process.Start(@path_to_build + "\\" + item2);
                            try
                            {
                                installation_done = FSInstaller.InstallBrand(@path_to_build + "\\" + item2, RBnormal.IsChecked.Value);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Cannot install " + @path_to_build + "\\" + item2);
                            }
                            
                            Directory_toIntall = @path_to_build + "\\" + item2;
                            if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                            {
                                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                            }
                            else
                            {
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                if (File.Exists(tmp))
                                {
                                    if (!File.Exists(destinationDirectory + item2))
                                        File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                }
                            }

                        }
                    }
                    if (!installation_done)
                    {
                        MessageBox.Show($"{OEMname} doesnt exist");
                    }
                }
                else
                {


                    foreach (var item2 in MediumInstallers)
                    {
                        if (File.Exists(@path_to_build + "\\" + item2) && installation_done == false)
                        {


                            // Process.Start(@path_to_build + "\\" + item2);
                            installation_done = FSInstaller.InstallBrand(@path_to_build + "\\" + item2, RBnormal.IsChecked.Value);
                            Directory_toIntall = @path_to_build + "\\" + item2;
                            if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                            {
                                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                            }
                            else
                            {
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                if (File.Exists(tmp))
                                {
                                    if (!File.Exists(destinationDirectory + item2))
                                        File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                }
                            }

                        }
                    }
                    foreach (var item2 in MediumInstallers)
                    {
                        if (File.Exists(@path_to_build + "\\" + item2) && installation_done == false)
                        {


                            // Process.Start(@path_to_build + "\\" + item2);
                            installation_done = FSInstaller.InstallBrand(@path_to_build + "\\" + item2, RBnormal.IsChecked.Value);
                            Directory_toIntall = @path_to_build + "\\" + item2;
                            if (!Directory.Exists(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]))
                            {
                                Directory.CreateDirectory(@"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()]);
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                            }
                            else
                            {
                                string tmp = @path_to_build + "\\" + item2;
                                string destinationDirectory = @"C:\Program Files\DGS - PAZE & MIBW\" + BrandtoSoft[cmbBrandstoinstall.SelectedValue.ToString()] + @"\";
                                if (File.Exists(tmp))
                                {
                                    if (!File.Exists(destinationDirectory + item2))
                                        File.Copy(tmp, destinationDirectory + System.IO.Path.GetFileName(tmp));
                                }
                            }
                        }
                    }
                }               
            }
            else
            {
                MessageBox.Show("Brand already installed");
            }

            clickCounter.AddClick((int)Buttons.InstallFittingSoftware);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        
        private void cmbbrandstoinstall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] marki = {"","","", "EXPRESSfit", "EXPRESSfitComposition", "EXPRESSFit", "Genie", "GenieComposition","Genie",  "Oasis", "OasisComposition","Oasis" };
            bool binded = false;
            Directory_toIntall = "";
            if (cmbBrandstoinstall.SelectedIndex > -1)
            {
                try
                {
                    if ((cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8 || cmbBrandstoinstall.SelectedIndex == 11)) // PRE
                    {
                        string tmp = $"//demant.com/data/KBN/RnD/FS_Programs/Fitting Applications/{marki[cmbBrandstoinstall.SelectedIndex]}/20{txtCompositionPart2.Text.ToString()}/Pre-releases/";
                        Directory_toIntall = tmp;
                        if (!binded)
                        {
                            cbindBuild(tmp);
                            binded = true;
                        }
                    }
                    else if ((cmbBrandstoinstall.SelectedIndex == 3 || cmbBrandstoinstall.SelectedIndex == 6 || cmbBrandstoinstall.SelectedIndex == 9)) //zwykle
                    {
                        if (!binded)
                        {
                            if (txtCompositionPart2.Text != "18.1")
                            {
                                Directory_toIntall = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\20" + txtCompositionPart2.Text.ToString()+ @" Release" + @"\"
                                  + cmbBuild.Text + @"\" + "Full" + @"\"
                                 + cmbBrandstoinstall.SelectedValue.ToString() + @"\Setup.exe";
                                string pathh = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\20" + txtCompositionPart2.Text.ToString() + @" Release" + @"\";
                                cbindBuild(pathh);
                            }
                            else
                            {
                                Directory_toIntall = $"//10.128.3.1/DFS_Data_SSC_FS_GenieBuilds/Phoenix/{marki[cmbBrandstoinstall.SelectedIndex]}/";
                                cbindBuild($"//10.128.3.1/DFS_Data_SSC_FS_GenieBuilds/Phoenix/{marki[cmbBrandstoinstall.SelectedIndex]}/");
                            }
                            binded = true;
                        }
                    }
                    else if ((cmbBrandstoinstall.SelectedIndex == 4 || cmbBrandstoinstall.SelectedIndex == 7 || cmbBrandstoinstall.SelectedIndex == 10)) //CVompozycje
                    {
                        string tmp = $"//demant.com/data/KBN/RnD/FS/DevResults/Phoenix/Dev_16.1/{marki[cmbBrandstoinstall.SelectedIndex]}/";
                        Directory_toIntall = tmp;
                        if (!binded)
                            cbindBuild($"//demant.com/data/KBN/RnD/FS/DevResults/Phoenix/Dev_16.1/{marki[cmbBrandstoinstall.SelectedIndex]}/");
                        binded = true;
                    } else if((cmbBrandstoinstall.SelectedIndex == 0 || cmbBrandstoinstall.SelectedIndex == 1 || cmbBrandstoinstall.SelectedIndex == 2)) //Nightly
                    {
                        Directory_toIntall = $"//demant.com/data/KBN/RnD/SWS/Build/Arizona/Phoenix/Nightly-18.1/";
                        cbindBuild($"//demant.com/data/KBN/RnD/SWS/Build/Arizona/Phoenix/Nightly-18.1/");
                        binded = true;
                    }
                }
                catch (Exception e1)
                {
                    dataBaseManager.LogException(e1.ToString(), "cmbbrandstoinstall_SelectionChanged Szczecin");
                    try //parsowanie katalogów dla ludzi nie ze szczecina
                    {
                        if ((cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8 || cmbBrandstoinstall.SelectedIndex == 11)) // PRE
                        {
                            string tmp = $"//demant.com/data/KBN/RnD/FS_Programs/Fitting Applications/{marki[cmbBrandstoinstall.SelectedIndex]}/20{Releases_prereleases}/Pre-releases/";
                            Directory_toIntall = tmp;
                            if (!binded)
                                cbindBuild(tmp);
                            binded = true;
                        }
                        else if ((cmbBrandstoinstall.SelectedIndex == 3 || cmbBrandstoinstall.SelectedIndex == 6 || cmbBrandstoinstall.SelectedIndex == 9)) //zwykle
                        {
                            Directory_toIntall = $"//demant.com/data/KBN/RnD/SWS/Build/Projects/{marki[cmbBrandstoinstall.SelectedIndex]}/";
                            cbindBuild($"//demant.com/data/KBN/RnD/SWS/Build/Projects/{marki[cmbBrandstoinstall.SelectedIndex]}/");
                            binded = true;
                        }
                    }
                    catch (Exception e2)
                    {
                        try
                        {
                            if ((cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8 || cmbBrandstoinstall.SelectedIndex == 11)) // PRE
                            {
                                string tmp = $"//demant.com/data/KBN/RnD/FS_Programs/Fitting Applications/{marki[cmbBrandstoinstall.SelectedIndex]}/20{txtCompositionPart2.Text.ToString()}/Pre-releases/";
                                Directory_toIntall = tmp;
                                if (!binded)
                                    cbindBuild(tmp);
                                binded = true;
                            }
                            else if ((cmbBrandstoinstall.SelectedIndex == 4 || cmbBrandstoinstall.SelectedIndex == 7 || cmbBrandstoinstall.SelectedIndex == 10)) //Compozycje
                            {
                                string tmp = $"//demant.com/data/KBN/RnD/FS/DevResults/Phoenix/Dev_16.1/{marki[cmbBrandstoinstall.SelectedIndex]}/";
                                Directory_toIntall = tmp;
                                if (!binded)
                                    cbindBuild($"//demant.com/data/KBN/RnD/FS/DevResults/Phoenix/Dev_16.1/{marki[cmbBrandstoinstall.SelectedIndex]}/");
                                binded = true;
                            }
                        }
                        catch (Exception e3)
                        {  
                         MessageBox.Show("No Access to directory");
                         dataBaseManager.LogException(e3.ToString(), "cmbbrandstoinstall_SelectionChanged composition");
                        }
                        dataBaseManager.LogException(e2.ToString(), "cmbbrandstoinstall_SelectionChanged Denmark");                        
                    }                    
                }
            }
            cmbBrandstoinstall.ToolTip = Directory_toIntall;
        }
        private void cmbbuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void LoggingMouseEnter(object sender, MouseEventArgs e)
        {
            int border;
            Border elementLeft, elementRight, buf;
            buf = new Border();
            for (int y=0; y<7; ++y)
            {
                border = 11;
                for (int x=0; x<6; ++x)
                {
                    elementLeft = Mario.Children.Cast<Border>().FirstOrDefault(b => Grid.GetColumn(b) == x && Grid.GetRow(b) == y);
                    elementRight = Mario.Children.Cast<Border>().FirstOrDefault(b => Grid.GetColumn(b) == border && Grid.GetRow(b) == y);
                    buf.Background = elementRight.Background;
                    elementRight.Background = elementLeft.Background;
                    elementLeft.Background = buf.Background;
                    border--;
                }
            }
        }

        private void btnChangeDate_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime;
            if (calendar.SelectedDate.HasValue)
            {
                dateTime = calendar.SelectedDate.Value;
                clockManager.SetTime((short)dateTime.Year, (short)dateTime.Month, (short)dateTime.Day);
            }
            else
            {
                dateTime = DateTime.Now;
                clockManager.SetTime((short)dateTime.Year, (short)dateTime.Month, (short)dateTime.Day);
            }
            clockManager.DateWasSet();
        }

        private void btnHoursDown_Click(object sender, RoutedEventArgs e)
        {
            clockManager.HourDown();
            lblTime.Content = clockManager.GetTime();
            clockManager.DateWasChanged();
        }

        private void btnHoursUp_Click(object sender, RoutedEventArgs e)
        {
            clockManager.HourUp();
            lblTime.Content = clockManager.GetTime();
            clockManager.DateWasChanged();
        }

        private void btnMinutesDown_Click(object sender, RoutedEventArgs e)
        {
            clockManager.MinuteDown();
            lblTime.Content = clockManager.GetTime();
            clockManager.DateWasChanged();
        }

        private void btnMinutesUp_Click(object sender, RoutedEventArgs e)
        {
            clockManager.MinuteUp();
            lblTime.Content = clockManager.GetTime();
            clockManager.DateWasChanged();
        }

        private void btnResetDate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("CMD.exe", "/C NET TIME /domain:EMEA /SET /Y");
            lblTime.Content = clockManager.GetTime();

        }

        private void btnLogToDB_Click(object sender, RoutedEventArgs e)
        {
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string firstHalf = cmbBuild.Text.ToString().Split(new char[] { ' ' }, 2)[0];
            cmbBuild.ToolTip = Directory_toIntall + firstHalf;
        }

        private void textBox_TextChanged(object sender, RoutedEventArgs e)
        {
        }
        
        private void set_mode_run_app()
        {

            if (rbnNotStartwithWindows.IsChecked.Value)
            {
                //File.WriteAllText(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt", "0");
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe"))
                {
                    try
                    {
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe");
                    }
                    catch (Exception)
                    {
                       
                    }
                }
            }



            if (rbnStartwithWindows.IsChecked.Value)
            {


                if (fileOperator.Get_run_with_Application())
                {
                    rbnStartwithWindows.IsChecked = true;
                    rbnNotStartwithWindows.IsChecked = false;
                }
                else
                {
                    rbnNotStartwithWindows.IsChecked = true;
                    rbnStartwithWindows.IsChecked = false;
                }

                
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe"))
                {
                    try
                    {
                        File.Copy(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\Ultimate Changer.exe",  Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe");
                    }
                    catch (Exception)
                    {
                       
                    }
                }

                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt"))
                    {
                        String line = sr.ReadToEnd();
                        sr.Close();
                        if (line != "1")
                        {
                            File.WriteAllText(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt", "1");
                        }
                    }
                }
                catch (Exception e)
                {
                    dataBaseManager.LogException(e.ToString(), "set_mode_run_app ");
                }
                
            }
            else
            {
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt"))
                    {
                        String line = sr.ReadToEnd();
                        sr.Close();
                        if (line != "0")
                        {
                            File.WriteAllText(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info.txt", "0");
                        }
                    }
                }
                catch (Exception e)
                {
                   
                }
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ultimate Changer.exe");
                }
               
            }

            if (fileOperator.Get_run_with_Application())
            {
                rbnStartwithWindows.IsChecked = true;
                rbnNotStartwithWindows.IsChecked = false;
            }
            else
            {
                rbnNotStartwithWindows.IsChecked = true;
                rbnStartwithWindows.IsChecked = false;
            }


        }
            
        private void CheckFileinfo2()
        {
            
            if (rbnDeletelogs.IsChecked.Value)
            {
                
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt"))
                    {
                        String line = sr.ReadToEnd();
                        sr.Close();
                        if (line != "1")
                        {
                            File.WriteAllText(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt", "1");
                        }
                    }
                }
                catch (Exception e)
                {
                    dataBaseManager.LogException(e.ToString(), "CheckFileinfo2 ");
                }

            }
            else
            {
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt"))
                    {
                        String line = sr.ReadToEnd();
                        sr.Close();
                        if (line != "0")
                        {
                            File.WriteAllText(@"C:\Program Files\DGS - PAZE & MIBW\Multi Changer\info2.txt", "0");
                        }
                    }
                }
                catch (Exception e)
                {
                    dataBaseManager.LogException(e.ToString(), "CheckFileinfo2 ");
                }


            }

            if (fileOperator.Get_delete_logs_file())
            {
                rbnDeletelogs.IsChecked = true;
                rbnholdlogs.IsChecked = false;
                SetLogSettings(true);
            }
            else
            {
                rbnDeletelogs.IsChecked = false;
                rbnholdlogs.IsChecked = true;
                SetLogSettings(false);
            }


        }
                      
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            fileOperator.UpdateLabels();
            //this.updateLabels();
            this.verifyInstalledBrands();
            this.startAnimation();
            this.checkInstallationStatus();
            this.TemporaryToolTipMethod();
            set_mode_run_app();
            CheckFileinfo2();
            clockManager.UpdateTime();
            lblTime.Content = clockManager.GetTime();
        }

        private void checkUninstallation_Tick(object sender, EventArgs e)
        {
            bool enduninstall;
            int numberOfUninstallations = 0;

            for (int brandNumber = 0; brandNumber < 3; brandNumber++)
            {
                enduninstall = this.checkRunningProcess(procesy_uninstall[brandNumber]);
                if (enduninstall == false)
                {
                    numberOfUninstallations++;
                }
            }

            if (numberOfUninstallations == 0)
            {
                uninstallTimer.Stop();
                deletesmieci();
                MessageBox.Show("Odinstalowano pomyslnie");
                btninstal.IsEnabled = true;
            }
            else
            {
                //cos tu sie dzieje
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void txtCompositionPart2_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Releases_prereleases
            Releases_prereleases = txtCompositionPart2.Text;
        }

        private void txtOEM_TextChanged(object sender, TextChangedEventArgs e)
        {
            OEMname = txtOEM.Text;
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            string message="";

                try
                {
                if (txtCompositionPart2.Text != "18.1")
                {
                    message = $"{Directory_toIntall } \nYES to copy ";
                }
                else
                {
                    string firstHalf = cmbBuild.Text.ToString().Split(new char[] { ' ' }, 2)[0];
                    message = $"{Directory_toIntall + firstHalf } \nYES to copy ";
                }
                }
                catch (Exception e11)
                {
                    dataBaseManager.LogException(e11.ToString(), "btnInfo_Click");
                    message = e11.ToString();
                }

            string caption = "Build directory";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result_choice = MessageBox.Show(this, message, caption, buttons);

            if (result_choice.ToString() == "Yes")
            {
                string good_path = Directory_toIntall.Replace('/','\\');
                string firstHalf = cmbBuild.Text.ToString().Split(new char[] { ' ' }, 2)[0];
                if (cmbBrandstoinstall.SelectedIndex == 5 || cmbBrandstoinstall.SelectedIndex == 8 || cmbBrandstoinstall.SelectedIndex == 11 )
                    Clipboard.SetText(good_path);
                else
                    Clipboard.SetText($"{good_path + firstHalf}");
            }
            else
            {
                return; // wyjscie z metody ?? No.
            }
        }
    }

}