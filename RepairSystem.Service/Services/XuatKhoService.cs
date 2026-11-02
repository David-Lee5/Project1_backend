using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class XuatKhoService : IXuatKhoService
{
    private const string TrongKho = "TRONG_KHO";

    private readonly AppDbContext _context;

    public XuatKhoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DongXuatKhoDto>> XuatKhoAsync(long phieuId, XuatKhoRequestDto dto, long taiKhoanId)
    {
        var thuKhoId = await GetThuKhoIdAsync(taiKhoanId);

        var serialIds = dto.LinhKienSerialIds ?? new List<long>();
        if (serialIds.Count != serialIds.Distinct().Count())
            throw new ArgumentException("Danh sách serial bị chọn trùng.");

        // Có chọn serial thì mỗi serial là một dòng xuất, số lượng = số serial
        var soLuong = serialIds.Count > 0 ? serialIds.Count : dto.SoLuong;
        if (soLuong <= 0)
            throw new ArgumentException("Số lượng xuất phải lớn hơn 0.");

        if (!await _context.Khos.AnyAsync(k => k.Id == dto.KhoId && k.DeletedAt == null))
            throw new ArgumentException($"Kho id={dto.KhoId} không tồn tại.");
        if (!await _context.LinhKiens.AnyAsync(l => l.Id == dto.LinhKienId && l.DeletedAt == null))
            throw new ArgumentException($"Linh kiện id={dto.LinhKienId} không tồn tại hoặc đã bị xoá.");

        if (serialIds.Count > 0)
        {
            var serials = await _context.LinhKienSerials
                .AsNoTracking()
                .Where(s => serialIds.Contains(s.Id))
                .ToListAsync();

            var khongThay = serialIds.Except(serials.Select(s => s.Id)).ToList();
            if (khongThay.Count > 0)
                throw new ArgumentException($"Không tìm thấy serial id: {string.Join(", ", khongThay)}.");

            var khongHopLe = serials
                .Where(s => s.LinhKienId != dto.LinhKienId || s.KhoId != dto.KhoId || s.TrangThai != TrongKho)
                .Select(s => s.SoSerial)
                .ToList();
            if (khongHopLe.Count > 0)
                throw new InvalidOperationException(
                    $"Serial không còn trong kho này hoặc không thuộc linh kiện đã chọn: {string.Join(", ", khongHopLe)}.");
        }
        else
        {
            // Linh kiện đang theo dõi serial thì bắt buộc chọn serial, tránh tồn số lượng lệch tồn serial
            var conSerial = await _context.LinhKienSerials.AnyAsync(s =>
                s.LinhKienId == dto.LinhKienId && s.KhoId == dto.KhoId && s.TrangThai == TrongKho);
            if (conSerial)
                throw new ArgumentException(
                    "Linh kiện này trong kho đang được theo dõi theo serial. Vui lòng chọn serial cụ thể để xuất.");
        }

        var ids = new List<long>();
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            if (serialIds.Count == 0)
            {
                ids.Add(await GoiXuatKhoAsync(phieuId, dto.KhoId, dto.LinhKienId, soLuong, thuKhoId));
            }
            else
            {
                foreach (var serialId in serialIds)
                {
                    var chiTietId = await GoiXuatKhoAsync(phieuId, dto.KhoId, dto.LinhKienId, 1, thuKhoId);

                    // Một câu UPDATE có điều kiện: hai người xuất cùng một serial thì chỉ một người thành công
                    var daGan = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE dbo.linh_kien_serial
                           SET trang_thai = 'DA_XUAT',
                               chi_tiet_xuat_kho_id = {chiTietId},
                               kho_id = NULL,
                               updated_at = GETUTCDATE()
                         WHERE id = {serialId}
                           AND trang_thai = 'TRONG_KHO'
                           AND linh_kien_id = {dto.LinhKienId}
                           AND kho_id = {dto.KhoId}");
                    if (daGan == 0)
                        throw new InvalidOperationException(
                            "Một serial vừa được người khác xuất hoặc không còn trong kho. Vui lòng tải lại danh sách và thử lại.");

                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE dbo.chi_tiet_xuat_kho
                           SET linh_kien_serial_id = {serialId}
                         WHERE id = {chiTietId}");

                    ids.Add(chiTietId);
                }
            }

            await tx.CommitAsync();
        }
        catch (SqlException ex)
        {
            await tx.RollbackAsync();
            throw MapSqlException(ex);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return await DocDongXuatAsync(x => ids.Contains(x.Id));
    }

    public Task<List<DongXuatKhoDto>> GetXuatKhoCuaPhieuAsync(long phieuId) =>
        DocDongXuatAsync(x => x.PhieuId == phieuId);

    // ── Nội bộ ─────────────────────────────────────────────────────

    /// <summary>Gọi usp_xuat_kho: trừ tồn có điều kiện, ghi dòng xuất và sổ biến động. Trả về id dòng xuất.</summary>
    private async Task<long> GoiXuatKhoAsync(long phieuId, long khoId, long linhKienId, int soLuong, long thuKhoId)
    {
        var outId = new SqlParameter("@chi_tiet_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.usp_xuat_kho @phieu_id, @kho_id, @linh_kien_id, @so_luong, @thu_kho_id, @chi_tiet_id OUTPUT",
            new SqlParameter("@phieu_id", phieuId),
            new SqlParameter("@kho_id", khoId),
            new SqlParameter("@linh_kien_id", linhKienId),
            new SqlParameter("@so_luong", soLuong),
            new SqlParameter("@thu_kho_id", thuKhoId),
            outId);

        return (long)outId.Value;
    }

    private async Task<List<DongXuatKhoDto>> DocDongXuatAsync(System.Linq.Expressions.Expression<Func<ChiTietXuatKho, bool>> dieuKien)
    {
        return await _context.ChiTietXuatKhos
            .AsNoTracking()
            .Where(dieuKien)
            .OrderBy(x => x.Id)
            .Select(x => new DongXuatKhoDto
            {
                ChiTietXuatKhoId = x.Id,
                PhieuId = x.PhieuId,
                KhoId = x.KhoId,
                TenKho = x.TonKho.Kho.TenKho,
                LinhKienId = x.LinhKienId,
                MaLk = x.TonKho.LinhKien.MaLk,
                TenLinhKien = x.TonKho.LinhKien.TenLinhKien,
                SoLuongXuat = x.SoLuongXuat,
                DonGiaXuat = x.DonGiaXuat,
                ThanhTien = x.ThanhTien ?? 0,
                NgayXuat = x.NgayXuat,
                BaoHanhDen = x.BaoHanhDen,
                SoSerial = x.LinhKienSerial != null ? x.LinhKienSerial.SoSerial : null
            })
            .ToListAsync();
    }

    private async Task<long> GetThuKhoIdAsync(long taiKhoanId)
    {
        var nhanVienId = await _context.NhanViens
            .Where(n => n.TaiKhoanId == taiKhoanId && n.DeletedAt == null)
            .Select(n => (long?)n.Id)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Tài khoản hiện tại chưa gắn với hồ sơ nhân viên.");

        if (!await _context.ThuKhos.AnyAsync(t => t.NhanVienId == nhanVienId))
            throw new InvalidOperationException("Chỉ thủ kho mới được thực hiện xuất kho.");

        return nhanVienId;
    }

    /// <summary>Đổi lỗi nghiệp vụ THROW 5002x trong usp_xuat_kho thành exception có ý nghĩa.</summary>
    private static Exception MapSqlException(SqlException ex) => ex.Number switch
    {
        50020 => new ArgumentException(ex.Message),
        50021 => new KeyNotFoundException(ex.Message),
        50022 or 50023 => new InvalidOperationException(ex.Message),
        _ => ex
    };
}
