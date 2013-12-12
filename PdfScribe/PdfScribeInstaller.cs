using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace PdfScribe
{
    public class PdfScribeInstaller
    {

        const string ENVIRONMENT_64 = "Windows x64";
        const string PRINTERNAME = "PDF Scribe";
        const string DRIVERNAME = "PDF Scribe Virtual Printer";
        const string HARDWAREID = "PDFScribe_Driver0101";
        const string PORTMONITOR = "PDFSCRIBE";
        const string MONITORDLL = "redmon64.dll";
        const string PORTNAME = "PSCRIBE:";
        const string PRINTPROCESOR = "winprint";

        const string DRIVERMANUFACTURER = "S T Chan";
        
        const string DRIVERFILE = "PSCRIPT5.DLL";
        const int DRIVERFILE_INDEX = 0;
        const string DRIVERUIFILE = "PS5UI.DLL";
        const int DRIVERUIFILE_INDEX = 1;
        const string DRIVERHELPFILE = "PSCRIPT.HLP";
        const int DRIVERHELPFILE_INDEX = 2;
        const string DRIVERDATAFILE = "SCPDFPRN.PPD";
        const int DRIVERDATAFILE_INDEX = 3;


        #region Port operations

        private int AddPdfScribePort(string portName)
        {
            return DoXcvDataPortOperation(portName, "AddPort");
        }

        public void DeletePdfScribePort(string portName)
        {
            DoXcvDataPortOperation(portName, "DeletePort");
        }

        private int DoXcvDataPortOperation(string portName, string xcvDataOperation)
        {

            int win32ErrorCode;

            PRINTER_DEFAULTS def = new PRINTER_DEFAULTS();

            def.pDatatype = null;
            def.pDevMode = IntPtr.Zero;
            def.DesiredAccess = 1; //Server Access Administer

            IntPtr hPrinter = IntPtr.Zero;

            if (NativeMethods.OpenPrinter(",XcvMonitor " + PORTMONITOR, ref hPrinter, def) != 0)
            {
                if (!portName.EndsWith("\0"))
                    portName += "\0"; // Must be a null terminated string

                // Must get the size in bytes. Rememeber .NET strings are formed by 2-byte characters
                uint size = (uint)(portName.Length * 2);

                // Alloc memory in HGlobal to set the portName
                IntPtr portPtr = Marshal.AllocHGlobal((int)size);
                Marshal.Copy(portName.ToCharArray(), 0, portPtr, portName.Length);

                uint needed; // Not that needed in fact...
                uint xcvResult; // Will receive de result here

                NativeMethods.XcvData(hPrinter, xcvDataOperation, portPtr, size, IntPtr.Zero, 0, out needed, out xcvResult);

                NativeMethods.ClosePrinter(hPrinter);
                Marshal.FreeHGlobal(portPtr);
                win32ErrorCode = (int)xcvResult;
            }
            else
            {
                win32ErrorCode = Marshal.GetLastWin32Error();
            }
            return win32ErrorCode;

        }

        #endregion

        #region Port Monitor

        /// <summary>
        /// Adds the PDF Scribe port monitor
        /// </summary>
        /// <param name="monitorFilePath">Directory where the uninstalled monitor dll is located</param>
        /// <returns>true if the monitor is installed, false if install failed</returns>
        public bool AddPdfScribePortMonitor(String monitorFilePath)
        {
            bool monitorAdded = false;

            IntPtr oldRedirectValue = IntPtr.Zero;

            try
            {
                oldRedirectValue = DisableWow64Redirection();
                if (!DoesMonitorExist(PORTMONITOR))
                {
                    // Copy the monitor DLL to
                    // the system directory
                    String fileSourcePath = Path.Combine(monitorFilePath, MONITORDLL);
                    String fileDestinationPath = Path.Combine(Environment.SystemDirectory, MONITORDLL);
                    File.Copy(fileSourcePath, fileDestinationPath, true);
                    MONITOR_INFO_2 newMonitor = new MONITOR_INFO_2();
                    newMonitor.pName = PORTMONITOR;
                    newMonitor.pEnvironment = ENVIRONMENT_64;
                    newMonitor.pDLLName = MONITORDLL;
                    if (!AddPortMonitor(newMonitor))
                        throw new Win32Exception(Marshal.GetLastWin32Error(), String.Format("Could not add port monitor {0}", PORTMONITOR));
                    else
                        monitorAdded = true;
                }

            }
            finally
            {
                if (oldRedirectValue != IntPtr.Zero) RevertWow64Redirection(oldRedirectValue);
            }


            return monitorAdded;
        }


        /// <summary>
        /// Disables WOW64 system directory file redirection
        /// if the current process is both
        /// 32-bit, and running on a 64-bit OS
        /// </summary>
        /// <returns>A Handle, which should be retained to reenable redirection</returns>
        private IntPtr DisableWow64Redirection()
        {
            IntPtr oldValue = IntPtr.Zero;
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                if (!NativeMethods.Wow64DisableWow64FsRedirection(ref oldValue))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not disable Wow64 file system redirection.");
            return oldValue;
        }

        /// <summary>
        /// Reenables WOW64 system directory file redirection
        /// if the current process is both
        /// 32-bit, and running on a 64-bit OS
        /// </summary>
        /// <param name="oldValue">A Handle value - should be retained from call to <see cref="DisableWow64Redirection"/></param>
        private void RevertWow64Redirection(IntPtr oldValue)
        {
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                if (!NativeMethods.Wow64RevertWow64FsRedirection(oldValue))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not reenable Wow64 file system redirection.");
                }
            }
        }

        /// <summary>
        /// Removes the PDF Scribe port monitor
        /// </summary>
        /// <returns>true if monitor successfully removed, false if removal failed</returns>
        public bool RemovePdfScribePortMonitor()
        {
            bool monitorRemoved = false;
            if ((NativeMethods.DeleteMonitor(null, ENVIRONMENT_64, PORTMONITOR)) != 0)
                monitorRemoved = true;
            return monitorRemoved;
        }

        private bool AddPortMonitor(MONITOR_INFO_2 newMonitor)
        {
            bool monitorAdded = false;
            if ((NativeMethods.AddMonitor(null, 2, ref newMonitor) != 0))
            {
                monitorAdded = true;
            }
            return monitorAdded;
        }

        private bool DeletePortMonitor(String monitorName)
        {
            bool monitorDeleted = false;
            if ((NativeMethods.DeleteMonitor(null, ENVIRONMENT_64, monitorName)) != 0)
            {
                monitorDeleted = true;
            }
            return monitorDeleted;
        }

        private bool DoesMonitorExist(String monitorName)
        {
            bool monitorExists = false;
            List<MONITOR_INFO_2> portMonitors = EnumerateMonitors();
            foreach (MONITOR_INFO_2 portMonitor in portMonitors)
            {
                if (portMonitor.pName == monitorName)
                {
                    monitorExists = true;
                    break;
                }
            }
            return monitorExists;
        }

        public List<MONITOR_INFO_2> EnumerateMonitors()
        {
            List<MONITOR_INFO_2> portMonitors = new List<MONITOR_INFO_2>();

            uint pcbNeeded = 0;
            uint pcReturned = 0;

            if (!NativeMethods.EnumMonitors(null, 2, IntPtr.Zero, 0, ref pcbNeeded, ref pcReturned))
            {
                IntPtr pMonitors = Marshal.AllocHGlobal((int)pcbNeeded);
                if (NativeMethods.EnumMonitors(null, 2, pMonitors, pcbNeeded, ref pcbNeeded, ref pcReturned))
                {
                    IntPtr currentMonitor = pMonitors;

                    for (int i = 0; i < pcReturned; i++)
                    {
                        portMonitors.Add((MONITOR_INFO_2)Marshal.PtrToStructure(currentMonitor, typeof(MONITOR_INFO_2)));
                        currentMonitor = (IntPtr)(currentMonitor.ToInt32() + Marshal.SizeOf(typeof(MONITOR_INFO_2)));
                    }
                    Marshal.FreeHGlobal(pMonitors);

                }
                else
                {
                    // Failed to retrieve enumerate
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not enumerate monitor ports.");
                }

            }
            else
            {
                throw new ApplicationException("Call to winspool.drv succeeded with a zero size buffer - unexpected error.");
            }

            return portMonitors;
        }

        #endregion

        #region Printer Install

        public String RetrievePrinterDriverDirectory()
        {
            StringBuilder driverDirectory = new StringBuilder(1024);
            int dirSizeInBytes = 0;
            if (!NativeMethods.GetPrinterDriverDirectory(null,
                                                         null,
                                                         1,
                                                         driverDirectory,
                                                         1024,
                                                         ref dirSizeInBytes))
                throw new ApplicationException("Could not retrieve printer driver directory.");
            return driverDirectory.ToString();
        }

#if DEBUG
        public bool InstallSoftscanPrinter_Test()
        {
            String driverSourceDirectory = @"C:\Code\OaisisRedmonInstaller\Lib\";
            String[] driverFilesToCopy = new String[] { DRIVERFILE, DRIVERDATAFILE, DRIVERHELPFILE, DRIVERUIFILE };
            String[] dependentFilesToCopy = new String[] { "PSCRIPT.NTF" };
            return InstallPdfScribePrinter(driverSourceDirectory, driverFilesToCopy, dependentFilesToCopy);
        }
#endif

        /// <summary>
        /// Installs the port monitor, port,
        /// printer drivers, and PDF Scribe virtual printer
        /// </summary>
        /// <param name="driverSourceDirectory">Directory where the uninstalled printer driver files are located</param>
        /// <param name="driverFilesToCopy">An array containing the printer driver's filenames</param>
        /// <param name="dependentFilesToCopy">An array containing dependent filenames</param>
        /// <returns>true if printer installed, false if failed</returns>
        public bool InstallPdfScribePrinter(String driverSourceDirectory,
                                            String[] driverFilesToCopy,
                                            String[] dependentFilesToCopy)
        {
            bool printerInstalled = false;

            String driverDirectory = RetrievePrinterDriverDirectory();
            if (AddPdfScribePort(PORTNAME) == 0)
            {
                if (CopyPrinterDriverFiles(driverSourceDirectory, driverFilesToCopy.Concat(dependentFilesToCopy).ToArray()))
                {
                    if (AddPdfScribePortMonitor(driverSourceDirectory))
                    {
                        if (InstallPrinterDriver(driverDirectory, dependentFilesToCopy))
                        {
                            printerInstalled = AddPdfScribePrinter();
                        }
                    }
                }
            }
            return printerInstalled;
        }

        private bool CopyPrinterDriverFiles(String driverSourceDirectory,
                                            String[] filesToCopy)
        {
            bool filesCopied = false;
            String driverDestinationDirectory = RetrievePrinterDriverDirectory();
            try
            {
                for (int loop = 0; loop < filesToCopy.Length; loop++)
                {
                    String fileSourcePath = Path.Combine(driverSourceDirectory, filesToCopy[loop]);
                    String fileDestinationPath = Path.Combine(driverDestinationDirectory, filesToCopy[loop]);
                    try
                    {
                        File.Copy(fileSourcePath, fileDestinationPath);
                    }
                    catch (IOException)
                    {
                        // Just keep going - file was already
                        // there, but we didn't overwrite
                        continue;
                    }
                }
                filesCopied = true;
            }
            catch (UnauthorizedAccessException)
            { }
            catch (PathTooLongException)
            { }
            catch (DirectoryNotFoundException)
            { }
            catch (FileNotFoundException)
            { }
            catch (NotSupportedException)
            { }


            return filesCopied;
        }

        private bool DeletePrinterDriverFiles(String driverSourceDirectory,
                                              String[] filesToDelete)
        {
            bool allFilesDeleted = true;
            for (int loop = 0; loop < filesToDelete.Length; loop++)
            {
                try
                {
                    File.Delete(Path.Combine(driverSourceDirectory, filesToDelete[loop]));
                }
                catch
                {
                    allFilesDeleted = false;
                }
            }
            return allFilesDeleted;
        }

        private bool InstallPrinterDriver(String driverSourceDirectory,
                                          String[] dependentDriverFiles)
        {
            bool printerDriverInstalled = false;
            DRIVER_INFO_6 printerDriverInfo = new DRIVER_INFO_6();

            printerDriverInfo.cVersion = 3;
            printerDriverInfo.pName = DRIVERNAME;
            printerDriverInfo.pEnvironment = ENVIRONMENT_64;
            printerDriverInfo.pDriverPath = Path.Combine(driverSourceDirectory, DRIVERFILE);
            printerDriverInfo.pConfigFile = Path.Combine(driverSourceDirectory,DRIVERUIFILE);
            printerDriverInfo.pHelpFile = Path.Combine(driverSourceDirectory,DRIVERHELPFILE);
            printerDriverInfo.pDataFile = Path.Combine(driverSourceDirectory,DRIVERDATAFILE);

            StringBuilder nullTerminatedDependentFiles = new StringBuilder();
            if (dependentDriverFiles != null &&
                dependentDriverFiles.Length > 0)
            {
                for (int loop = 0; loop <= dependentDriverFiles.GetUpperBound(0); loop++)
                {
                    nullTerminatedDependentFiles.Append(dependentDriverFiles[loop]);
                    nullTerminatedDependentFiles.Append("\0");
                }
                nullTerminatedDependentFiles.Append("\0");
            }
            else
            {
                nullTerminatedDependentFiles.Append("\0\0"); 
            }
            printerDriverInfo.pDependentFiles = nullTerminatedDependentFiles.ToString();

            printerDriverInfo.pMonitorName = PORTMONITOR;
            printerDriverInfo.pDefaultDataType = String.Empty;
            printerDriverInfo.dwlDriverVersion = 0x0000000200000000U;
            printerDriverInfo.pszMfgName = DRIVERMANUFACTURER;
            printerDriverInfo.pszHardwareID = HARDWAREID;
            printerDriverInfo.pszProvider = DRIVERMANUFACTURER;

            printerDriverInstalled = NativeMethods.AddPrinterDriver(null, 6, ref printerDriverInfo);
            if (printerDriverInstalled == false)
            {
                int lastWinError = Marshal.GetLastWin32Error();
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not add printer PDF Scribe printer driver.");
            }
            return printerDriverInstalled;
        }


        private bool AddPdfScribePrinter()
        {
            bool printerAdded = false;
            PRINTER_INFO_2 pdfScribePrinter = new PRINTER_INFO_2();

            pdfScribePrinter.pServerName = null;
            pdfScribePrinter.pPrinterName = PRINTERNAME;
            pdfScribePrinter.pPortName = PORTNAME;
            pdfScribePrinter.pDriverName = DRIVERNAME;
            pdfScribePrinter.pPrintProcessor = PRINTPROCESOR;
            pdfScribePrinter.pDatatype = "RAW";
            pdfScribePrinter.Attributes = 0x00000002;

            int softScanPrinterHandle = NativeMethods.AddPrinter(null, 2, ref pdfScribePrinter);
            if (softScanPrinterHandle != 0)
            {
                // Added ok
                int closeCode = NativeMethods.ClosePrinter((IntPtr)softScanPrinterHandle);
                printerAdded = true;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not add PDF Scribe virtual printer.");
            }
            return printerAdded;
        }

        #endregion


    }
}
