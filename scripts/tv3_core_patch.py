#!/usr/bin/env python3
"""Version-locked fixes for TV3 based on VanPhongPham-main.zip dated 26/10/2026.

Dry run by default. Never modifies GitHub or performs a Git operation.
Needs manual review and EF migration in the PRIVATE group project.
"""
from __future__ import annotations

import argparse
import subprocess
from pathlib import Path

FIXES: dict[str, list[tuple[str, str]]] = {
    "Program.cs": [
        (" o.Password.RequiredLength=3;", " o.Password.RequiredLength=12;"),
        (" o.Password.RequiredUniqueChars=1;", " o.Password.RequiredUniqueChars=4;"),
        (" o.Password.RequireDigit=false;", " o.Password.RequireDigit=true;"),
        (" o.Password.RequireLowercase=false;", " o.Password.RequireLowercase=true;"),
        (" o.Password.RequireUppercase=false;", " o.Password.RequireUppercase=true;"),
        (" o.Password.RequireNonAlphanumeric=false;", """ o.Password.RequireNonAlphanumeric=true;
 o.User.RequireUniqueEmail=true;
 o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);
 o.Lockout.MaxFailedAccessAttempts=5;
 o.Lockout.AllowedForNewUsers=true;"""),
    ],
    "Areas/Identity/Pages/Account/Register.cshtml.cs": [
        ('StringLength(100,MinimumLength=3,ErrorMessage="Mật khẩu phải từ 3 đến 100 ký tự")',
         'StringLength(100,MinimumLength=12,ErrorMessage="Mật khẩu phải từ 12 đến 100 ký tự")'),
        ("  if(!ModelState.IsValid) return Page();", """  if(!ModelState.IsValid) return Page();
  Input.Email=Input.Email.Trim();
  if(await users.FindByEmailAsync(Input.Email)!=null) {
   ModelState.AddModelError("Input.Email","Email đã được sử dụng");
   return Page();
  }"""),
    ],
    "Areas/Identity/Pages/Account/Register.cshtml": [
        ('autocomplete="off" minlength="3"', 'autocomplete="new-password" minlength="12"'),
        ('<span asp-validation-for="Input.Password" class="text-danger"></span></div>',
         '<div class="form-text">Ít nhất 12 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.</div><span asp-validation-for="Input.Password" class="text-danger"></span></div>'),
    ],
    "Services/OrderService.cs": [
        ("""   var now=DateTime.Now;
   foreach(var u in usages) {
    await db.Vouchers.Where(x=>x.Id==u.VoucherId && x.StartAt<=now && x.EndAt>=now && x.UsedCount>0)
     .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.UsedCount,x=>x.UsedCount-1));
    u.Status=UsageStatus.Released;
   }""",
         """   foreach(var u in usages) {
    // Hoàn lượt cho đơn hủy, kể cả khi voucher đã hết hạn.
    var released=await db.Vouchers.Where(x=>x.Id==u.VoucherId && x.UsedCount>0)
     .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.UsedCount,x=>x.UsedCount-1));
    if(released!=1) {
     await tx.RollbackAsync();
     return(false,"Không thể hoàn lượt voucher, cần kiểm tra dữ liệu");
    }
    u.Status=UsageStatus.Released;
   }"""),
    ],
    "Services/UnpaidOrderCleanup.cs": [
        ("public class UnpaidOrderCleanup(IServiceScopeFactory scopes,ILogger<UnpaidOrderCleanup> logger) : BackgroundService {",
         "public class UnpaidOrderCleanup(IServiceScopeFactory scopes,ILogger<UnpaidOrderCleanup> logger,IConfiguration config) : BackgroundService {"),
        ("  using var timer=new PeriodicTimer(TimeSpan.FromMinutes(5));",
         """  var interval=Math.Max(1,config.GetValue<int>("Orders:CleanupIntervalMinutes",5));
  var unpaid=Math.Max(1,config.GetValue<int>("Orders:UnpaidMinutes",30));
  using var timer=new PeriodicTimer(TimeSpan.FromMinutes(interval));"""),
        ("    var cutoff=DateTime.Now.AddMinutes(-30);", "    var cutoff=DateTime.Now.AddMinutes(-unpaid);"),
    ],
    "Data/ApplicationDbContext.cs": [
        ("  mb.Entity<Voucher>().HasIndex(x => x.Code).IsUnique();",
         """  mb.Entity<Voucher>().HasIndex(x => x.Code).IsUnique();
  mb.Entity<Product>().ToTable(t => {
   t.HasCheckConstraint("CK_Products_Stock_Nonnegative", "[Stock] >= 0");
   t.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
  });
  mb.Entity<OrderItem>().ToTable(t => {
   t.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
   t.HasCheckConstraint("CK_OrderItems_UnitPrice_Nonnegative", "[UnitPrice] >= 0");
  });
  mb.Entity<Voucher>().ToTable(t => {
   t.HasCheckConstraint("CK_Vouchers_UsedCount_Nonnegative", "[UsedCount] >= 0");
   t.HasCheckConstraint("CK_Vouchers_Limit",
    "[TotalLimit] IS NULL OR ([TotalLimit] >= 1 AND [UsedCount] <= [TotalLimit])");
  });"""),
    ],
}


def compute(root: Path) -> dict[Path, tuple[str, str]]:
    result = {}
    for relative, replacements in FIXES.items():
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Missing file: {path}")
        original = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
        updated = original
        for old, new in replacements:
            if updated.count(old) != 1:
                raise ValueError(f"Source differs from reviewed snapshot: {relative}, anchor {old[:70]!r}")
            updated = updated.replace(old, new, 1)
        result[path] = (original, updated)
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project", type=Path, help="Directory containing VanPhongPham.csproj")
    parser.add_argument("--apply", action="store_true", help="Apply ONLY after reviewing diff and clean Git status")
    args = parser.parse_args()
    root = args.project.resolve()
    if not (root / "VanPhongPham.csproj").is_file():
        parser.error("Provide the directory containing VanPhongPham.csproj")
    files = compute(root)  # Validate ALL six files before touching any.
    print(f"Verified {len(files)} TV3 files against reviewed snapshot; no unrelated paths included.")
    if not args.apply:
        print("DRY-RUN ONLY. Nothing changed. Use --apply on a clean feature branch with TV3 approval.")
        return
    repo = subprocess.run(["git", "-C", str(root), "status", "--porcelain"],
                          capture_output=True, text=True, check=True)
    if repo.stdout.strip():
        raise SystemExit("Working tree is not clean. Refusing to overwrite existing changes.")
    for path, (_, new) in files.items():
        with path.open("w", encoding="utf-8", newline="") as handle:
            handle.write(new)
        print(f"Updated: {path.relative_to(root)}")
    print("Patch applied locally. REVIEW git diff; dotnet build, EF MIGRATION and SQL tests are still required.")


if __name__ == "__main__":
    main()
