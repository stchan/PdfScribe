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
    public class Program
    {

        static Application activityWindow;

        [STAThread]
        static void Main(string[] args)
        {
            // Install the global exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);

            ShowActivitityNotificationWindow();
            Thread.Sleep(3000);

            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = Path.Combine(Path.GetTempPath(), "OAISISSOFTSCAN.PDF");

            // Only set absolute minimum parameters, let the postscript input
            // dictate as much as possible
            String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
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
                CloseActivityNotificationWindow();
            }
        }

        /// <summary>
        /// All unhandled exceptions will bubble their way up here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }


        public static void ShowActivitityNotificationWindow()
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
        }

        public static void CloseActivityNotificationWindow()
        {
            activityWindow.Dispatcher.InvokeShutdown();
        }
    }
}
