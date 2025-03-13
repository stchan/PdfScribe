## Change log
#### v1.3.2
* Set GS AutoRotatePages to /None (issue #32)
#### v1.3.0
* Added **StripNoRedistill** option (fix for issue #33).
#### v1.1.5
* Maintenance release, no new functionality. Link to Ghostscript 10.0, locally compiled redmon64pdfscribe.dll.
#### v1.1.3
* Set output filename dialog box - PDF filename defaults to redmon document name. Inspired by changes @rainmakerho made (Pull request [#36](https://github.com/stchan/PdfScribe/pull/36))
#### v1.1.2
* Complete fix for issue #26 by @Zhuangkh (Pull request [#40](https://github.com/stchan/PdfScribe/pull/40))
* .NET 4.8 required
#### v1.1.0
* Redmon modification for issue #17 by @mca0815
#### v1.0.9
* Redmon modification for issue #17 by @tahoop (Dialog box for filename not appearing).
#### v1.0.8
* Partial fix for issue #26 (bug with filename containing unicode/utf8 letters)
#### v1.0.7
* Fix for issue #13 (Couldn't use environment variable in output filename).
* Now defaults to use the print spooler rather than direct printing. (issue #14)

#### v1.0.6
* Added option to automatically open PDF in the default viewer (enhancement request from issue #4).

#### v1.0.5
* Installer package now properly removes older versions during a major upgrade. If you are upgrading from v1.0.4 or older, manually remove the old version first - the UninstallPrinter custom action is never called during an upgrade.
