using Microsoft.Data.SqlClient;
using MuMoo.Models.Dtos;
using MuMoo.Utilities;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
namespace MuMoo.Services
{
    public class MuMooService
    {
        private readonly IConfiguration _config;

        public MuMooService(IConfiguration config)
        {
            _config = config;
        }

        public string GetClass(string sql, string className, string caseString, string database, string connectionString)
        {
            var conn = this.GetConnection(database, connectionString);
            var result = conn.DumpClass(sql, className, caseString);
            return result;
        }
        public string GetMapping(string tableName, string caseString, string database, string connectionString, string dotNet, bool mapType)
        {
            var conn = this.GetConnection(database, connectionString);
            var result = "";
            if (dotNet.ToLower() == "framework")
                result = conn.DumpMapNetFramework(tableName, caseString, connectionString, mapType);
            else
                result = conn.DumpMapNetCore(tableName, caseString, connectionString, mapType);
            return result;
        }
        private IDbConnection GetConnection(string database, string connectionString)
        {
            IDbConnection conn = null;
            switch (database)
            {
                case "oracle":
                    conn = new OracleConnection(connectionString);
                    break;
                case "mssql":
                case "sqlserver":
                    conn = new SqlConnection(connectionString);
                    break;
                case "mysql":
                    conn = new MySqlConnection(connectionString);
                    break;
                default:
                    conn = new OracleConnection(connectionString);
                    break;
            }
            return conn;
        }
        public ParameterGuideDto GetParameterGuide()
        {
            var result = new ParameterGuideDto();
            result.caseString.Add(new ValueDesc() { value = "title", desc = "ColumnName" });
            result.caseString.Add(new ValueDesc() { value = "upper", desc = "COLUMNNAME" });
            result.caseString.Add(new ValueDesc() { value = "lower", desc = "columnname" });
            result.caseString.Add(new ValueDesc() { value = "normal", desc = "COLUMN_NAME" });
            result.caseString.Add(new ValueDesc() { value = "default", desc = "COLUMN_NAME" });

            result.database.Add(new ValueDesc() { value = "oracle", desc = "Oracle Database" });
            result.database.Add(new ValueDesc() { value = "mssql", desc = "Microsoft SQL Server Database" });
            result.database.Add(new ValueDesc() { value = "sqlserver", desc = "Microsoft SQL Server Database" });
            result.database.Add(new ValueDesc() { value = "mysql", desc = "MySql Database" });

            result.dotNet.Add(new ValueDesc() { value = "core", desc = "Dot Net Core" });
            result.dotNet.Add(new ValueDesc() { value = "framework", desc = "Dot Net Framework" });

            return result;
        }
    }
}
