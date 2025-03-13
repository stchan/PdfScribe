# PdfScribe v1.3.2

PdfScribe is a PDF virtual printer. Check the [releases](https://github.com/stchan/PdfScribe/releases) page to download a prebuilt MSI package.

## System Requirements

* 64-bit Windows
* .NET Framework 4.8 or later

## Building from source

Visual Studio 2022 is required to build PdfScribe.

PdfScribe links to, and distributes the following third party components:

* Microsoft Postscript Printer Driver (V3)
* Ghostscript (64-bit)
* Redmon 1.9 (64-bit)

## License

PdfScribe is AGPLv3+.


## Configuration
 
In the application config file (PdfScribe.exe.config), there are the following settings in the "applicationSettings" element:

* ****AskUserForOutputFilename**** - set value to *true* if you want PdfScribe to ask the user where to save the PDF.
* ****OutputFile**** - if there is a constant filename you want the PDF to be saved to, set its value here. Environment variables can be used. PdfScribe will overwrite each time. This setting is ignored if  **AskUserForOutputFilename** is set to *true*. Note that a literal % character cannot be used even though it is legal in a Windows filename.
* ****OpenAfterCreating**** - set value to *true* if you want the PDF automatically opened with the default viewer. This setting is ignored if the file extension is not .PDF
* ****StripNoRedistill**** - set to *true* if you want PdfScribe to remove any postscript that prevents printing of secured PDFs (see issues #25,#33 for details). A separate pass of the postscript output needs to be made, so only set to *true* if needed.

## Known Issues
* Check repo [issues](https://github.com/stchan/PdfScribe/issues) for the latest.

## Release notes
#### v1.4.0
* Wix (installer) upgrade, Ghostscript 10.5
