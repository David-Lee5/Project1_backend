using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Service.DTOs;

namespace RepairSystem.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var sql = ex as SqlException ?? ex.InnerException as SqlException;
            var (status, message) = ex switch
            {
                KeyNotFoundException => (404, ex.Message),
                UnauthorizedAccessException => (403, "Bạn không có quyền thực hiện thao tác này."),
                ArgumentException => (400, ex.Message),
                InvalidOperationException => (409, ex.Message),
                _ when sql?.Number is 2601 or 2627 => (409, "Dữ liệu đã tồn tại. Vui lòng kiểm tra lại."),
                _ when sql?.Number is 547 => (409, "Dữ liệu đang được sử dụng hoặc không đáp ứng ràng buộc nghiệp vụ."),
                _ when sql?.Number is 1205 or 1222 => (409, "Dữ liệu đang được xử lý đồng thời. Vui lòng thử lại thao tác."),
                _ when sql?.Number >= 50000 => (409, "Thao tác không hợp lệ với trạng thái dữ liệu hiện tại."),
                _ when sql?.Number is -2 or -1 or 2 or 53 or 4060 or 18456 => (503, "Cơ sở dữ liệu hiện chưa sẵn sàng."),
                DbUpdateConcurrencyException => (409, "Dữ liệu vừa được thay đổi. Vui lòng tải lại và thử lại."),
                DbUpdateException => (409, "Không thể lưu thay đổi do ràng buộc dữ liệu."),
                _ => (500, "Đã xảy ra lỗi xử lý. Vui lòng thử lại sau.")
            };
            // Log identifiers and exception type, never SQL text, credentials, or request bodies.
            logger.Log(status >= 500 ? LogLevel.Error : LogLevel.Warning,
                "Request {TraceId} failed with {ExceptionType}, status {Status}",
                context.TraceIdentifier, ex.GetType().Name, status);
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse(message));
        }
    }
}
