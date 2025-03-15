echo off
if EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" (
    call "%ProgramFiles%\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
    echo on
    msbuild PdfScribe.sln  -t:Rebuild -p:Configuration=Release -r
    if NOT EXIST "publish\signed" mkdir "publish\signed"
    del /q "publish\signed\*"
    copy /y  "publish\unsigned\PdfScribe*" "publish\signed\"
    cd "publish\signed\"
    signtool sign /sha1 %CodeSignHash% /t http://time.certum.pl /fd sha256 /v PdfScribe*.msi
) ELSE (
    echo Could not set build tools environment.
    exit 1
)