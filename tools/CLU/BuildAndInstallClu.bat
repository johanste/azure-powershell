echo off
setlocal
set root=%~dp0..\..
where dotnet.exe
if ERRORLEVEL 1 (
    echo Please install 'dotnet', say from 'https://azureclu.blob.core.windows.net/tools/dotnet-win-x64.latest.zip', unzip, then add its bin folder to the PATH
    exit /B 1
)

pushd
cd %root%\src\CLU
call dotnet restore -s https://api.nuget.org/v3/index.json -s "%root%\tools\LocalFeed"
if ERRORLEVEL 1 (
    echo "dnu.cmd restore" failed under folder of "%root%\src\CLU"
    popd
    exit /B 1
)
popd

@powershell -file %~dp0\BuildDrop.ps1

REM cook a msclu.cfg with a correct local repro path. 
set mscluCfg=%root%\drop\clurun\win7-x64\msclu.cfg
if not exist %mscluCfg% (
    copy /Y %root%\src\CLU\clurun\msclu.cfg %root%\drop\clurun\win7-x64
)
echo ^(Get-Content "%mscluCfg%"^) ^| ForEach-Object { $_ -replace "TOFILL", "%root%\drop\CommandRepo" } ^| Set-Content "%mscluCfg%"^ >"%temp%\Rep.ps1"
@powershell -file %temp%\Rep.ps1

for %%i IN ("win7-x64" "ubuntu.14.04-x64" "osx.10.10-x64") DO (
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.CLU.Commands.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Profile.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Resources.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Resources.Cmdlets.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Websites.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Network.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Management.Storage.%%i
	%root%\drop\clurun\win7-x64\clurun.exe --install Microsoft.Azure.Commands.Compute.%%i
)

REM Replace *.exe with static copy for non-Windows drops
echo param([string] $runtime) > %temp%\FixNonWindowsExes.ps1
echo Get-ChildItem %root%\drop\clurun\$runtime -Recurse -Include *.exe ^| %%{ Copy-Item -Path "%root%\tools\CLU\$runtime\coreconsole" -Destination "$($_.DirectoryName)\$($_.BaseName)" -Force } >> %temp%\FixNonWindowsExes.ps1
@powershell -file %temp%\FixNonWindowsExes.ps1 -runtime ubuntu.14.04-x64 
@powershell -file %temp%\FixNonWindowsExes.ps1 -runtime osx.10.10-x64
del %root%\drop\clurun\osx.10.10-x64\*.exe /s
del %root%\drop\clurun\ubuntu.14.04-x64\*.exe /s

rename %root%\drop\clurun\win7-x64\azure.bat az.bat

REM setup osx and linux bits which can be xcopied and run. 
REM note, for known nuget bugs, skip --install by copying over cmdlet packages.
copy /Y %root%\drop\clurun\win7-x64\msclu.cfg %root%\drop\clurun\osx.10.10-x64

REM: copy over the pre-cooked az.sh and ensure correct line endings
copy /Y %~dp0\az.sh %root%\drop\clurun\osx.10.10-x64\az
set azuresh=%root%\drop\clurun\osx.10.10-x64\az
echo Get-ChildItem %azuresh% ^| ForEach-Object { >  %temp%\fixLineEndings.ps1
echo $contents = [IO.File]::ReadAllText($_) -replace "`r`n?", "`n" >> %temp%\fixLineEndings.ps1 
echo [IO.File]::WriteAllText($_, $contents) >> %temp%\fixLineEndings.ps1 
echo } >> %temp%\fixLineEndings.ps1
@powershell -file %temp%\fixLineEndings.ps1

copy /Y %root%\drop\clurun\win7-x64\msclu.cfg %root%\drop\clurun\ubuntu.14.04-x64
copy /Y %azuresh% %root%\drop\clurun\ubuntu.14.04-x64\az

REM, windows version also needs it for bash based testing
copy /Y %~dp0\az.win.sh %root%\drop\clurun\win7-x64\az
