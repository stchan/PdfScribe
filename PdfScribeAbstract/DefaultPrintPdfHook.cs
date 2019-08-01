using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PdfScribe
{
    public class DefaultPrintPdfHook : IPrintPdfHook
    {
        const string errorDialogCaption = "PDF Scribe"; // Error taskdialog caption text

        const string errorDialogOutputFilenameInvalid = "Output file path is not valid. Check the \"OutputFile\" setting in the config file.";
        const string errorDialogOutputFilenameTooLong = "Output file path too long. Check the \"OutputFile\" setting in the config file.";
        const string errorDialogOutputFileAccessDenied = "Access denied - check permissions on output folder.";

        const string traceSourceName = "PdfScribe";

        static readonly TraceSource logEventSource = new TraceSource(traceSourceName);

        public virtual bool GetPdfOutputFilename(ref string outputFile)
        {
            bool filenameRetrieved = false;
            switch (Properties.Settings.Default.AskUserForOutputFilename)
            {
                case (true):
                    using (SetOutputFilename dialogOwner = new SetOutputFilename())
                    {
                        dialogOwner.TopMost = true;
                        dialogOwner.TopLevel = true;
                        dialogOwner.Show(); // Form won't actually show - Application.Run() never called
                                            // but having a topmost/toplevel owner lets us bring the SaveFileDialog to the front
                        dialogOwner.BringToFront();
                        using (SaveFileDialog pdfFilenameDialog = new SaveFileDialog())
                        {
                            pdfFilenameDialog.AddExtension = true;
                            pdfFilenameDialog.AutoUpgradeEnabled = true;
                            pdfFilenameDialog.CheckPathExists = true;
                            pdfFilenameDialog.Filter = "pdf files (*.pdf)|*.pdf";
                            pdfFilenameDialog.ShowHelp = false;
                            pdfFilenameDialog.Title = "PDF Scribe - Set output filename";
                            pdfFilenameDialog.ValidateNames = true;
                            if (pdfFilenameDialog.ShowDialog(dialogOwner) == DialogResult.OK)
                            {
                                outputFile = pdfFilenameDialog.FileName;
                                filenameRetrieved = true;
                            }
                        }
                        dialogOwner.Close();
                    }
                    break;
                default:
                    try
                    {
                        outputFile = GetOutputFilename();
                        // Test if we can write to the destination
                        using (FileStream newOutputFile = File.Create(outputFile))
                        { }
                        File.Delete(outputFile);
                        filenameRetrieved = true;
                    }
                    catch (Exception ex) when (ex is ArgumentException ||
                                               ex is ArgumentNullException ||
                                               ex is NotSupportedException ||
                                               ex is DirectoryNotFoundException)
                    {
                        logEventSource.TraceEvent(TraceEventType.Error,
                                                 (int)TraceEventType.Error,
                                                 errorDialogOutputFilenameInvalid + Environment.NewLine +
                                                 "Exception message: " + ex.Message);
                        MessageHelper.DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFilenameInvalid);
                    }
                    catch (PathTooLongException ex)
                    {
                        // filename is greater than 260 characters
                        logEventSource.TraceEvent(TraceEventType.Error,
                                                 (int)TraceEventType.Error,
                                                 errorDialogOutputFilenameTooLong + Environment.NewLine +
                                                 "Exception message: " + ex.Message);
                        MessageHelper.DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFilenameTooLong);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logEventSource.TraceEvent(TraceEventType.Error,
                                                 (int)TraceEventType.Error,
                                                 errorDialogOutputFileAccessDenied + Environment.NewLine +
                                                 "Exception message: " + ex.Message);
                        // Can't write to target dir
                        MessageHelper.DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFileAccessDenied);
                    }
                    break;
            }
            return filenameRetrieved;
        }

        /// <summary>
        /// Opens the PDF in the default viewer
        /// if the OpenAfterCreating app setting is "True"
        /// and the file extension is .PDF
        /// </summary>
        /// <param name="pdfFilename"></param>
        public virtual void OnPdfPrinted(string pdfFilename)
        {
            if (Properties.Settings.Default.OpenAfterCreating &&
                !String.IsNullOrEmpty(Path.GetExtension(pdfFilename)) &&
                (Path.GetExtension(pdfFilename).ToUpper() == ".PDF"))
            {
                Process.Start(pdfFilename);
            }
        }

        private static String GetOutputFilename()
        {
            String outputFilename = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.OutputFile));
            // Check if there are any % characters -
            // even though it's a legal Windows filename character,
            // it is a special character to Ghostscript
            if (outputFilename.Contains("%"))
                throw new ArgumentException("OutputFile setting contains % character.");
            return outputFilename;
        }
    }
}
