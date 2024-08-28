if EXIST "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" (
    call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
    echo on
    msbuild PdfScribe.sln  -t:Rebuild -p:Configuration=Release -r
) ELSE (
    echo Could not set build tools environment.
    exit 1
)