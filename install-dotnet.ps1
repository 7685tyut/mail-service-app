# Установка .NET 6.0 SDK для Mail Client App
# Запустите этот скрипт из PowerShell от имени администратора

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Установка .NET 6.0 SDK для Mail Client App" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Проверяем, установлен ли .NET 6.0
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK уже установлен! Версия: $dotnetVersion" -ForegroundColor Green
    Write-Host ""
    
    # Переходим в папку приложения и запускаем
    Set-Location -Path "$PSScriptRoot\MailClientApp"
    Write-Host "Запускаем приложение..." -ForegroundColor Yellow
    dotnet run
    exit
}
catch {
    Write-Host ".NET SDK не найден. Устанавливаем..." -ForegroundColor Yellow
    Write-Host ""
}

# URL для скачивания .NET 6.0 SDK
$dotnetUrl = "https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-6.0.400-windows-x64-installer"
$installerPath = "$env:TEMP\dotnet-sdk-6.0.400-win-x64.exe"

# Скачиваем установщик
Write-Host "Скачиваем .NET 6.0 SDK..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri $dotnetUrl -OutFile $installerPath
    Write-Host "Установщик скачан успешно!" -ForegroundColor Green
}
catch {
    Write-Host "Ошибка скачивания: $_" -ForegroundColor Red
    Write-Host "Попробуйте скачать вручную с сайта: https://dotnet.microsoft.com/download/dotnet/6.0" -ForegroundColor Yellow
    exit
}

# Запускаем установщик
Write-Host "Запускаем установщик..." -ForegroundColor Yellow
Start-Process -FilePath $installerPath -ArgumentList "/quiet", "/norestart" -Wait

Write-Host "" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Установка завершена!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Переходим в папку приложения
Set-Location -Path "$PSScriptRoot\MailClientApp"
Write-Host "Запускаем приложение..." -ForegroundColor Yellow
Write-Host ""

# Запускаем приложение
dotnet run

Write-Host "" -ForegroundColor Cyan
Write-Host "Готово! Приложение должно запуститься." -ForegroundColor Green