using Microsoft.Data.SqlClient;

namespace RepairSystem.Infrastructure;

internal static class V3SchemaValidator
{
    private const string Migration = "V3_20261106_API";
    private const string MigrationFile = "database_v3/BO_SUNG_API_20261106.sql";
    private const string KtvMigration = "V3_20261107_KTV";
    private const string KtvMigrationFile = "database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql";

    public static async Task ValidateAsync(string connectionString)
    {
        var settings = new SqlConnectionStringBuilder(connectionString);
        await using var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                $"Không kết nối được SQL Server '{settings.DataSource}', CSDL '{settings.InitialCatalog}'. " +
                "Kiểm tra ConnectionStrings__DefaultConnection và trạng thái SQL Server.", ex);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DB_NAME(),
                   OBJECT_ID(N'dbo.v3_schema_migration', N'U'),
                   OBJECT_ID(N'dbo.ma_lien_ket_khach_hang_v3', N'U'),
                   OBJECT_ID(N'dbo.lich_hen_v3', N'U'),
                   OBJECT_ID(N'dbo.xu_ly_bao_hanh_ncc_v3', N'U'),
                   COL_LENGTH(N'dbo.bao_gia', N'khach_hang_phan_hoi_id'),
                   COL_LENGTH(N'dbo.phieu_bao_hanh_ncc', N'kho_gui_id'),
                   COL_LENGTH(N'dbo.phieu_bao_hanh_ncc', N'da_tru_ton'),
                   COL_LENGTH(N'dbo.phieu_bao_hanh_ncc', N'nguon_hang'),
                   COL_LENGTH(N'dbo.phieu_sua_chua', N'ktv_nhan_id'),
                   COL_LENGTH(N'dbo.phieu_sua_chua', N'ktv_nhan_luc'),
                   COL_LENGTH(N'dbo.phieu_sua_chua', N'ket_qua_kiem_tra');
            """;

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException("Không đọc được thông tin schema CSDL V3.");

        var databaseName = reader.GetString(0);
        if (reader.IsDBNull(1))
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' chưa có schema V3. " +
                "Kiểm tra tên CSDL trong ConnectionStrings__DefaultConnection và tạo CSDL V3 trước khi chạy backend.");

        var missing = new List<string>();
        if (reader.IsDBNull(2)) missing.Add("dbo.ma_lien_ket_khach_hang_v3");
        if (reader.IsDBNull(3)) missing.Add("dbo.lich_hen_v3");
        if (reader.IsDBNull(4)) missing.Add("dbo.xu_ly_bao_hanh_ncc_v3");
        if (reader.IsDBNull(5)) missing.Add("dbo.bao_gia.khach_hang_phan_hoi_id");
        if (reader.IsDBNull(6)) missing.Add("dbo.phieu_bao_hanh_ncc.kho_gui_id");
        if (reader.IsDBNull(7)) missing.Add("dbo.phieu_bao_hanh_ncc.da_tru_ton");
        if (reader.IsDBNull(8)) missing.Add("dbo.phieu_bao_hanh_ncc.nguon_hang");
        var missingKtv = new List<string>();
        if (reader.IsDBNull(9)) missingKtv.Add("dbo.phieu_sua_chua.ktv_nhan_id");
        if (reader.IsDBNull(10)) missingKtv.Add("dbo.phieu_sua_chua.ktv_nhan_luc");
        if (reader.IsDBNull(11)) missingKtv.Add("dbo.phieu_sua_chua.ket_qua_kiem_tra");
        await reader.CloseAsync();

        command.CommandText = "SELECT COUNT_BIG(*) FROM dbo.v3_schema_migration WHERE version = @version";
        command.Parameters.AddWithValue("@version", Migration);
        var migrationApplied = (long)(await command.ExecuteScalarAsync() ?? 0L) > 0;

        if (!migrationApplied)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu migration {Migration}. " +
                $"Dừng backend, sao lưu CSDL, chạy {MigrationFile} trên đúng CSDL rồi khởi động lại. " +
                (missing.Count > 0 ? $"Thành phần còn thiếu: {string.Join(", ", missing)}." :
                    "Các bảng đã có nhưng chưa có dấu phiên bản; kiểm tra schema trước khi chạy lại migration."));

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' đã ghi migration {Migration} " +
                $"nhưng thiếu: {string.Join(", ", missing)}. Kiểm tra schema và khôi phục từ bản sao lưu.");

        command.Parameters.Clear();
        command.Parameters.AddWithValue("@version", KtvMigration);
        var ktvApplied = (long)(await command.ExecuteScalarAsync() ?? 0L) > 0;
        if (!ktvApplied)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu migration {KtvMigration}. " +
                $"Dừng backend, sao lưu CSDL, chạy {KtvMigrationFile} trên đúng CSDL rồi khởi động lại. " +
                (missingKtv.Count > 0 ? $"Thành phần còn thiếu: {string.Join(", ", missingKtv)}." :
                    "Các cột đã có nhưng chưa có dấu phiên bản; kiểm tra schema trước khi chạy lại migration."));
        if (missingKtv.Count > 0)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' đã ghi migration {KtvMigration} " +
                $"nhưng thiếu: {string.Join(", ", missingKtv)}. Kiểm tra schema và khôi phục từ bản sao lưu.");
    }
}
