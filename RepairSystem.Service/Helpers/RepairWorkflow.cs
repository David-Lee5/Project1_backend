namespace RepairSystem.Service.Helpers;

/// <summary>Repair state rules shared by service validation and clients reading the workflow.</summary>
public static class RepairWorkflow
{
    public const string Received = "TIEP_NHAN";
    public const string Repairing = "DANG_SUA";
    public const string WaitingParts = "CHO_LINH_KIEN";
    public const string Completed = "HOAN_THANH";
    public const string Returned = "DA_TRA";
    public const string Cancelled = "HUY";

    public static readonly IReadOnlySet<string> States = new HashSet<string>(StringComparer.Ordinal)
    { Received, Repairing, WaitingParts, Completed, Returned, Cancelled };

    public static bool IsMutable(string state) => state is Received or Repairing or WaitingParts;

    public static bool CanTransition(string from, string to) => (from, to) switch
    {
        (Received, Repairing or Cancelled) => true,
        (Repairing, WaitingParts or Completed or Cancelled) => true,
        (WaitingParts, Repairing or Cancelled) => true,
        (Completed, Returned) => true,
        _ => false
    };

    public static void ValidateTransition(string from, string to, bool assigned, string? result, string? cancellationReason)
    {
        if (!States.Contains(to)) throw new ArgumentException("Trạng thái phiếu không hợp lệ.");
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Không thể chuyển phiếu từ {from} sang {to}.");
        if (to is not (Received or Cancelled) && !assigned)
            throw new InvalidOperationException("Phiếu phải được phân công kỹ thuật viên trước khi chuyển trạng thái.");
        if (to == Completed && string.IsNullOrWhiteSpace(result))
            throw new ArgumentException("Cần ghi kết quả sửa chữa trước khi hoàn thành.");
        if (to == Cancelled && string.IsNullOrWhiteSpace(cancellationReason))
            throw new ArgumentException("Cần ghi lý do huỷ phiếu.");
    }
}
