using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using APICodePack = Microsoft.WindowsAPICodePack.Dialogs;

namespace PdfScribe
{
    public class ErrorDialogPresenter
    {
        #region Ctor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ErrorDialogPresenter()
        {}

        /// <summary>
        /// Ctor overload that shows the
        /// task dialog immediately
        /// </summary>
        /// <param name="captionText"></param>
        /// <param name="instructionText"></param>
        /// <param name="messageText"></param>
        public ErrorDialogPresenter(String captionText,
                                     String instructionText,
                                     String messageText)
        {
            ShowSimple(captionText, instructionText, messageText);
        }

        #endregion

        /// <summary>
        /// Pops up a simple TaskDialog box
        /// with a standard error icon, and
        /// just a Close button
        /// </summary>
        /// <param name="captionText"></param>
        /// <param name="instructionText"></param>
        /// <param name="messageText"></param>
        public void ShowSimple(String captionText,
                               String instructionText,
                               String messageText)
        {
            using (APICodePack.TaskDialog errorDialog = new APICodePack.TaskDialog())
            {
                errorDialog.Caption = captionText;
                errorDialog.InstructionText = instructionText;
                errorDialog.Text = messageText;
                errorDialog.Icon = APICodePack.TaskDialogStandardIcon.Error;
                errorDialog.StandardButtons = APICodePack.TaskDialogStandardButtons.Close;
                errorDialog.Opened += new EventHandler(errorDialog_Opened);
                errorDialog.Show();                
            }
        }

        private void errorDialog_Opened(object sender, EventArgs e)
        {
            // Really fucking annoying -
            // There's a bug somewhere in the API Code Pack that
            // causes the icon not to show
            // unless you set it on the Opened event
            // See: http://stackoverflow.com/questions/15645592/taskdialogstandardicon-not-working-on-task-dialog
            // One of these days I'll try to find and fix it (honestly I hope
            // someone else fixes first - also why isn't the API Code pack on codeplex
            // or github so people can push patches), but until then...
            ((APICodePack.TaskDialog)sender).Icon = APICodePack.TaskDialogStandardIcon.Error;
        }

    }
}
