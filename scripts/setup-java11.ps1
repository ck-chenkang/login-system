param(
    [string]$JdkPath
)

function Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# Check elevation
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Warning "Please run PowerShell as Administrator to set machine-level environment variables."
}

if (-not $JdkPath) {
    Step "Detecting JDK 11 installation..."
    $candidates = @()
    $searchRoots = @(
        "C:\\Program Files\\Eclipse Adoptium",
        "C:\\Program Files\\Microsoft"
    )
    foreach ($root in $searchRoots) {
        if (Test-Path $root) {
            $dirs = Get-ChildItem -Path $root -Directory -Recurse -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -match '^jdk-11' } |
                Select-Object -ExpandProperty FullName
            if ($dirs) { $candidates += $dirs }
        }
    }

    if ($candidates.Count -eq 0) {
        Write-Error "No JDK 11 installation detected. Install one first, e.g.: winget install -e --id EclipseAdoptium.Temurin.11.JDK"
        exit 1
    }

    # Pick the highest 'jdk-11.*' by lexical sort of the suffix after 'jdk-'
    $JdkPath = $candidates | Sort-Object { $_ -replace '.*jdk-','' } -Descending | Select-Object -First 1
}

if (-not (Test-Path $JdkPath)) { Write-Error "Provided JdkPath not found: $JdkPath"; exit 1 }
$bin = Join-Path $JdkPath 'bin'
if (-not (Test-Path $bin)) { Write-Error "bin directory not found under: $JdkPath"; exit 1 }

Step "Setting JAVA_HOME to $JdkPath"
try {
    [Environment]::SetEnvironmentVariable("JAVA_HOME", $JdkPath, "Machine")
} catch {
    Write-Error "Failed to set JAVA_HOME at Machine scope. Run as Administrator. $_"
    exit 1
}

$machinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
$entries = $machinePath -split ';' | Where-Object { $_ -ne '' }
$hasJavaHomeBin = $entries -contains "%JAVA_HOME%\bin"
$hasResolvedBin = $entries -contains $bin

if (-not $hasJavaHomeBin -and -not $hasResolvedBin) {
    Step "Appending %JAVA_HOME%\\bin to PATH (Machine)"
    $newPath = $machinePath.TrimEnd(';') + ";%JAVA_HOME%\\bin"
    try {
        [Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
    } catch {
        Write-Error "Failed to update PATH at Machine scope. Run as Administrator. $_"
        exit 1
    }
} else {
    Step "PATH already contains JDK bin"
}

Write-Host "`nDone. Close and reopen terminals (or sign out/in) for changes to take effect." -ForegroundColor Green
Write-Host "Verify with:" -ForegroundColor Yellow
Write-Host "  java -version"
Write-Host "  javac -version"
Write-Host "  echo %JAVA_HOME%   (cmd)  or   $env:JAVA_HOME (PowerShell)"

