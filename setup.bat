@echo off
set "INSTALL_DIR=%APPDATA%\OpenFolderProtocol"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

:: open-folder.js をダウンロード
powershell -NoProfile -Command "(New-Object System.Net.WebClient).DownloadFile('https://proton-wind.github.io/folder-widget/open-folder.js', '%INSTALL_DIR%\open-folder.js')"

:: レジストリ登録
reg add "HKCU\Software\Classes\open-folder" /ve /t REG_SZ /d "URL:Open Folder Protocol" /f >nul
reg add "HKCU\Software\Classes\open-folder" /v "URL Protocol" /t REG_SZ /d "" /f >nul
reg add "HKCU\Software\Classes\open-folder\shell\open\command" /ve /t REG_SZ /d "wscript.exe \"%INSTALL_DIR%\open-folder.js\" \"%%1\"" /f >nul

echo ========================================================
echo Setup completed successfully!
echo ========================================================
pause