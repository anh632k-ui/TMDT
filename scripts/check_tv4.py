#!/usr/bin/env python3
"""Kiểm tra tĩnh phần TV4; KHÔNG thay thế test website thật."""
from __future__ import annotations

import csv
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / "VanPhongPham"


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8")


class Tv4StaticChecks(unittest.TestCase):
    def test_all_deliverables_exist(self):
        files = [
            "Areas/Admin/Controllers/HomeController.cs",
            "Areas/Admin/Controllers/ProductsController.cs",
            "Areas/Admin/Controllers/OrdersController.cs",
            "Areas/Admin/Controllers/VouchersController.cs",
            "Areas/Admin/ViewModels/ProductFormVm.cs",
            "Areas/Admin/ViewModels/VoucherFormVm.cs",
            "Areas/Admin/Views/_ViewImports.cshtml",
            "Areas/Admin/Views/_ViewStart.cshtml",
            "Areas/Admin/Views/Shared/_AdminLayout.cshtml",
            "Areas/Admin/Views/Home/Index.cshtml",
            "Areas/Admin/Views/Products/Index.cshtml",
            "Areas/Admin/Views/Products/Form.cshtml",
            "Areas/Admin/Views/Orders/Index.cshtml",
            "Areas/Admin/Views/Orders/Details.cshtml",
            "Areas/Admin/Views/Vouchers/Index.cshtml",
            "Areas/Admin/Views/Vouchers/Form.cshtml",
            "Areas/Admin/Views/Vouchers/Details.cshtml",
            "Controllers/PaymentController.cs",
            "Views/Payment/Mock.cshtml",
            "Data/DbSeeder.cs",
            "wwwroot/images/products/placeholder.svg",
        ]
        for path in files:
            with self.subTest(path=path):
                self.assertTrue((APP / path).is_file(), path)
        for name in ("README.md", "DEMO.md", "TEST_CASES.csv", "INTEGRATION_CHECKLIST.md"):
            self.assertTrue((ROOT / "docs" / "TV4" / name).is_file())

    def test_admin_controllers_have_auth_and_area(self):
        for name in ("Home", "Products", "Orders", "Vouchers"):
            source = (APP / f"Areas/Admin/Controllers/{name}Controller.cs").read_text("utf-8")
            with self.subTest(name=name):
                self.assertIn('Area("Admin")', source)
                self.assertIn('Authorize(Roles = "Admin")', source)

    def test_mutations_enforce_csrf(self):
        actions = {"Products": 3, "Orders": 2, "Vouchers": 4}
        for controller, minimum in actions.items():
            s = (APP / f"Areas/Admin/Controllers/{controller}Controller.cs").read_text("utf-8")
            with self.subTest(controller=controller):
                self.assertGreaterEqual(s.count("ValidateAntiForgeryToken"), minimum)
        pay = (APP / "Controllers/PaymentController.cs").read_text("utf-8")
        self.assertGreaterEqual(pay.count("ValidateAntiForgeryToken"), 2)

    def test_payment_only_allows_owned_online_orders(self):
        s = (APP / "Controllers/PaymentController.cs").read_text("utf-8")
        for mark in ('[Authorize]', "x.UserId == userId", 'x.PaymentMethod == "Online"',
                     "OrderStatus.Pending", "_orders.MarkPaidAsync", "_orders.CancelAsync"):
            with self.subTest(mark=mark):
                self.assertIn(mark, s)

    def test_product_upload_constraints(self):
        s = (APP / "Areas/Admin/Controllers/ProductsController.cs").read_text("utf-8")
        for mark in ("MaxImageBytes", ".jpg", ".jpeg", ".png", ".webp",
                     "Guid.NewGuid()", "Directory.CreateDirectory", "ReadAsync(header.AsMemory())",
                     "decimal.Truncate(vm.Price)", "ToggleActive"):
            with self.subTest(mark=mark):
                self.assertIn(mark, s)
        self.assertNotIn("_db.Products.Remove", s)

    def test_voucher_guards(self):
        s = (APP / "Areas/Admin/Controllers/VouchersController.cs").read_text("utf-8")
        for mark in ("vm.EndAt <= vm.StartAt", "vm.Value < 1", "vm.Value > 100",
                     "Regex.IsMatch(vm.Code", "v.UsedCount > 0",
                     "x.UsedCount > vm.TotalLimit.Value", "VoucherUsages.AnyAsync",
                     "VoucherScopes.AddRange", "IsActive = !v.IsActive"):
            with self.subTest(mark=mark):
                self.assertIn(mark, s)
        self.assertNotIn("v.UsedCount = vm.", s)

    def test_order_lifecycle_delegates_to_tv3(self):
        s = (APP / "Areas/Admin/Controllers/OrdersController.cs").read_text("utf-8")
        self.assertIn("_orders.ChangeStatusAsync", s)
        self.assertIn("_orders.CancelAsync", s)
        self.assertNotIn("UsedCount =", s)

    def test_seed_has_sample_data(self):
        s = (APP / "Data/DbSeeder.cs").read_text("utf-8")
        names = re.findall(
            r'\("([^"]+)","(Giấy|Bút viết|Sổ tay|Dụng cụ văn phòng)",\d+,\d+\)', s
        )
        self.assertGreaterEqual(len(names), 20)
        self.assertLessEqual(len(names), 30)
        self.assertEqual(len(names), len(set(x[0] for x in names)))
        codes = re.findall(r'Code="([A-Z0-9]+)"', s)
        self.assertEqual(set(codes), {
            "VPP50", "FREESHIP", "GIAM20K", "HETHAN", "SAPTOI",
            "HETLUOT", "LUOTCUOI", "CHAOMUNG", "GIAY10"
        })
        self.assertIn('FindByEmailAsync(adminEmail)', s)
        self.assertIn('IsInRoleAsync(admin, "Admin")', s)
        self.assertIn('CategoryId = cats["Giấy"]', s)
        self.assertIn('("Sổ tay bìa cứng A5 (bản giới hạn)","Sổ tay",65000,1)', s)

    def test_test_case_matrix_complete_and_unclaimed(self):
        with (ROOT / "docs/TV4/TEST_CASES.csv").open(encoding="utf-8-sig", newline="") as f:
            rows = list(csv.DictReader(f))
        self.assertEqual(len(rows), 23)
        self.assertEqual([int(r["STT"]) for r in rows], list(range(1, 24)))
        self.assertTrue(all(r["Nhóm"] == "Voucher" for r in rows[:14]))
        self.assertEqual([r["Nhóm"] for r in rows[14:17]], ["Tồn kho", "Bảo mật", "Tồn kho"])
        self.assertTrue(all(r["Nhóm"] == "Hệ thống" for r in rows[17:]))
        # Khi có kết quả kiểm thử thật, CI vẫn phải cho phép cập nhật bảng.
        statuses = {"Chưa chạy", "Đạt", "Không đạt", "PASS", "FAIL"}
        for row in rows:
            self.assertIn(row["Trạng thái"], statuses)
            if row["Trạng thái"] in {"Đạt", "Không đạt", "PASS", "FAIL"}:
                self.assertTrue(row["Kết quả thực tế"].strip(), f"Thiếu bằng chứng TC{row['STT']}")

    def test_tv3_integration_contract(self):
        payment = (APP / "Controllers/PaymentController.cs").read_text("utf-8")
        details = (APP / "Areas/Admin/Views/Orders/Details.cshtml").read_text("utf-8")
        self.assertIn("catch (InvalidOperationException ex)", payment)
        self.assertIn('Model.PaymentMethod == "COD"', details)
        self.assertIn('value="Shipping"', details)
        self.assertIn('value="Paid"', details)

    def test_no_accidental_razor_page_directive(self):
        for view in (APP / "Areas/Admin/Views").rglob("*.cshtml"):
            with self.subTest(view=view.name):
                self.assertNotRegex(view.read_text("utf-8"), r"@page\b")

    def test_payment_and_admin_razor_forms_have_anti_forgery(self):
        views = (
            "Areas/Admin/Views/Products/Index.cshtml",
            "Areas/Admin/Views/Products/Form.cshtml",
            "Areas/Admin/Views/Orders/Details.cshtml",
            "Areas/Admin/Views/Vouchers/Index.cshtml",
            "Areas/Admin/Views/Vouchers/Form.cshtml",
            "Views/Payment/Mock.cshtml",
        )
        for name in views:
            source = (APP / name).read_text("utf-8")
            with self.subTest(name=name):
                self.assertIn('method="post"', source)
                self.assertIn("@Html.AntiForgeryToken()", source)


if __name__ == "__main__":
    unittest.main(verbosity=2)
