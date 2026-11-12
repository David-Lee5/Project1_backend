using Microsoft.Data.SqlClient;

namespace RepairSystem.Infrastructure;

internal static class DatabaseSchemaValidator
{
    private const string Migration = "V3_20261106_API";
    private const string MigrationFile = "database_v3/BO_SUNG_API_20261106.sql";
    private const string KtvMigration = "V3_20261107_KTV";
    private const string KtvMigrationFile = "database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql";
    private const string RulesMigration = "V3_20261108_RULES";
    private const string RulesMigrationFile = "database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql";
    private const string WorkflowMigration = "V3_20261108_WORKFLOW";
    private const string WorkflowMigrationFile = "database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql";
    private const string TableRenameFile = "database/DOI_TEN_BANG_BO_V3_20261111.sql";

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
                   OBJECT_ID(N'dbo.ma_lien_ket_khach_hang', N'U'),
                   OBJECT_ID(N'dbo.lich_hen', N'U'),
                   OBJECT_ID(N'dbo.xu_ly_bao_hanh_ncc', N'U'),
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
        if (reader.IsDBNull(2)) missing.Add("dbo.ma_lien_ket_khach_hang");
        if (reader.IsDBNull(3)) missing.Add("dbo.lich_hen");
        if (reader.IsDBNull(4)) missing.Add("dbo.xu_ly_bao_hanh_ncc");
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

        command.Parameters.Clear();
        command.Parameters.AddWithValue("@version", RulesMigration);
        var rulesApplied = (long)(await command.ExecuteScalarAsync() ?? 0L) > 0;
        if (!rulesApplied)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu migration {RulesMigration}. " +
                $"Dừng backend, sao lưu CSDL, chạy {RulesMigrationFile} trên đúng CSDL rồi khởi động lại.");

        command.Parameters.Clear();
        command.CommandText = """
            SELECT CASE WHEN COL_LENGTH(N'dbo.phieu_sua_chua', N'ly_do_khong_serial') IS NOT NULL
                AND COL_LENGTH(N'dbo.phieu_sua_chua', N'ap_dung_kiem_soat_tiep_nhan') IS NOT NULL
                AND COL_LENGTH(N'dbo.phieu_sua_chua', N'checklist_tiep_nhan_json') IS NOT NULL
                AND COL_LENGTH(N'dbo.anh_phieu_sua_chua', N'sha256') IS NOT NULL
                AND COL_LENGTH(N'dbo.anh_phieu_sua_chua', N'nguoi_tai_len_id') IS NOT NULL
                AND OBJECT_ID(N'dbo.quy_tac_van_hanh', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.xac_nhan_tiep_nhan', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.duyet_bao_hanh_lap', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.kiem_ke_kho', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.dong_kiem_ke_kho', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.bien_dong_serial_kiem_ke', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.cach_ly_vat_tu', N'U') IS NOT NULL
                THEN 1 ELSE 0 END
            """;
        if ((int)(await command.ExecuteScalarAsync() ?? 0) != 1)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' đã ghi migration {RulesMigration} " +
                "nhưng thiếu thành phần schema. Kiểm tra và khôi phục từ bản sao lưu.");

        command.CommandText = "SELECT COUNT_BIG(*) FROM dbo.v3_schema_migration WHERE version = @version";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@version", WorkflowMigration);
        if ((long)(await command.ExecuteScalarAsync() ?? 0L) == 0)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu migration {WorkflowMigration}. " +
                $"Dừng backend, sao lưu CSDL, chạy {WorkflowMigrationFile} trên đúng CSDL rồi khởi động lại.");
        command.Parameters.Clear();
        command.CommandText = """
            SELECT CASE WHEN OBJECT_ID(N'dbo.cong_viec_sua_chua', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.cong_viec_vat_tu', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.bao_hanh_phan_tich', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.ho_so_thanh_ly', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.tai_lieu_thanh_ly', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.hoa_don_dau_vao', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.hoa_don_dien_tu', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.dieu_chinh_hoa_don', N'U') IS NOT NULL
                THEN 1 ELSE 0 END
            """;
        if ((int)(await command.ExecuteScalarAsync() ?? 0) != 1)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' đã ghi migration {WorkflowMigration} " +
                "nhưng thiếu thành phần schema. Kiểm tra và khôi phục từ bản sao lưu.");
        command.CommandText = """
            SELECT COUNT_BIG(*)
            FROM sys.tables
            WHERE schema_id = SCHEMA_ID(N'dbo')
              AND name IN (
                  N'yeu_cau_linh_kien_v3', N'chi_tiet_yeu_cau_linh_kien_v3',
                  N'tra_linh_kien_kho_v3', N'chi_tiet_hoa_don_v3', N'bi_mat_thiet_bi_v3',
                  N'ma_lien_ket_khach_hang_v3', N'lich_hen_v3', N'xu_ly_bao_hanh_ncc_v3',
                  N'quy_tac_van_hanh_v3', N'xac_nhan_tiep_nhan_v3',
                  N'duyet_bao_hanh_lap_v3', N'kiem_ke_kho_v3',
                  N'dong_kiem_ke_kho_v3', N'bien_dong_serial_kiem_ke_v3',
                  N'cach_ly_vat_tu_v3', N'cong_viec_sua_chua_v3',
                  N'cong_viec_vat_tu_v3', N'bao_hanh_phan_tich_v3',
                  N'ho_so_thanh_ly_v3', N'tai_lieu_thanh_ly_v3',
                  N'hoa_don_dau_vao_v3', N'hoa_don_dien_tu_v3',
                  N'dieu_chinh_hoa_don_v3');
            """;
        var oldTableCount = (long)(await command.ExecuteScalarAsync() ?? 0L);
        if (oldTableCount > 0)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' còn {oldTableCount} bảng hậu tố _v3. " +
                $"Dừng backend, sao lưu CSDL, chạy {TableRenameFile} trên đúng CSDL rồi khởi động lại.");

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu: {string.Join(", ", missing)}. " +
                "Kiểm tra schema và khôi phục từ bản sao lưu.");

        command.CommandText = """
            SELECT COUNT_BIG(*)
            FROM sys.tables
            WHERE schema_id = SCHEMA_ID(N'dbo')
              AND name IN (
                  N'yeu_cau_linh_kien', N'chi_tiet_yeu_cau_linh_kien',
                  N'tra_linh_kien_kho', N'chi_tiet_hoa_don', N'bi_mat_thiet_bi');
            """;
        var requiredTableCount = (long)(await command.ExecuteScalarAsync() ?? 0L);
        if (requiredTableCount != 5)
            throw new InvalidOperationException(
                $"CSDL '{databaseName}' trên '{settings.DataSource}' thiếu bảng nghiệp vụ bắt buộc. " +
                "Kiểm tra schema và khôi phục từ bản sao lưu.");
    }
}
