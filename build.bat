@echo off
echo M3U8 Downloader - Build
echo ========================
dotnet publish M3U8Downloader/M3U8Downloader.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained false ^
  -p:PublishSingleFile=true ^
  -o ./dist
echo.
if exist dist\M3U8Downloader.exe (
    echo  Tayyor: dist\M3U8Downloader.exe
) else (
    echo  XATO: build muvaffaqiyatsiz
)
pause
