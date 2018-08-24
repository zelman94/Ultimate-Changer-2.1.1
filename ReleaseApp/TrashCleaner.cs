using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace UltimateChanger
{
    public class TrashCleaner
    {
        DataBaseManager dataBase;

        private Dictionary<string, string> BrandtoSoft;

        public TrashCleaner(Dictionary<string, string> BrandtoSoft, DataBaseManager dataBase)
        {
            this.BrandtoSoft = BrandtoSoft;
            this.dataBase = dataBase;
        }

        public void DeleteTrash(string DirectoryName)
        {
            string tempName;
            if (Directory.Exists(DirectoryName))
            {
                DirectoryInfo di = new DirectoryInfo(DirectoryName);
                try
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        tempName = $"{di.ToString()}/{file.Name.ToString()}";
                        if (File.Exists($"{di.ToString()}/{file.Name.ToString()}"))
                        {
                            File.SetAttributes(tempName, FileAttributes.Normal);
                            File.Delete($"{di.ToString()}/{file.Name.ToString()}");
                        }
                        else
                        {

                        }
                    }
                    foreach (DirectoryInfo directory in di.GetDirectories())
                    {
                        if (Directory.Exists(di.ToString() + "/" + directory.ToString()))
                        {
                            DeleteTrash(di.ToString() + "/" + directory.ToString());
                            Directory.Delete(di.ToString() + "/" + directory.ToString(),true);
                        }
                        else
                        {

                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    dataBase.LogException(e.ToString(), "deleteTrash Unauthorized Access");
                    MessageBox.Show($"Cos sie zepsulo {e.ToString()}");
                }
                catch (DirectoryNotFoundException ee)
                {
                    dataBase.LogException(ee.ToString(), "deleteTrash Directory Not Found");
                    MessageBox.Show($"Cos sie mocno zepsulo {ee.ToString()}");
                    return;
                }
            }
            Directory.Delete(DirectoryName);
        }

        public void DeleteLogs(string brand_name)
        {
            string DirectoryName = $"C:/ProgramData/{brand_name}/{BrandtoSoft[brand_name]}/Logfiles/";

            System.IO.DirectoryInfo di = new DirectoryInfo(DirectoryName);
            try
            {
                string tempName;
                if(Directory.Exists(DirectoryName))
                foreach (FileInfo file in di.GetFiles())
                {
                    tempName = $"{di.ToString()}/{file.Name.ToString()}";
                    if (File.Exists(tempName))
                    {
                        File.SetAttributes(tempName, FileAttributes.Normal);
                        File.Delete($"{di.ToString()}/{file.Name.ToString()}");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                dataBase.LogException(ex.ToString(), "deleteLogs");
                MessageBox.Show("Cos sie zepsulo");
            }
            catch (DirectoryNotFoundException ee)
            {
                dataBase.LogException(ee.ToString(), "deleteLogs");
                MessageBox.Show("Cos sie mocno zepsulo");
            }
        }
    }
}
