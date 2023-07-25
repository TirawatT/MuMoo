using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;

namespace MuMoo.Utilities
{
    public static class LinqPadExtensions
    {
        private static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string>
                                                                       {
                                                                           {typeof (int), "int"},
                                                                           {typeof (short), "short"},
                                                                           {typeof (byte), "byte"},
                                                                           {typeof (byte[]), "byte[]"},
                                                                           {typeof (long), "long"},
                                                                           {typeof (double), "double"},
                                                                           {typeof (decimal), "decimal"},
                                                                           {typeof (float), "float"},
                                                                           {typeof (bool), "bool"},
                                                                           {typeof (string), "string"}
                                                                       };

        private static readonly HashSet<Type> NullableTypes = new HashSet<Type>
                                                              {
                                                                  typeof (int),
                                                                  typeof (short),
                                                                  typeof (long),
                                                                  typeof (double),
                                                                  typeof (decimal),
                                                                  typeof (float),
                                                                  typeof (bool),
                                                                  typeof (DateTime)
                                                              };

        public static string DumpClass(this IDbConnection connection, string sql, string className, string caseString)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();

            var builder = new StringBuilder();
            do
            {
                if (reader.FieldCount <= 1) continue;

                builder.AppendLine("public class " + className.ConvertCase(caseString));
                builder.AppendLine("{");
                var schema = reader.GetSchemaTable();

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        var type = (Type)row["DataType"];
                        var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;
                        var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
                        var collumnName = ((string)row["ColumnName"]).ConvertCase(caseString);

                        builder.AppendLine(string.Format("\tpublic {0}{1} {2} {{ get; set; }}", name,
                            isNullable ? "?" : string.Empty, collumnName));
                    }
                }

                builder.AppendLine("}");
                builder.AppendLine();
            } while (reader.NextResult());

            return builder.ToString();
        }

        public static string DumpMapNetFramework(this IDbConnection connection, string tableName, string caseString, string connectionString, bool mapType)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select * from " + tableName;
            var reader = cmd.ExecuteReader();


            //var keys = GetKeys(connection, tableName);
            //var keys = "keys";
            var keys = new List<string>();
            if (connection is OracleConnection) keys = GetKeysOracle(connection, tableName);
            else if (connection is SqlConnection) keys = GetKeysSqlServer(connectionString, tableName);
            else if (connection is MySqlConnection) keys = GetKeysMySql(connectionString, tableName);

            var builder = new StringBuilder();
            do
            {
                if (reader.FieldCount <= 1) continue;

                var className = tableName.ToTitleCase().Replace("_", string.Empty) + "Map";

                builder.AppendFormat("public class {0} : EntityTypeConfiguration<{0}>\r\n", className);
                builder.AppendLine("{");
                builder.AppendFormat("\tpublic {0}()", className);
                builder.AppendLine("\t{");
                builder.AppendLine("\t\tHasKey(t => new { " + string.Join(", ", keys) + " });");
                builder.AppendLine(string.Format("\t\tToTable(\"{0}\");", tableName));

                var schema = reader.GetSchemaTable();
                if (schema != null)
                {
                    var i = 0;
                    foreach (DataRow row in schema.Rows)
                    {
                        var columnName = (string)row["ColumnName"];
                        var propertyName = ((string)row["ColumnName"]).ConvertCase(caseString);
                        var sqlType = reader.GetDataTypeName(i);

                        builder.Append(string.Format("\t\tbuilder.Property(t => t.{0}).HasColumnName(\"{1}\")", propertyName, columnName));

                        if (mapType)
                            builder.Append(string.Format(".HasColumnType(\"{0}\")", sqlType));

                        builder.AppendLine(";");

                        i++;
                    }
                }
                builder.AppendLine("\t}");
                builder.AppendLine("}");
                builder.AppendLine();
            } while (reader.NextResult());

            return builder.ToString();
        }

        public static string DumpMapNetCore(this IDbConnection connection, string tableName, string caseString, string connectionString, bool mapType)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select * from " + tableName;
            var reader = cmd.ExecuteReader();

            var keys = new List<string>();
            if (connection is OracleConnection) keys = GetKeysOracle(connection, tableName);
            else if (connection is SqlConnection) keys = GetKeysSqlServer(connectionString, tableName);
            else if (connection is MySqlConnection) keys = GetKeysMySql(connectionString, tableName);
            var builder = new StringBuilder();
            do
            {
                if (reader.FieldCount <= 1) continue;

                var className = tableName.ToTitleCase().Replace("_", string.Empty);
                var classNameConfig = tableName.ToTitleCase().Replace("_", string.Empty) + "Configuration";

                //builder.AppendFormat("public class {0} : EntityTypeConfiguration<{0}>\r\n", className);
                builder.AppendFormat("public class {0} : IEntityTypeConfiguration<{1}>\r\n", classNameConfig, className);
                builder.AppendLine("{");
                //builder.AppendFormat("\tpublic {0}()", className);
                builder.AppendFormat("\tpublic void Configure(EntityTypeBuilder<{0}> builder)\n", className);
                builder.AppendLine("\t{");
                builder.AppendLine(string.Format("\t\tbuilder.ToTable(\"{0}\");", tableName));
                builder.AppendLine("\t\tbuilder.HasKey(t => new { " + string.Join(", ", keys) + " });");


                var schema = reader.GetSchemaTable();
                if (schema != null)
                {
                    var i = 0;
                    foreach (DataRow row in schema.Rows)
                    {
                        var columnName = (string)row["ColumnName"];
                        var propertyName = ((string)row["ColumnName"]).ConvertCase(caseString);

                        var sqlType = reader.GetDataTypeName(i);

                        builder.Append(string.Format("\t\tbuilder.Property(t => t.{0}).HasColumnName(\"{1}\")", propertyName, columnName));

                        if (mapType)
                            builder.Append(string.Format(".HasColumnType(\"{0}\")", sqlType));

                        builder.AppendLine(";");

                        i++;
                    }
                }
                builder.AppendLine("\t}");
                builder.AppendLine("}");
                builder.AppendLine();
            } while (reader.NextResult());

            return builder.ToString();
        }

        public static string DumpInsert(this IDbConnection connection, string sql, string table)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();

            var columns = new List<string>();
            do
            {
                if (reader.FieldCount <= 1) continue;

                var schema = reader.GetSchemaTable();

                if (schema != null)
                {
                    columns.AddRange(from DataRow row in schema.Rows select (string)row["ColumnName"]);
                }
            } while (reader.NextResult());

            return string.Format("INSERT INTO {0}({1}) VALUES({2})", table, string.Join(",", columns), string.Join(",", columns.Select(x => ":" + x)));
        }
        private static List<string> GetKeysOracle(IDbConnection connection, string tableName)
        {
            var cmd = connection.CreateCommand();
            var queryOracle = string.Format(@"
SELECT DISTINCT column_name FROM all_cons_columns WHERE constraint_name = (
SELECT constraint_name FROM user_constraints
WHERE UPPER(table_name) = UPPER('{0}') AND CONSTRAINT_TYPE = 'P')
"
, tableName);
            var querySqlServer = string.Format(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND UPPER(TABLE_NAME) = UPPER('{0}') 
ORDER BY ORDINAL_POSITION ", tableName);
            var queryMySql = string.Format(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
 WHERE CONSTRAINT_NAME = 'PRIMARY' 
AND UPPER(TABLE_NAME) = UPPER('{0}') 
 ORDER BY ORDINAL_POSITION ", tableName);

            if (connection is OracleConnection) cmd.CommandText = queryOracle;
            else if (connection is SqlConnection) cmd.CommandText = querySqlServer;
            else if (connection is MySqlConnection) cmd.CommandText = queryMySql;

            var keys = new List<string>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    keys.Add("t." + ((string)reader["column_name"]).ToTitleCase().Replace("_", string.Empty));
                }
            }

            return keys;
        }

        private static List<string> GetKeysSqlServer(string connectionString, string tableName)
        {

            var sql = string.Format(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND UPPER(TABLE_NAME) = UPPER('{0}') 
ORDER BY ORDINAL_POSITION ", tableName);


            var keys = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keys.Add("t." + ((string)reader["column_name"]).ToTitleCase().Replace("_", string.Empty));

                        }
                    }
                }
            }

            return keys;
        }
        private static List<string> GetKeysMySql(string connectionString, string tableName)
        {

            var sql = string.Format(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
 WHERE CONSTRAINT_NAME = 'PRIMARY' 
AND UPPER(TABLE_NAME) = UPPER('{0}') 
 ORDER BY ORDINAL_POSITION ", tableName);


            var keys = new List<string>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keys.Add("t." + ((string)reader["column_name"]).ToTitleCase().Replace("_", string.Empty));

                        }
                    }
                }
            }

            return keys;
        }

        private static string ConvertCase(this string str, string caseString)
        {
            var result = "";
            switch (caseString)
            {
                case "title":
                    result = str.ToTitleCase().Replace("_", string.Empty);
                    break;
                case "upper":
                    result = str.ToUpper().Replace("_", string.Empty);
                    break;
                case "lower":
                    result = str.ToLower().Replace("_", string.Empty);
                    break;
                case "default":
                case "normal":
                    result = str;
                    break;
                default:
                    result = str.ToTitleCase().Replace("_", string.Empty);
                    break;
            }
            return result;
        }
    }
}
