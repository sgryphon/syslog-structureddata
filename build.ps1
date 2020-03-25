#!/usr/bin/env pwsh
Write-Host 'Building Nuget Packages for Syslog.StructuredData'

dotnet tool restore

Write-Host "Getting GitVersion"
$json = (dotnet tool run dotnet-gitversion /output json)
#Write-Host $json
$v = ($json | ConvertFrom-Json)

Write-Host "Building version $($v.SemVer)+$($v.ShortSha) (Nuget $($v.NuGetVersion))"

dotnet test (Join-Path $PSScriptRoot Syslog.StructuredData.sln)
if (!$?) { throw 'Tests failed' }

dotnet pack (Join-Path $PSScriptRoot 'src/Syslog.StructuredData') -c Release -p:AssemblyVersion=$($v.AssemblySemVer) -p:FileVersion=$($v.AssemblySemFileVer) -p:Version=$($v.SemVer)+$($v.ShortSha) -p:PackageVersion=$($v.NuGetVersion) --output pack
