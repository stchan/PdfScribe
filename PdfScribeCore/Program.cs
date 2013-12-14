using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfScribe
{
    class Program
    {
        static void Main(string[] args)
        {
            //RemovePort();
            //RemovePortMonitor();
            //TestGetPorts();
            //AddPort();
            //TestGetPrinterDriverDir();
            //TestInstallPrinterDriver();
            DeletePdfScribePort();
        }

        public static void AddPort()
        {
            var installer = new PdfScribeInstaller();
            installer.AddPdfScribePortMonitor("SOFTSCAN",
                                             "redmon64.dll",
                                             @"C:\Code\PdfScribe\Lib\");
        }

        public static void DeletePdfScribePort()
        {
            var installer = new PdfScribeInstaller();
            installer.DeletePdfScribePort("SSCAN");
        }

        public static void RemovePortMonitor()
        {
            var installer = new PdfScribeInstaller();
            installer.RemoveSoftscanPortMonitor("SOFTSCAN");
            installer.RemoveSoftscanPortMonitor("OAISISSOFTSCAN");
        }

        public static void TestGetPorts()
        {
            var installer = new PdfScribeInstaller();
            List<MONITOR_INFO_2> monList = installer.EnumerateMonitors();
            foreach (MONITOR_INFO_2 currentMonitor in monList)
            {
                Console.WriteLine(currentMonitor.pName + " / " +
                                  currentMonitor.pEnvironment + " / " +
                                  currentMonitor.pDLLName);
            }
        }

        public static void TestGetPrinterDriverDir()
        {
            var installer = new PdfScribeInstaller();
            String driverDir = installer.RetrievePrinterDriverDirectory();
        }

        public static void TestInstallPrinterDriver()
        {
            var installer = new PdfScribeInstaller();
            /*installer.AddSoftscanPortMonitor("OAISISSOFTSCAN",
                                 "redmon64.dll",
                                 @"C:\Code\OaisisRedmonInstaller\Lib\"); */
            installer.InstallSoftscanPrinter_Test();
            //installer.RemoveSoftscanPortMonitor("SOFTSCAN");

        }
    }
}
