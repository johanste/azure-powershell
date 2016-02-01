param([string]$cmdletsDir, [string]$packageId, [string]$packageSource, [string]$packageVersion, [string]$outputDir, [string]$runtimeType)

Set-StrictMode -Version 3

if (!(Test-Path $cmdletsDir))
{
    throw "cmdletsDir: '$cmdletsDir' must be an existing directory containing cmdlet code"
}
if (!(Test-Path $outputDir))
{
    throw "outputDir: '$outputDir' must be an existing directory"
}

if ([string]::IsNullOrWhiteSpace($env:WORKSPACE) -or !(Test-Path $env:WORKSPACE))
{
    throw "env:WORKSPACE: '$env:WORKSPACE' must be an existing directory"
}

$packageSource = $packageSource.TrimEnd('\\')
Write-Host "using package id: $packageId, package source: $packageSource, packageVersion: $packageVersion"
$buildDir = [io.path]::combine($cmdletsDir, "bin", "build", $runtimeType)
New-Item $buildDir -ItemType Directory -Force
cd $buildDir
dotnet publish $cmdletsDir -f dnxcore50 -r $runtimeType -o $packageSource

if (Test-Path $cmdletsDir\content)
{
    Copy-Item -Path $cmdletsDir\content -Destination $packageSource\content -Recurse -Force
}
Copy-Item -Path $cmdletsDir\*xml -Destination $packageSource\content -Force

$nuSpecTemplate = (Get-ChildItem ([System.IO.Path]::Combine($cmdletsDir, ($packageId + ".nuspec.template"))))
$nuSpecOutput = [System.IO.Path]::Combine($packageSource, ($packageId + ".$runtimeType.nuspec"))
Write-Host "Creating dynamic nuspec package in: $nuSpecOutput"

$fileContent = Get-Content $nuSpecTemplate
$files = (Get-ChildItem $packageSource | Where -FilterScript { !$_.Name.Contains("nuspec") -and !$_.PSIsContainer } | Select-Object -Property Name)
$refFiles = $files | Where -FilterScript { $_.Name.EndsWith(".dll") } 
$additionalFiles = $files | Where -FilterScript { !$_.Name.EndsWith(".pdb") -and !$_.Name.Equals("coreconsole") -and !$_.Name.Equals("content") } # Nuget Bug: files without extensions can't be added to the package
$refFileText = ""
$refFiles | %{$refFileText +=  ("        <reference file=""" + $_.Name + """/>`r`n")}
$contentFileText = ""
if ($packageId -ne "Microsoft.CLU.Commands") 
{
    if (Test-Path "$packageSource\content\help") 
    {    
        $contentFileText += "    <file src=""content\*xml"" target=""content""/>`r`n"
    
        $contentFileText += "    <file src=""content\help\*.hlp"" target=""content\help""/>`r`n"
    }
}
$sourceFileText = ""
$refFiles | %{$sourceFileText += ("    <file src=""" + $_.Name + """ target=""lib\dnxcore50""/>`r`n")}
$additionalFiles | %{$sourceFileText += ("    <file src=""" + $_.Name + """ target=""lib\dnxcore50""/>`r`n")}
$outputContent = $fileContent -replace "%PackageVersion%", $packageVersion 
$outputContent = $outputContent -replace "%Runtime%", $runtimeType
$outputContent = $outputContent -replace "%ReferenceFiles%", $refFileText
$outputContent = $outputContent -replace "%SourceFiles%", $sourceFileText 
$outputContent = $outputContent -replace "%ContentFiles%", $contentFileText
Set-Content -Value $outputContent -Path $nuspecOutput

Write-Host "Creating nuget package..."
cmd /c "$env:WORKSPACE\tools\Nuget.exe pack $nuspecOutput -OutputDirectory $outputDir"
Pop-Location 
