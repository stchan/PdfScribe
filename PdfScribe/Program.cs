using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace PdfScribe
{
    public class Program
    {


        #region Message constants
        
        const string errorDialogCaption = "PDF Scribe"; // Error taskdialog caption text
        
        const string errorDialogInstructionPDFGeneration = "There was a PDF generation error.";
        const string errorDialogInstructionCouldNotWrite = "Could not create the output file.";
        const string errorDialogInstructionUnexpectedError = "There was an internal error. Enable tracing for details.";

        const string errorDialogTextFileInUse = "{0} is being used by another process.";
        const string errorDialogTextGhostScriptConversion = "Ghostscript error code {0}.";

        const string warnFileNotDeleted = "{0} could not be deleted.";

        #endregion

        #region Other constants
        const string traceSourceName = "PdfScribe";

        const string defaultOutputFilename = "PDFSCRIBE.PDF";

        #endregion

        static TraceSource logEventSource = new TraceSource(traceSourceName);

        [STAThread]
        static void Main(string[] args)
        {
            // Install the global exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);


            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = String.Empty;

            try
            {
                using (BinaryReader standardInputReader = new BinaryReader(Console.OpenStandardInput()))
                {
                    using (FileStream standardInputFile = new FileStream(standardInputFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        standardInputReader.BaseStream.CopyTo(standardInputFile);
                    }
                }

                if (GetPdfOutputFilename(ref outputFilename))
                {
                    // Remove the existing PDF file if present
                    File.Delete(outputFilename);
                    // Only set absolute minimum parameters, let the postscript input
                    // dictate as much as possible
                    String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                                String.Format("-sOutputFile={0}", outputFilename), standardInputFilename };

                    GhostScript64.CallAPI(ghostScriptArguments);
                    DisplayPdf(outputFilename);
                }
            }
            catch (IOException ioEx)
            {
                // We couldn't delete, or create a file
                // because it was in use
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error,
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + ioEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionCouldNotWrite + Environment.NewLine +
                                    String.Format("{0} is in use.", outputFilename));
            }
            catch (UnauthorizedAccessException unauthorizedEx)
            {
                // Couldn't delete a file
                // because it was set to readonly
                // or couldn't create a file
                // because of permissions issues
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error, 
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + unauthorizedEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionCouldNotWrite + Environment.NewLine +
                                    String.Format("Insufficient privileges to either create or delete {0}", outputFilename));


            }
            catch (ExternalException ghostscriptEx)
            {
                // Ghostscript error
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error, 
                                          String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()) +
                                          Environment.NewLine +
                                          "Exception message: " + ghostscriptEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionPDFGeneration + Environment.NewLine +
                                    String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()));

            }
            finally
            {
                try
                {
                    File.Delete(standardInputFilename);
                }
                catch 
                {
                    logEventSource.TraceEvent(TraceEventType.Warning,
                                              (int)TraceEventType.Warning,
                                              String.Format(warnFileNotDeleted, standardInputFilename));
                }
                logEventSource.Flush();
            }
        }

        /// <summary>
        /// All unhandled exceptions will bubble their way up here -
        /// a final error dialog will be displayed before the crash and burn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logEventSource.TraceEvent(TraceEventType.Critical,
                                      (int)TraceEventType.Critical,
                                      ((Exception)e.ExceptionObject).Message + Environment.NewLine +
                                                                        ((Exception)e.ExceptionObject).StackTrace);
            DisplayErrorMessage(errorDialogCaption,
                                errorDialogInstructionUnexpectedError);
        }

        static bool GetPdfOutputFilename(ref String outputFile)
        {
            bool filenameRetrieved = false;
            switch (Properties.Settings.Default.AskUserForOutputFilename)
            {
                case (true) :
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
                    outputFile = GetOutputFilename();
                    filenameRetrieved = true;
                    break;
            }
            return filenameRetrieved;

        }

        private static String GetOutputFilename()
        {

            String outputFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), defaultOutputFilename);
            if (!String.IsNullOrEmpty(Properties.Settings.Default.OutputFile) &&
                !String.IsNullOrWhiteSpace(Properties.Settings.Default.OutputFile))
            {
                if (IsFilePathValid(Properties.Settings.Default.OutputFile))
                {
                    outputFilename = Properties.Settings.Default.OutputFile;
                }
                else
                {
                    if (IsFilePathValid(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.OutputFile)))
                    {
                        outputFilename = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.OutputFile);
                    }
                }
            }
            else
            {
                logEventSource.TraceEvent(TraceEventType.Warning,
                                          (int)TraceEventType.Warning,
                                          String.Format("Using default output filename {0}",
                                                        outputFilename));
            }
            return outputFilename;
        }

        static bool IsFilePathValid(String filePath)
        {
            bool pathIsValid = false;

            if (!String.IsNullOrEmpty(filePath) && filePath.Length <= 260)
            {
                String directoryName = Path.GetDirectoryName(filePath);
                String filename = Path.GetFileName(filePath);

                if (Directory.Exists(directoryName))
                {
                    // Check for invalid filename chars
                    Regex containsABadCharacter = new Regex("["
                                                    + Regex.Escape(new String(System.IO.Path.GetInvalidPathChars())) + "]");
                    pathIsValid = !containsABadCharacter.IsMatch(filename);
                }
            }
            else
            {
                logEventSource.TraceEvent(TraceEventType.Warning,
                                          (int)TraceEventType.Warning,
                                          "Output filename is longer than 260 characters, or blank.");
            }
            return pathIsValid;
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
    }
}
