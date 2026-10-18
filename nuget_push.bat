@echo off
rem Thin launcher kept for double-click and habit; the real script is nuget_push.ps1 (PowerShell 7).
rem Set the Signotaur key and server first:  set signkey=...  set signserver=https://host:443
rem (or $env:signkey / $env:signserver in PowerShell)
pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0nuget_push.ps1" %*
exit /b %ERRORLEVEL%
