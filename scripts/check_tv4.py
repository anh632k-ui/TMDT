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
        for name in ("README.md", "DEMO.md", "TEST_CASES.csv", "INTEGRATION_CHECKLIST.md", "EVIDENCE_INDEX.md", "BACKUP_AND_VIDEO.md"):
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

    def test_test_case_matrix_matches_current_instructions(self):
        with (ROOT / "docs/TV4/TEST_CASES.csv").open(encoding="utf-8-sig", newline="") as f:
            reader = csv.DictReader(f)
            rows = list(reader)
            header = list(reader.fieldnames or [])
        self.assertEqual(header, [
            "STT", "Tình huống", "Chuẩn bị", "Các bước",
            "Kết quả mong đợi", "Kết quả thực tế", "Đạt/Không đạt", "Người sửa"
        ])
        self.assertEqual(len(rows), 20)
        self.assertEqual([int(r["STT"]) for r in rows], list(range(1, 21)))
        self.assertTrue(all(all(r.get(k) is not None for k in header) for r in rows))
        for key in ("Tình huống", "Chuẩn bị", "Các bước", "Kết quả mong đợi"):
            self.assertTrue(all(r[key].strip() for r in rows), key)
        self.assertIn("LUOTCUOI", rows[12]["Tình huống"] + rows[12]["Các bước"])
        self.assertIn("hai tab", (rows[13]["Tình huống"] + rows[13]["Các bước"]).lower())
        self.assertIn("/Admin", rows[18]["Các bước"])
        for row in rows:
            self.assertIn(row["Đạt/Không đạt"], {"Chưa chạy", "Đạt", "Không đạt"})
            if row["Đạt/Không đạt"] != "Chưa chạy":
                self.assertTrue(row["Kết quả thực tế"].strip(),
                                f"TC{row['STT']} cannot be marked complete without actual observations")

    def test_demo_a_b_c_and_safety_docs(self):
        demo = (ROOT / "docs/TV4/DEMO.md").read_text("utf-8")
        for mark in ("A – Không bán vượt kho", "B – Không vượt lượt voucher",
                     "C – Không đặt đơn trùng", "hai tab", "Lần 1", "Lần 2"):
            self.assertIn(mark, demo)
        for name in ("EVIDENCE_INDEX.md", "BACKUP_AND_VIDEO.md"):
            self.assertTrue((ROOT / "docs" / "TV4" / name).is_file())
        backup = (ROOT / "docs/TV4/BACKUP_AND_VIDEO.md").read_text("utf-8")
        self.assertIn("RESTORE VERIFYONLY", backup)
        self.assertIn("Không push .bak", backup)

    def test_no_internal_note_in_admin_razor_views(self):
        root = APP / "Areas/Admin/Views"
        for view in root.rglob("*.cshtml"):
            source = view.read_text("utf-8")
            with self.subTest(view=view.name):
                self.assertNotIn("IOrderService của TV3 xử lý", source)
                self.assertNotRegex(source, r"(?i)\\bTODO\\b|\\bTV[234]\\b")

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
