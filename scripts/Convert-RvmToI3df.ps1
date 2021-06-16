#Requires -Version 7
[CmdletBinding()]
param (
    [string] $InputDirectory = $(Join-Path "$PSScriptRoot" ".." "TestData" "HDA_RVM"),
    [string] $WorkDirectory = $(Join-Path "$PSScriptRoot" ".\outputs\"),
    [Parameter(Mandatory = $false)][long] $ProjectId = 10000,
    [Parameter(Mandatory = $false)][long] $ModelId = 1,
    [Parameter(Mandatory = $false)][long] $RevisionId = 0,
    [string] $ArtifactDirectory = "C:\Users\nhals\GitRepos\Echo3DWeb\EchoReflectApi\EchoReflect.Api\AppData\demomodel",
    [string] $I3dfPath = "C:\Users\nhals\GitRepos\conceal\i3df",
    [switch] $Force = $true,
    [switch] $UploadToDev = $false
)

begin {
}

process {
}

end {

    #region PsSetup
    Set-StrictMode -Version 3
    [Console]::ResetColor()
    $ErrorActionPreference = 'Stop'
    #endregion PsSetup


    #region Guards
    if (-not (Get-Command "dotnet" -ErrorAction 'SilentlyContinue')) {
        Write-Error "Could not find dotnet. Is the Dotnet CLI installed?"
    }

    if (-not (Get-Command "cargo" -ErrorAction 'SilentlyContinue')) {
        Write-Error "Could not find cargo. Do you have Rust and Cargo installed?"
    }

    if (-not (Get-Command "ctmconv" -ErrorAction 'SilentlyContinue')) {
        Write-Error "Could not find ctmconv. You need to install OpenCTM, and restart the terminal. http://openctm.sourceforge.net/?page=download"
    }

    if (-not $Force -and (Get-ChildItem $WorkDirectory)) {
        Write-Error "The output directory is not empty. Consider using the ""-Force"" argument if this is expected."
    }
    #endregion Guards


    #region Reveal Composer
    $cadRevealComposerPath = Join-Path "$PSScriptRoot" ".." "CadRevealComposer.exe" "CadRevealComposer.exe.csproj"
    & dotnet.exe run --configuration Release --project $cadRevealComposerPath -- --InputDirectory $InputDirectory --OutputDirectory $WorkDirectory --ProjectId $ProjectId --ModelId $ModelId --RevisionId $RevisionId
    if ($LASTEXITCODE) {
        Write-Error "Dotnet failed with exit code $LASTEXITCODE"
    }

    #endregion Reveal Composer


    #region i3df-converter
    $expectedOutputPath = Join-Path (Resolve-Path $WorkDirectory) "output.json"
    if (-not (Test-Path $expectedOutputPath)) {
        Write-Error "Expected the output from dotnet to be at ""$expectedOutputPath"". Could not find the file. Has anything changed?"
    }

    $outputi3dFile = (Join-Path (Resolve-Path $WorkDirectory) "sector_0.i3d")

    $i3dfCargoPath = Join-Path $I3dfPath "Cargo.toml"

    & cargo.exe run --manifest-path $i3dfCargoPath --release --features dump --bin i3df-converter -- $expectedOutputPath $outputi3dFile
    if ($LASTEXITCODE) {
        Write-Error "cargo failed with exit code $LASTEXITCODE"
    }

    #endregion i3df-converter

    #region ctm-converter

    $CtmConverterPath = Join-Path $PSScriptRoot ".." "tools" "mesh2ctm.exe"
    Get-ChildItem -Path "$WorkDirectory/*" -Filter "*.obj" | ForEach-Object {
        $ctmInputPath = $_.FullName

        $ctmFileName = $_.BaseName + ".ctm"
        $ctmOutputPath = Join-Path $WorkDirectory $ctmFileName

        $compressionLevel = 4
        & $CtmConverterPath $ctmInputPath $ctmOutputPath --comment "Echo was here." --method MG1 --level $compressionLevel --no-texcoords --no-colors --upaxis Y
        if ($LASTEXITCODE) {
            Write-Error "ctmconv failed." -ErrorAction Stop
        }
    }

    #endregion ctm-converter

    #region Artifact Staging

    $artifactStagingDirectory = Join-Path $WorkDirectory "ArtifactStaging"
    If (-not (Test-Path $artifactStagingDirectory)) {
        New-Item -Path $artifactStagingDirectory -ItemType Directory -Force
    }

    $artifactStagingGlobs = @(
        "scene.json"
        "cadnodeinfo.json"
        "*.i3d"
        "*.f3d"
        "*.ctm"
    )

    foreach ($filter in $artifactStagingGlobs) {
        Copy-Item -Path (Join-Path $WorkDirectory $filter) -Destination $artifactStagingDirectory
    }

    #endregion Artifact Staging

    Copy-Item -Path (Join-Path $artifactStagingDirectory "*") -Destination $ArtifactDirectory

    if ($UploadToDev) {
        if (-not (Get-Command "az" -ErrorAction 'SilentlyContinue')) {
            Write-Error "Could not find az. Do you have Azure Cli installed?"
        }

        $destination = "hda/demomodel/reveal/"

        az storage azcopy blob upload `
            --container 'models' `
            --account-name 'stechoreflectapidev' `
            --source "$artifactStagingDirectory/*" `
            --destination $destination
    }

    Write-Host "Success. Output copied to ""$ArtifactDirectory"""
}

