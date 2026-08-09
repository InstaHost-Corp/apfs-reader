param(
    [ValidateSet("x86", "x64")]
    [string]$Architecture = "x64",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$msbuild = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
if ($Architecture -eq "x64") {
    $msbuild = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
}
if (-not (Test-Path $msbuild)) {
    throw ".NET Framework 4 build tools were not found."
}

& $msbuild "src\APFSReader\APFSReader.csproj" /m `
    "/p:Configuration=$Configuration" "/p:Platform=$Architecture" /v:minimal
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

