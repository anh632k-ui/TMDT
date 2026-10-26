param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = "."
)

$ErrorActionPreference = "Stop"

# Use ASCII-only script text to support Windows PowerShell 5.1 without BOM.
$location = (Resolve-Path -LiteralPath $RepoRoot).Path
$repoPath = & git -C $location rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoPath)) {
    throw "Not a Git repository: $location"
}
$repo = [string]$repoPath.Trim()
$app = Join-Path $repo "VanPhongPham"
$project = Join-Path $app "VanPhongPham.csproj"
if (!(Test-Path -LiteralPath $project -PathType Leaf)) {
    throw "Cannot find VanPhongPham/VanPhongPham.csproj under repository root: $repo"
}

$branch = & git -C $repo branch --show-current
if ($LASTEXITCODE -ne 0) { throw "Could not determine Git branch." }
$branch = [string]$branch.Trim()
if ($branch -ne "tv4-quantri") {
    throw "Current branch is '$branch'. Switch to tv4-quantri in the GROUP repository first."
}

$remote = & git -C $repo remote get-url origin
if ($LASTEXITCODE -ne 0) { throw "Could not read Git origin." }
if (([string]$remote) -notmatch 'datvau0401-tech/VanPhongPham(?:\.git)?$') {
    throw "Wrong repository remote: $remote. Expected the private GROUP repository."
}
$dirty = @(& git -C $repo status --porcelain)
if ($LASTEXITCODE -ne 0) { throw "Failed to check Git working tree." }
if ($dirty.Count -gt 0 -and -not ($dirty.Count -eq 1 -and [string]::IsNullOrWhiteSpace($dirty[0]))) {
    throw "Working tree has changes. Commit or stash them before importing TV4."
}

Write-Host "Validated repository: $repo"
Write-Host "Validated branch: $branch"

$dir = Join-Path ([IO.Path]::GetTempPath()) ("tmdt-tv4-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $dir -Force | Out-Null
$archive = Join-Path $dir "tv4.zip"
Write-Host "Downloading TV4 from anh632k-ui/TMDT (branch tv4-quantri)..."
Invoke-WebRequest -Uri "https://github.com/anh632k-ui/TMDT/archive/refs/heads/tv4-quantri.zip" -OutFile $archive
Expand-Archive -LiteralPath $archive -DestinationPath $dir -Force

$source = Join-Path $dir "TMDT-tv4-quantri/VanPhongPham"
if (!(Test-Path -LiteralPath $source -PathType Container)) {
    throw "TV4 source folder not found in downloaded archive."
}

$owned = @(
    "Areas/Admin",
    "Controllers/PaymentController.cs",
    "Views/Payment/Mock.cshtml",
    "Data/DbSeeder.cs",
    "wwwroot/images/products/placeholder.svg"
)

# Check every source path BEFORE writing anything into the group repo.
foreach ($rel in $owned) {
    $src = Join-Path $source $rel
    if (!(Test-Path -LiteralPath $src)) {
        throw "Missing TV4 source path: $rel"
    }
}

$backup = Join-Path $dir "backup-existing-TV4"
foreach ($rel in $owned) {
    $src = Join-Path $source $rel
    $dst = Join-Path $app $rel

    if (Test-Path -LiteralPath $dst) {
        $saved = Join-Path $backup $rel
        New-Item -ItemType Directory -Path (Split-Path $saved -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $dst -Destination $saved -Recurse -Force
    }

    if (Test-Path -LiteralPath $src -PathType Container) {
        New-Item -ItemType Directory -Path $dst -Force | Out-Null
        Get-ChildItem -LiteralPath $src -Force | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $dst -Recurse -Force
        }
    }
    else {
        New-Item -ItemType Directory -Path (Split-Path $dst -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $src -Destination $dst -Force
    }

    Write-Host "Imported: $rel"
}

Write-Host ""
Write-Host "Import finished. No commits or pushes were made."
Write-Host "Existing files were backed up (if any) under: $backup"
Write-Host "TV3 must add DbSeeder.SeedAsync to Program.cs."
& git -C $repo status --short
& git -C $repo diff --check
if ($LASTEXITCODE -ne 0) { throw "git diff --check failed. Review before committing." }
