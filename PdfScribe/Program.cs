using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace PdfScribe
{
    class Program
    {

        static Application activityWindow;

        [STAThread]
        static void Main(string[] args)
        {

            var activityWindowThread = new Thread(new ThreadStart(() =>
                {
                    activityWindow = new Application();
                    activityWindow.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    activityWindow.Run(new ActivityNotification());
                }
            ));
            activityWindowThread.SetApartmentState(ApartmentState.STA);
            activityWindowThread.Start();

            Thread.Sleep(3000);

            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = Path.Combine(Path.GetTempPath(), "OAISISSOFTSCAN.PDF");

            String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER", "-dCompatibilityLevel=1.4", "-dPDFSETTINGS=/prepress",   "-sDEVICE=pdfwrite",
                                              String.Format("-sOutputFile={0}", outputFilename), standardInputFilename };


            try
            {
                // Remove the existing OAISISSOFTSCAN.PDF file if present
                File.Delete(outputFilename);

                using (BinaryReader standardInputReader = new BinaryReader(Console.OpenStandardInput()))
                {
                    using (FileStream standardInputFile = new FileStream(standardInputFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        standardInputReader.BaseStream.CopyTo(standardInputFile);
                    }
                }
                GhostScript64.CallAPI(ghostScriptArguments);
            }
            catch (IOException)
            {
                // We couldn't delete, or create a file
                // because it was in use
            }
            catch (UnauthorizedAccessException)
            {
                // Couldn't delete a file
                // because it was set to readonly
                // or couldn't create a file
                // because of permissions issues
            }
            catch (ExternalException)
            {
                // Ghostscript error

            }
            finally
            {
                File.Delete(standardInputFilename);
                activityWindow.Dispatcher.InvokeShutdown();
            }
        }
    }
}
