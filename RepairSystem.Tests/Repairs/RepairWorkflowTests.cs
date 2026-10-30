using RepairSystem.Service.Helpers;

namespace RepairSystem.Tests.Repairs;

public class RepairWorkflowTests
{
    [Theory]
    [InlineData("TIEP_NHAN", "DANG_SUA")]
    [InlineData("TIEP_NHAN", "HUY")]
    [InlineData("DANG_SUA", "CHO_LINH_KIEN")]
    [InlineData("DANG_SUA", "HOAN_THANH")]
    [InlineData("DANG_SUA", "HUY")]
    [InlineData("CHO_LINH_KIEN", "DANG_SUA")]
    [InlineData("CHO_LINH_KIEN", "HUY")]
    [InlineData("HOAN_THANH", "DA_TRA")]
    public void AllowsDocumentedProgress(string from, string to) =>
        RepairWorkflow.ValidateTransition(from, to, assigned: true, result: "Đã thay thế bộ phận lỗi", cancellationReason: "Khách hàng huỷ");

    [Theory]
    [InlineData("TIEP_NHAN", "HOAN_THANH")]
    [InlineData("CHO_LINH_KIEN", "HOAN_THANH")]
    [InlineData("HOAN_THANH", "DANG_SUA")]
    [InlineData("HOAN_THANH", "HUY")]
    [InlineData("DA_TRA", "DANG_SUA")]
    [InlineData("HUY", "DANG_SUA")]
    [InlineData("DANG_SUA", "DANG_SUA")]
    public void RejectsInvalidOrTerminalTransitions(string from, string to) =>
        Assert.Throws<InvalidOperationException>(() => RepairWorkflow.ValidateTransition(from, to, true, "Đã sửa", "Huỷ"));

    [Fact]
    public void CannotStartWorkWithoutAssignedTechnician() =>
        Assert.Throws<InvalidOperationException>(() => RepairWorkflow.ValidateTransition("TIEP_NHAN", "DANG_SUA", false, null, null));

    [Fact]
    public void CompletionNeedsRepairResult() =>
        Assert.Throws<ArgumentException>(() => RepairWorkflow.ValidateTransition("DANG_SUA", "HOAN_THANH", true, " ", null));

    [Fact]
    public void CancellationNeedsReason() =>
        Assert.Throws<ArgumentException>(() => RepairWorkflow.ValidateTransition("TIEP_NHAN", "HUY", false, null, " "));
}
