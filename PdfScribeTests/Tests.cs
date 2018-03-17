using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using PdfScribeCore;

namespace PdfScribeTests
{
    public class Tests
    {
        #region PdfScribeCore Tests
        //[Test]
        public void Test_DeletePdfScribePort()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.DeletePdfScribePort();
        }

        //[Test]
        public void Test_RemovePdfScribeDriver()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.RemovePDFScribePrinterDriver();
        }

        //[Test]
        public void Test_AddPdfScribePort()
        {

            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.AddPdfScribePort_Test();
        }

        //[Test]
        public void Test_IsPrinterDriverInstalled()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.IsPrinterDriverInstalled_Test("PDF Scribe Virtual Printer");
        }

        //[Test]
        public void Test_InstallPdfScribePrinter()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.InstallPdfScribePrinter(@"C:\Code\PdfScribe\Lib\", String.Empty, String.Empty);
        }

        //[Test]
        public void Test_UninstallPdfScribePrinter()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.UninstallPdfScribePrinter();
        }

        //[Test]
        public void Test_RemovePdfScribePortMonitor()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.RemovePdfScribePortMonitor();
        }

        #endregion

    }
}
