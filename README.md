# PdfScribe v1.0.7

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

PdfScribe is AGPL.


## Configuration
 
In the application config file (PdfScribe.exe.config), there are the following settings in the "applicationSettings" element:

* ****AskUserForOutputFilename**** - set value to *true* if you want PdfScribe to ask the user where to save the PDF.
* ****OutputFile**** - if there is a constant filename you want the PDF to be saved to, set its value here. Environment variables can be used. PdfScribe will overwrite each time. This setting is ignored if  **AskUserForOutputFilename** is set to *true*. Note that a literal % character cannot be used even though it is legal in a Windows filename.
* ****OpenAfterCreating**** - set value to *true* if you want the PDF automatically opened with the default viewer. This setting is ignored if the file extension is not .PDF


## Release notes
#### v1.07
* Fix for issue #13 (Couldn't use environment variable in output filename).

#### v1.0.6
* Added option to automatically open PDF in the default viewer (enhancement request from issue #4).

#### v1.0.5
* Installer package now properly removes older versions during a major upgrade. If you are upgrading from v1.0.4 or older, manually remove the old version first - the UninstallPrinter custom action is never called during an upgrade.

## To do

* Allow auto-generated filenames with sequence numbers if the user doesn't want to overwrite (ex: OUTPUT-001.PDF, OUTPUT-002.PDF, etc)
* Allow file appending if **OutputFile** setting is used.
* GUI for configuration
* Watermarking output

