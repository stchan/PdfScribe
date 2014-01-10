using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using APICodePack = Microsoft.WindowsAPICodePack.Dialogs;

namespace PdfScribeCore
{
    public abstract class TaskDialogPresenter
    {

        protected abstract APICodePack.TaskDialogStandardIcon DefaultTaskIcon { get; } // Override this to set the dialog icon you want


        public TaskDialogPresenter()
        { }

        /// <summary>
        /// Ctor that shows the
        /// task dialog immediately
        /// </summary>
        /// <param name="captionText">Text that goes in the window caption</param>
        /// <param name="instructionText">Instructional text (Appears next to the icon)</param>
        /// <param name="messageText">Smaller message detail text at bottom</param>
        public TaskDialogPresenter(String captionText,
                                        String instructionText,
                                        String messageText)
        {
            ShowSimple(captionText, instructionText, messageText);
        }

        /// <summary>
        /// Pops up a simple TaskDialog box
        /// with just a Close button and
        /// the default standard dialog icon
        /// </summary>
        /// <param name="captionText">Text that goes in the window's caption</param>
        /// <param name="instructionText">Instructional text (Appears next to the error icon)</param>
        /// <param name="messageText">Smaller message detail text at bottom</param>
        public virtual void ShowSimple(String captionText,
                                       String instructionText,
                                       String messageText)
        {
            using (APICodePack.TaskDialog simpleTaskDialog = new APICodePack.TaskDialog())
            {
                simpleTaskDialog.Caption = captionText;
                simpleTaskDialog.InstructionText = instructionText;
                simpleTaskDialog.Text = messageText;
                simpleTaskDialog.Icon = this.DefaultTaskIcon;
                simpleTaskDialog.StandardButtons = APICodePack.TaskDialogStandardButtons.Close;
                simpleTaskDialog.Opened += new EventHandler(simpleTaskDialog_Opened);
                simpleTaskDialog.StartupLocation = APICodePack.TaskDialogStartupLocation.CenterScreen;
                simpleTaskDialog.Show();
                //System.Windows.Threading.Dispatcher.Run();
            }
        }

        private void simpleTaskDialog_Opened(object sender, EventArgs e)
        {
            // Really fucking annoying -
            // There's a bug somewhere in the API Code Pack that
            // causes the icon not to show
            // unless you set it on the Opened event
            // See: http://stackoverflow.com/questions/15645592/taskdialogstandardicon-not-working-on-task-dialog
            // One of these days I'll try to find and fix it (honestly I hope
            // someone else fixes first - also why isn't the API Code pack on codeplex
            // or github so people can push patches), but until then...
            ((APICodePack.TaskDialog)sender).Icon = this.DefaultTaskIcon;
            
        }

    }
}
