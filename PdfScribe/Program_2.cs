using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PdfScribe
{
    public partial class Program
    {
        static bool GetPdfOutputFilename(ref String outputFile)
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
                            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDMON_DOCNAME")))
                            {
                                // Replace illegal characters with spaces
                                Regex regEx = new Regex(@"[\\/:""*?<>|]");
                                pdfFilenameDialog.FileName = regEx.Replace(Environment.GetEnvironmentVariable("REDMON_DOCNAME"), " ");
                            }
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
                        DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFilenameInvalid);
                    }
                    catch (PathTooLongException ex)
                    {
                        // filename is greater than 260 characters
                        logEventSource.TraceEvent(TraceEventType.Error,
                                                 (int)TraceEventType.Error,
                                                 errorDialogOutputFilenameTooLong + Environment.NewLine +
                                                 "Exception message: " + ex.Message);
                        DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFilenameTooLong);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logEventSource.TraceEvent(TraceEventType.Error,
                                                 (int)TraceEventType.Error,
                                                 errorDialogOutputFileAccessDenied + Environment.NewLine +
                                                 "Exception message: " + ex.Message);
                        // Can't write to target dir
                        DisplayErrorMessage(errorDialogCaption,
                                            errorDialogOutputFileAccessDenied);
                    }
                    break;
            }
            return filenameRetrieved;

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


        /// <summary>
        /// Opens the PDF in the default viewer
        /// if the OpenAfterCreating app setting is "True"
        /// and the file extension is .PDF
        /// </summary>
        /// <param name="pdfFilename"></param>
        static void DisplayPdf(String pdfFilename)
        {
            if (Properties.Settings.Default.OpenAfterCreating &&
                !String.IsNullOrEmpty(Path.GetExtension(pdfFilename)) &&
                (Path.GetExtension(pdfFilename).ToUpper() == ".PDF"))
            {
                Process.Start(pdfFilename);
            }
        }

        /// <summary>
        /// Displays up a topmost, OK-only message box for the error message
        /// </summary>
        /// <param name="boxCaption">The box's caption</param>
        /// <param name="boxMessage">The box's message</param>
        static void DisplayErrorMessage(String boxCaption,
                                        String boxMessage)
        {

            MessageBox.Show(boxMessage,
                            boxCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);

        }

        static void StripNoDistill(String postscriptFile)
        {
            if (Properties.Settings.Default.StripNoRedistill)
            {
                String strippedFile = Path.GetTempFileName();

                using (StreamReader inputReader = new StreamReader(File.OpenRead(postscriptFile), System.Text.Encoding.UTF8))
                using (StreamWriter strippedWriter = new StreamWriter(File.OpenWrite(strippedFile), new UTF8Encoding(false)))
                {
                    NoDistillStripping strippingStatus = NoDistillStripping.Searching;
                    String inputLine;
                    while (!inputReader.EndOfStream)
                    {
                        inputLine = inputReader.ReadLine();
                        if (inputLine != null)
                        {
                            switch ((int)strippingStatus)
                            {
                                case (int)NoDistillStripping.Searching:
                                    if (inputLine == "%ADOBeginClientInjection: DocumentSetup Start \"No Re-Distill\"")
                                        strippingStatus= NoDistillStripping.Removing;
                                    else
                                        strippedWriter.WriteLine(inputLine);
                                    break;
                                case (int)NoDistillStripping.Removing:
                                    if (inputLine == "%ADOEndClientInjection: DocumentSetup Start \"No Re-Distill\"")
                                        strippingStatus = NoDistillStripping.Complete;
                                        break;
                                case (int)NoDistillStripping.Complete:
                                    strippedWriter.WriteLine(inputLine);
                                    break;
                            }
                        }
                    }
                    strippedWriter.Close();
                    inputReader.Close();
                }

                File.Delete(postscriptFile);
                File.Move(strippedFile, postscriptFile);
            }

        }

    }
}
