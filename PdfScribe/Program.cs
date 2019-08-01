using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace PdfScribe
{
    public class Program
    {
        #region Message constants

        const string errorDialogCaption = "PDF Scribe"; // Error taskdialog caption text

        const string errorDialogInstructionPDFGeneration = "There was a PDF generation error.";
        const string errorDialogInstructionCouldNotWrite = "Could not create the output file.";
        const string errorDialogInstructionUnexpectedError = "There was an internal error. Enable tracing for details.";

        const string errorDialogTextGhostScriptConversion = "Ghostscript error code {0}.";

        const string warnFileNotDeleted = "{0} could not be deleted.";

        #endregion

        #region Other constants
        const string traceSourceName = "PdfScribe";

        #endregion

        static readonly TraceSource logEventSource = new TraceSource(traceSourceName);

        [STAThread]
        static void Main(string[] args)
        {
            IPrintPdfHook printPdfHook;

            try
            {
                var printPdfHookImpl = Properties.Settings.Default.PrintPdfHookImpl.Split(',');
                var objectHandle = Activator.CreateInstance(AppDomain.CurrentDomain, $"{printPdfHookImpl[0]}", printPdfHookImpl[1]);

                printPdfHook = objectHandle.Unwrap() as IPrintPdfHook ?? new DefaultPrintPdfHook();
            }
            catch (Exception e)
            {
                logEventSource.TraceEvent(TraceEventType.Error,
                    (int)TraceEventType.Error,
                    errorDialogInstructionCouldNotWrite +
                    Environment.NewLine +
                    "Exception message: " + e.Message);
                printPdfHook = new DefaultPrintPdfHook();
            }

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

                if (printPdfHook.GetPdfOutputFilename(ref outputFilename))
                {
                    // Remove the existing PDF file if present
                    File.Delete(outputFilename);
                    // Only set absolute minimum parameters, let the postscript input
                    // dictate as much as possible
                    String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                                String.Format("-sOutputFile={0}", outputFilename), standardInputFilename,
                                                "-c", @"[/Creator(PdfScribe 1.0.7 (PSCRIPT5)) /DOCINFO pdfmark", "-f"};

                    GhostScript64.CallAPI(ghostScriptArguments);
                    printPdfHook.OnPdfPrinted(outputFilename);
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
                MessageHelper.DisplayErrorMessage(errorDialogCaption,
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
                MessageHelper.DisplayErrorMessage(errorDialogCaption,
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
                MessageHelper.DisplayErrorMessage(errorDialogCaption,
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
            MessageHelper.DisplayErrorMessage(errorDialogCaption,
                                errorDialogInstructionUnexpectedError);
        }
    }
}
