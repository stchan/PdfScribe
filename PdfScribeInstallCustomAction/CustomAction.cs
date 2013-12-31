using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

using PdfScribeCore;

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

            PdfScribeInstaller installer = new PdfScribeInstaller();
            if (installer.IsPdfScribePrinterInstalled())
                resultCode = ActionResult.Success;
            else
                resultCode = ActionResult.Failure;

            return resultCode;
        }


        [CustomAction]
        public static ActionResult InstallPdfScribePrinter(Session session)
        {
            ActionResult printerInstalled;

            String driverSourceDirectory = session.CustomActionData["DriverSourceDirectory"];
            String outputCommand = session.CustomActionData["OutputCommand"];
            String outputCommandArguments = session.CustomActionData["OutputCommandArguments"];

            PdfScribeInstaller installer = new PdfScribeInstaller();

            if (installer.InstallPdfScribePrinter(driverSourceDirectory,
                                              outputCommand,
                                              outputCommandArguments))
                printerInstalled = ActionResult.Success;
            else
                printerInstalled = ActionResult.Failure;

            return printerInstalled;
        }


        [CustomAction]
        public static ActionResult UninstallPdfScribePrinter()
        {
            ActionResult printerUninstalled;

            PdfScribeInstaller installer = new PdfScribeInstaller();
            if (installer.UninstallPdfScribePrinter())
                printerUninstalled = ActionResult.Success;
            else
                printerUninstalled = ActionResult.Failure;

            return printerUninstalled;
        }
    }
}
