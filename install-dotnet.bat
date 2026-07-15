@echo off
chcp 866 >nul
echo ============================================
echo Установка .NET 6.0 SDK для Mail Client App
echo ============================================
echo.

:: Проверяем, установлен ли уже .NET 6.0
where dotnet >nul 2>&1
if %errorlevel% equ 0 (
    echo .NET SDK уже установлен!
    dotnet --version
    echo.
    goto RUN_APP
)

echo .NET SDK не найден. Устанавливаем...
echo.

:: Скачиваем и устанавливаем .NET 6.0 SDK
set DOTNET_URL=https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-6.0.400-windows-x64-installer
set INSTALLER=dotnet-sdk-6.0.400-win-x64.exe

if not exist "%TEMP%\%INSTALLER%" (
    echo Скачиваем .NET 6.0 SDK...
    powershell -Command "(New-Object Net.WebClient).DownloadFile('%DOTNET_URL%', '%TEMP%\%INSTALLER%')"
)

echo Запускаем установщик...
start /wait "" "%TEMP%\%INSTALLER%" /quiet /norestart

echo.
echo ============================================
echo Установка завершена!
echo ============================================
echo.

:RUN_APP
cd /d "%~dp0MailClientApp"
echo.
echo Переходим в папку MailClientApp...
echo.
echo Запускаем приложение...
echo.
dotnet run

pause