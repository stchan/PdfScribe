# PdfScribe v1.0.6

PdfScribe is a PDF virtual printer. Check the [releases](https://github.com/stchan/PdfScribe/releases) page for this project to download a prebuilt MSI package.

## System Requirements

* 64-bit Windows Vista or later
* .NET Framework 4.0 or later

## Building from source

Visual Studio 2017, Wix 3.11, and Votive 2017 are required to build PdfScribe.

PdfScribe links to, and distributes the following third party components:

* Microsoft Postscript Printer Driver (V3)
* Ghostscript (64-bit)
* Redmon 1.9 (64-bit)

## License

Ghostscript is AGPL (more restrictive than GPLv3), so PdfScribe is bound by that license.



## Configuration
 
In the application config file (PdfScribe.exe.config), there are the following settings in the "applicationSettings" element:

* ****AskUserForOutputFilename**** - set value to *true* if you want PdfScribe to ask the user where to save the PDF.
* ****OutputFile**** - if there is a constant filename you want the PDF to be saved to, set its value here. Environment variables can be used. PdfScribe will overwrite each time. This setting is ignored if  **AskUserForOutputFilename** is set to *true*. 
* ****OpenAfterCreating**** - set value to *true* if you want the PDF automatically opened with the default viewer. This setting is ignored if the file extension is not .PDF

## To do

* Allow auto-generated filenames with sequence numbers if the user doesn't want to overwrite (ex: OUTPUT-001.PDF, OUTPUT-002.PDF, etc)
* Allow file appending if **OutputFile** setting is used.
* GUI for configuration
* Allow selection of page sizes other than default
* Watermarking output

 


