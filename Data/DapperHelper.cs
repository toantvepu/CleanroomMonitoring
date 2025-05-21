using Dapper;

using System.Data;
using Microsoft.Data.SqlClient;

namespace CleanroomMonitoring.Web.Data
{
    public class DapperHelper : IDisposable
    {
        private readonly string _connectionString;

        public DapperHelper(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Tạo và trả về một kết nối SQL mới sử dụng chuỗi kết nối
        /// </summary>
        /// <returns></returns>
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
        /// <summary>
        /// Thực hiện truy vấn SQL và trả về danh sách đối tượng
        /// var users = await dapperHelper.QueryAsync<User>("SELECT * FROM Users WHERE Age > @MinAge", new { MinAge = 18 });
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">Hỗ trợ truyền tham số (optional)</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection()) {
                return await connection.QueryAsync<T>(sql, parameters);
            }
        }
        /// <summary>
        /// Truy vấn và trả về một đối tượng đầu tiên hoặc null
        /// var user = await dapperHelper.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @UserId", new { UserId = 1 });
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection()) {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
            }
        }
        /// <summary>
        /// Thực thi các câu lệnh INSERT, UPDATE, DELETE
        /// Trả về số dòng bị ảnh hưởng
        /// int rowsAffected = await dapperHelper.ExecuteAsync("DELETE FROM Users WHERE Age < @MinAge", new { MinAge = 18 });
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            using (var connection = CreateConnection()) {
                return await connection.ExecuteAsync(sql, parameters);
            }
        }
        /// <summary>
        /// Thực thi stored procedure và trả về kết quả
        /// var results = await dapperHelper.ExecuteStoredProcedureAsync<Report>("GetMonthlyReport", new { Month = 12, Year = 2024 });
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, object parameters = null)
        {
            using (var connection = CreateConnection()) {
                return await connection.QueryAsync<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void Dispose()
        {
            // Implement IDisposable if needed
        }
    }
}
