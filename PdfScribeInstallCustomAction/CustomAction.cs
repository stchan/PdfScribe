using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

using PdfScribeCore;

namespace PdfScribeInstallCustomAction
{
    public class CustomActions
    {

        [CustomAction]
        public static ActionResult CheckIfPrinterNotInstalled(Session session)
        {
            ActionResult resultCode;

            PdfScribeInstaller installer = new PdfScribeInstaller();
            if (installer.IsPdfScribePrinterInstalled())
            {
                resultCode = ActionResult.Success;
            }
            else
            {
                resultCode = ActionResult.Failure;
            }

            return resultCode;
        }
    }
}
