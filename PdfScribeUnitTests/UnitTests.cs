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

        [Test]
        public void Test_DeletePdfScribePort()
        {
            var scribeInstaller = new PdfScribeInstaller();
            scribeInstaller.DeletePdfScribePort("SSCAN");
        }

    }
}
