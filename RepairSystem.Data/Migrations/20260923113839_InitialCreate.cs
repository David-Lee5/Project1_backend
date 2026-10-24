using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepairSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DICH_VU",
                columns: table => new
                {
                    MaDV = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenDichVu = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GiaNiemYet = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DICH_VU__2725865754CDEA0D", x => x.MaDV);
                });

            migrationBuilder.CreateTable(
                name: "KHO",
                columns: table => new
                {
                    MaKho = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenKho = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NguoiPhuTrach = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KHO__3BDA93500D0E6044", x => x.MaKho);
                });

            migrationBuilder.CreateTable(
                name: "LINH_KIEN",
                columns: table => new
                {
                    MaLK = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenLinhKien = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DonGiaBan = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LINH_KIE__2725C77A842FD052", x => x.MaLK);
                });

            migrationBuilder.CreateTable(
                name: "NHA_CUNG_CAP",
                columns: table => new
                {
                    MaNCC = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenNCC = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NHA_CUNG__3A185DEBA0BDFFC3", x => x.MaNCC);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TON_KHO",
                columns: table => new
                {
                    MaKho = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaLK = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TON_KHO__89A8CF27CCDF5B97", x => new { x.MaKho, x.MaLK });
                    table.ForeignKey(
                        name: "FK_TonKho_Kho",
                        column: x => x.MaKho,
                        principalTable: "KHO",
                        principalColumn: "MaKho");
                    table.ForeignKey(
                        name: "FK_TonKho_LinhKien",
                        column: x => x.MaLK,
                        principalTable: "LINH_KIEN",
                        principalColumn: "MaLK");
                });

            migrationBuilder.CreateTable(
                name: "PHIEU_NHAP",
                columns: table => new
                {
                    MaPN = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TongTienNhap = table.Column<decimal>(type: "decimal(18,0)", nullable: true, defaultValue: 0m),
                    MaNCC = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKho = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PHIEU_NH__2725E7F02B9DBE3F", x => x.MaPN);
                    table.ForeignKey(
                        name: "FK_PN_Kho",
                        column: x => x.MaKho,
                        principalTable: "KHO",
                        principalColumn: "MaKho");
                    table.ForeignKey(
                        name: "FK_PN_NCC",
                        column: x => x.MaNCC,
                        principalTable: "NHA_CUNG_CAP",
                        principalColumn: "MaNCC");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    PermissionsId = table.Column<int>(type: "int", nullable: false),
                    RolesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TAI_KHOAN",
                columns: table => new
                {
                    TaiKhoanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TAI_KHOA__9A124B652638884D", x => x.TaiKhoanID);
                    table.ForeignKey(
                        name: "FK_TAI_KHOAN_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CT_NHAP_KHO",
                columns: table => new
                {
                    MaPN = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaLK = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoLuongNhap = table.Column<int>(type: "int", nullable: false),
                    DonGiaNhap = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(29,0)", nullable: true, computedColumnSql: "([SoLuongNhap]*[DonGiaNhap])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CT_NHAP___9557BB877487EBE4", x => new { x.MaPN, x.MaLK });
                    table.ForeignKey(
                        name: "FK_CTNK_LK",
                        column: x => x.MaLK,
                        principalTable: "LINH_KIEN",
                        principalColumn: "MaLK");
                    table.ForeignKey(
                        name: "FK_CTNK_PN",
                        column: x => x.MaPN,
                        principalTable: "PHIEU_NHAP",
                        principalColumn: "MaPN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KHACH_HANG",
                columns: table => new
                {
                    MaKH = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TaiKhoanID = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KHACH_HA__2725CF1EC9DB226A", x => x.MaKH);
                    table.ForeignKey(
                        name: "FK_KH_TaiKhoan",
                        column: x => x.TaiKhoanID,
                        principalTable: "TAI_KHOAN",
                        principalColumn: "TaiKhoanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "KY_THUAT_VIEN",
                columns: table => new
                {
                    MaKTV = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TaiKhoanID = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KY_THUAT__3BDE216877E3CEF9", x => x.MaKTV);
                    table.ForeignKey(
                        name: "FK_KTV_TaiKhoan",
                        column: x => x.TaiKhoanID,
                        principalTable: "TAI_KHOAN",
                        principalColumn: "TaiKhoanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "THIET_BI",
                columns: table => new
                {
                    MaThietBi = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenThietBi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LoaiMay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoSerial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MaKH = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__THIET_BI__8AEC71F73CBF8F13", x => x.MaThietBi);
                    table.ForeignKey(
                        name: "FK_TB_KhachHang",
                        column: x => x.MaKH,
                        principalTable: "KHACH_HANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PHIEU_SUA_CHUA",
                columns: table => new
                {
                    MaPhieu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NgayNhan = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    NgayHenTra = table.Column<DateTime>(type: "datetime", nullable: true),
                    TinhTrangLoi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Đang chờ"),
                    MaThietBi = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKTV = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PHIEU_SU__2660BFE055CADCA4", x => x.MaPhieu);
                    table.ForeignKey(
                        name: "FK_PSC_KTV",
                        column: x => x.MaKTV,
                        principalTable: "KY_THUAT_VIEN",
                        principalColumn: "MaKTV");
                    table.ForeignKey(
                        name: "FK_PSC_ThietBi",
                        column: x => x.MaThietBi,
                        principalTable: "THIET_BI",
                        principalColumn: "MaThietBi");
                });

            migrationBuilder.CreateTable(
                name: "CT_DICH_VU",
                columns: table => new
                {
                    MaPhieu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaDV = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DonGiaThucTe = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(29,0)", nullable: true, computedColumnSql: "([SoLuong]*[DonGiaThucTe])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CT_DICH___4412E785C043331F", x => new { x.MaPhieu, x.MaDV });
                    table.ForeignKey(
                        name: "FK_CTDV_DV",
                        column: x => x.MaDV,
                        principalTable: "DICH_VU",
                        principalColumn: "MaDV");
                    table.ForeignKey(
                        name: "FK_CTDV_PSC",
                        column: x => x.MaPhieu,
                        principalTable: "PHIEU_SUA_CHUA",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CT_XUAT_KHO",
                columns: table => new
                {
                    MaPhieu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaLK = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKho = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SoLuongXuat = table.Column<int>(type: "int", nullable: false),
                    DonGiaXuat = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(29,0)", nullable: true, computedColumnSql: "([SoLuongXuat]*[DonGiaXuat])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CT_XUAT___9412E397964851C3", x => new { x.MaPhieu, x.MaLK });
                    table.ForeignKey(
                        name: "FK_CTXK_Kho",
                        column: x => x.MaKho,
                        principalTable: "KHO",
                        principalColumn: "MaKho");
                    table.ForeignKey(
                        name: "FK_CTXK_LK",
                        column: x => x.MaLK,
                        principalTable: "LINH_KIEN",
                        principalColumn: "MaLK");
                    table.ForeignKey(
                        name: "FK_CTXK_PSC",
                        column: x => x.MaPhieu,
                        principalTable: "PHIEU_SUA_CHUA",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HOA_DON",
                columns: table => new
                {
                    MaHD = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TongThanhToan = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    PhuongThucTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Chưa thanh toán"),
                    MaPhieu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HOA_DON__2725A6E0645E1CA2", x => x.MaHD);
                    table.ForeignKey(
                        name: "FK_HD_PSC",
                        column: x => x.MaPhieu,
                        principalTable: "PHIEU_SUA_CHUA",
                        principalColumn: "MaPhieu");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CT_DICH_VU_MaDV",
                table: "CT_DICH_VU",
                column: "MaDV");

            migrationBuilder.CreateIndex(
                name: "IX_CT_NHAP_KHO_MaLK",
                table: "CT_NHAP_KHO",
                column: "MaLK");

            migrationBuilder.CreateIndex(
                name: "IX_CT_XUAT_KHO_MaKho",
                table: "CT_XUAT_KHO",
                column: "MaKho");

            migrationBuilder.CreateIndex(
                name: "IX_CT_XUAT_KHO_MaLK",
                table: "CT_XUAT_KHO",
                column: "MaLK");

            migrationBuilder.CreateIndex(
                name: "UQ__HOA_DON__2660BFE18BBDBB1F",
                table: "HOA_DON",
                column: "MaPhieu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__KHACH_HA__9A124B64AAD5E738",
                table: "KHACH_HANG",
                column: "TaiKhoanID",
                unique: true,
                filter: "[TaiKhoanID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__KY_THUAT__9A124B64E1C3A243",
                table: "KY_THUAT_VIEN",
                column: "TaiKhoanID",
                unique: true,
                filter: "[TaiKhoanID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEU_NHAP_MaKho",
                table: "PHIEU_NHAP",
                column: "MaKho");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEU_NHAP_MaNCC",
                table: "PHIEU_NHAP",
                column: "MaNCC");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEU_SUA_CHUA_MaKTV",
                table: "PHIEU_SUA_CHUA",
                column: "MaKTV");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEU_SUA_CHUA_MaThietBi",
                table: "PHIEU_SUA_CHUA",
                column: "MaThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RolesId",
                table: "RolePermissions",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_TAI_KHOAN_RoleId",
                table: "TAI_KHOAN",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__TAI_KHOA__536C85E42DB62234",
                table: "TAI_KHOAN",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_THIET_BI_MaKH",
                table: "THIET_BI",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "UQ__THIET_BI__A4DDF2C9F6A339A2",
                table: "THIET_BI",
                column: "SoSerial",
                unique: true,
                filter: "[SoSerial] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TON_KHO_MaLK",
                table: "TON_KHO",
                column: "MaLK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CT_DICH_VU");

            migrationBuilder.DropTable(
                name: "CT_NHAP_KHO");

            migrationBuilder.DropTable(
                name: "CT_XUAT_KHO");

            migrationBuilder.DropTable(
                name: "HOA_DON");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "TON_KHO");

            migrationBuilder.DropTable(
                name: "DICH_VU");

            migrationBuilder.DropTable(
                name: "PHIEU_NHAP");

            migrationBuilder.DropTable(
                name: "PHIEU_SUA_CHUA");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "LINH_KIEN");

            migrationBuilder.DropTable(
                name: "KHO");

            migrationBuilder.DropTable(
                name: "NHA_CUNG_CAP");

            migrationBuilder.DropTable(
                name: "KY_THUAT_VIEN");

            migrationBuilder.DropTable(
                name: "THIET_BI");

            migrationBuilder.DropTable(
                name: "KHACH_HANG");

            migrationBuilder.DropTable(
                name: "TAI_KHOAN");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
