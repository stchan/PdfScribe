if EXIST "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" (
    call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
    echo on
    msbuild PdfScribe.sln  -t:Rebuild -p:Configuration=Release -r
    if NOT EXIST "publish\signed" mkdir "publish\signed"
    del /q "publish\signed\*"
    copy /y  "publish\unsigned\PdfScribeInstall_*.msi" "publish\signed\"
    cd "publish\signed\"
    signtool sign /sha1 %CodeSignHash% /t http://time.certum.pl /fd sha256 /v PdfScribeInstall_*.msi
) ELSE (
    echo Could not set build tools environment.
    exit 1
)