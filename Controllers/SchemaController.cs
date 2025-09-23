using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemaController : ControllerBase
    {
        private readonly IConfiguration _config;
        public SchemaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("verify-decimal-columns")]
        public IActionResult VerifyDecimalColumns()
        {
            var connStr = _config.GetConnectionString("DefaultConnection");
            var tables = new[] { "Scholarships", "ScholarshipEligibilities", "StudentProfiles" };
            var columns = new[] { "MonetaryValue", "MinimumGPA", "MinGPA", "GPA" };
            var sb = new StringBuilder();
            using var conn = new SqlConnection(connStr);
            conn.Open();
            foreach (var table in tables)
            {
                sb.AppendLine($"Table: {table}");
                foreach (var col in columns)
                {
                    using var cmd = new SqlCommand($@"SELECT COLUMN_NAME, DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @col", conn);
                    cmd.Parameters.AddWithValue("@table", table);
                    cmd.Parameters.AddWithValue("@col", col);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        sb.AppendLine($"  {reader["COLUMN_NAME"]}: {reader["DATA_TYPE"]}({reader["NUMERIC_PRECISION"]},{reader["NUMERIC_SCALE"]})");
                    }
                }
            }
            return Content(sb.ToString());
        }
    }
}
