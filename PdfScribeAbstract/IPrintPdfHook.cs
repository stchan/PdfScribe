namespace PdfScribe
{
    public interface IPrintPdfHook
    {
        /// <summary>
        /// Implement this method to get pdf file store location
        /// </summary>
        /// <param name="outputFilename"></param>
        /// <returns></returns>
        bool GetPdfOutputFilename(ref string outputFilename);

        /// <summary>
        /// Implement this method to do custom action for printed pdf file.
        /// </summary>
        /// <param name="outputFilename"></param>
        void OnPdfPrinted(string outputFilename);
    }
}