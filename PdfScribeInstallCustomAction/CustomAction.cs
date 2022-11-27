using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

//using PdfScribeCore;

namespace PdfScribeInstallCustomAction
{
    /// <summary>
    /// Lotsa notes from here:
    /// http://stackoverflow.com/questions/835624/how-do-i-pass-msiexec-properties-to-a-wix-c-sharp-custom-action
    /// </summary>
    public class CustomActions
    {


        [CustomAction]
        public static ActionResult CheckIfPrinterNotInstalled(Session session)
        {
            ActionResult resultCode;
            SessionLogWriterTraceListener installTraceListener = new SessionLogWriterTraceListener(session);
            PdfScribeInstaller installer = new PdfScribeInstaller();
            installer.AddTraceListener(installTraceListener);
            try
            {
                if (installer.IsPdfScribePrinterInstalled())
                    resultCode = ActionResult.Success;
                else
                    resultCode = ActionResult.Failure;
            }
            finally
            {
                if (installTraceListener != null)
                    installTraceListener.Dispose();
            }

            return resultCode;
        }


        [CustomAction]
        public static ActionResult InstallPdfScribePrinter(Session session)
        {
            ActionResult printerInstalled;

            String driverSourceDirectory = session.CustomActionData["DriverSourceDirectory"];
            String outputCommand = session.CustomActionData["OutputCommand"];
            String outputCommandArguments = session.CustomActionData["OutputCommandArguments"];

            SessionLogWriterTraceListener installTraceListener = new SessionLogWriterTraceListener(session);
            installTraceListener.TraceOutputOptions = TraceOptions.DateTime;

            PdfScribeInstaller installer = new PdfScribeInstaller();
            installer.AddTraceListener(installTraceListener);
            try
            {


                if (installer.InstallPdfScribePrinter(driverSourceDirectory,
                                                      outputCommand,
                                                      outputCommandArguments))
                    printerInstalled = ActionResult.Success;
                else
                    printerInstalled = ActionResult.Failure;

                installTraceListener.CloseAndWriteLog();
            }
            finally
            {
                if (installTraceListener != null)
                    installTraceListener.Dispose();
                
            }
            return printerInstalled;
        }


        [CustomAction]
        public static ActionResult UninstallPdfScribePrinter(Session session)
        {
            ActionResult printerUninstalled;

            SessionLogWriterTraceListener installTraceListener = new SessionLogWriterTraceListener(session);
            installTraceListener.TraceOutputOptions = TraceOptions.DateTime;

            PdfScribeInstaller installer = new PdfScribeInstaller();
            installer.AddTraceListener(installTraceListener);
            try
            {
                if (installer.UninstallPdfScribePrinter())
                    printerUninstalled = ActionResult.Success;
                else
                    printerUninstalled = ActionResult.Failure;
                installTraceListener.CloseAndWriteLog();
            }
            finally
            {
                if (installTraceListener != null)
                    installTraceListener.Dispose();
            }
            return printerUninstalled;
        }
    }
}
