param(
    [Parameter(Mandatory=$true)]
    [string]$RepoRoot
)

$ErrorActionPreference = "Stop"
$repo = (Resolve-Path -LiteralPath $RepoRoot).Path
$app = Join-Path $repo "VanPhongPham"
if (!(Test-Path -LiteralPath (Join-Path $app "VanPhongPham.csproj"))) {
    throw "Không tìm thấy VanPhongPham/VanPhongPham.csproj. Hãy chỉ đường dẫn tới GỐC repo nhóm, không phải thư mục chứa .csproj."
}
$branch = (& git -C $repo branch --show-current).Trim()
if ($LASTEXITCODE -ne 0) { throw "Chưa mở đúng Git repository." }
if ($branch -ne "tv4-quantri") {
    throw "Đang ở nhánh '$branch'. Hãy tạo/chuyển sang nhánh tv4-quantri của repo NHÓM trước khi chạy."
}
$dirty = & git -C $repo status --porcelain
if ($LASTEXITCODE -ne 0 -or $dirty) {
    throw "Working tree chưa sạch; hãy commit/stash thay đổi trước khi ghép."
}

$dir = Join-Path ([IO.Path]::GetTempPath()) ("tmdt-tv4-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $dir | Out-Null
$archive = Join-Path $dir "tv4.zip"
Write-Host "Tải mã TV4 từ nhánh tv4-quantri do bạn quản lý..."
Invoke-WebRequest -Uri "https://github.com/anh632k-ui/TMDT/archive/refs/heads/tv4-quantri.zip" -OutFile $archive
Expand-Archive -LiteralPath $archive -DestinationPath $dir -Force
$source = Join-Path $dir "TMDT-tv4-quantri/VanPhongPham"
if (!(Test-Path -LiteralPath $source)) { throw "Không tìm được thư mục nguồn TV4 trong bản tải về." }

$owned = @(
    "Areas/Admin",
    "Controllers/PaymentController.cs",
    "Views/Payment/Mock.cshtml",
    "Data/DbSeeder.cs",
    "wwwroot/images/products/placeholder.svg"
)

$backup = Join-Path $dir "backup-existing-TV4"
foreach ($rel in $owned) {
    $src = Join-Path $source $rel
    $dst = Join-Path $app $rel
    if (!(Test-Path -LiteralPath $src)) { throw "Thiếu nguồn TV4: $rel" }
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
    } else {
        New-Item -ItemType Directory -Path (Split-Path $dst -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $src -Destination $dst -Force
    }
    Write-Host ("TV4: " + $rel)
}

Write-Host ""
Write-Host "Đã ghép các đường dẫn thuộc TV4, chưa commit/push."
Write-Host ("Backup source đã ghi đè: " + $backup)
Write-Host "Kiểm tra git diff và hỏi TV3 bổ sung lời gọi DbSeeder.SeedAsync trong Program.cs."
& git -C $repo status --short
& git -C $repo diff --check
if ($LASTEXITCODE -ne 0) { throw "git diff --check báo vấn đề. Không commit trước khi sửa." }
