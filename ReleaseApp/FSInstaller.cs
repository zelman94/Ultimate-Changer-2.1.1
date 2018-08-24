using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace UltimateChanger
{
    static public class FSInstaller 
    {
        static public bool InstallBrand(string path, bool mode_normal)
        {
            if (File.Exists(path) && mode_normal)
            {
                try
                {
                    Process.Start(path);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
            if (File.Exists(path) && !mode_normal) //silent installation
            {
                try
                {
                    //string tmp2 = @"\\10.128.3.1\DFS_Data_SSC_FS_GenieBuilds\Phoenix\ExpressFit\ExpressFit_4.0.784.161\Full\Sonic\Setup.exe /quiet";
                    Process.Start(path , " /install /quiet");
                    return true;
                }
                catch (Exception)
                {                   
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        static public void UninstallBrand()
        {

        }
    }
}
