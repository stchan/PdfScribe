using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using APICodePack = Microsoft.WindowsAPICodePack.Dialogs;

namespace PdfScribeCore
{
    public class ErrorDialogPresenter : TaskDialogPresenter
    {
        protected override APICodePack.TaskDialogStandardIcon DefaultTaskIcon
        {
            get { return APICodePack.TaskDialogStandardIcon.Error; }
        }

        public ErrorDialogPresenter()
            : base()
        {

        }

        /// <summary>
        /// Ctor that shows the
        /// task dialog immediately
        /// </summary>
        /// <param name="captionText">Text that goes in the window caption</param>
        /// <param name="instructionText">Instructional text (Appears next to the icon)</param>
        /// <param name="messageText">Smaller message detail text at bottom</param>
        public ErrorDialogPresenter(String captionText,
                                    String instructionText,
                                    String messageText) :
                                    base(captionText, instructionText, messageText)
        {
        }



    }
}
