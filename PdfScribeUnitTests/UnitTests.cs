using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using PdfScribe;

namespace PdfScribeUnitTests
{
    [TestFixture]
    public class UnitTests
    {
        #region Ctor
        public UnitTests()
        { }
        #endregion

        //[Test]
        public void Test_DeletePdfScribePort()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.DeletePdfScribePort("SSCAN");
        }

        //[Test]
        public void Test_RemovePdfScribeDriver()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.RemovePDFScribePrinterDriver();
        }

        //[Test]
        public void Test_InstallPdfScribePrinter()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.InstallSoftscanPrinter_Test();
        }

        [Test]
        public void Test_UninstallPdfScribePrinter()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.UninstallPdfScribePrinter();
        }
    }
}
