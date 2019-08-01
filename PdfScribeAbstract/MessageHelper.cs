using System;
using System.Windows.Forms;

namespace PdfScribe
{
    public static class MessageHelper
    {
        /// <summary>
        /// Displays up a topmost, OK-only message box for the error message
        /// </summary>
        /// <param name="boxCaption">The box's caption</param>
        /// <param name="boxMessage">The box's message</param>
        public static void DisplayErrorMessage(String boxCaption,
            String boxMessage)
        {

            MessageBox.Show(boxMessage,
                boxCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);

        }
    }
}