namespace RepairSystem.Model;

public sealed class MaLienKetKhachHang
{
    public long Id { get; set; }
    public long KhachHangId { get; set; }
    public string MaHash { get; set; } = null!;
    public DateTime HetHan { get; set; }
    public DateTime? DaDungLuc { get; set; }
    public long NguoiCapId { get; set; }
    public DateTime CreatedAt { get; set; }
}
