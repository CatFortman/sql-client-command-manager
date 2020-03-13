using System.Linq;
using System.Threading.Tasks;

namespace System.Data.SqlClient.CommandManager
{
    public class SqlCommandManager
    {
        private string _connectionString;
        public string ConnectionString
        {
            get { return _connectionString; }
            protected set { _connectionString = value; }
        }

        public SqlCommandManager(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public virtual T ExecuteScalar<T>(string commandString, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SetupExecuteScalarCommand(parameters, command);
                    return (T)command.ExecuteScalar();
                }
            }
        }

        public virtual async Task<T> ExecuteScalarAsync<T>(string commandString, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SetupExecuteScalarCommand(parameters, command);
                    return (T)await command.ExecuteScalarAsync();
                }
            }
        }

        public virtual int ExecuteNonQuery(string commandString, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SqlParameter returnParam = SetupExecuteNonQueryCommand(parameters, command);

                    command.ExecuteNonQuery();

                    return (int)returnParam.Value;
                }
            }
        }

        public virtual async Task<int> ExecuteNonQueryAsync(string commandString, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SqlParameter returnParam = SetupExecuteNonQueryCommand(parameters, command);

                    await command.ExecuteNonQueryAsync();

                    return (int)returnParam.Value;
                }
            }
        }

        /// <summary>
        /// Executes a Data Reader for the parameters provided.
        /// </summary>
        public virtual T ExecuteReader<T>(string commandString, SqlParameter[] parameters, Func<SqlDataReader, T> reader)
        {
            T result;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SetupExecuteReaderCommand(parameters, command);

                    using (SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        result = reader(dataReader);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Executes a Data Reader for the parameters provided.
        /// </summary>
        public virtual async Task<T> ExecuteReaderAsync<T>(string commandString, SqlParameter[] parameters, Func<SqlDataReader, Task<T>> reader)
        {
            T result;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    SetupExecuteReaderCommand(parameters, command);

                    await connection.OpenAsync();
                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        result = await reader(dataReader);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Sets up the default fields for the Execute Reader Command.
        /// </summary>
        protected virtual void SetupExecuteScalarCommand(SqlParameter[] parameters, SqlCommand command)
        {
            command.CommandType = CommandType.Text;

            command.Parameters.AddRange(parameters.Where(x => x != null).ToArray());
        }

        /// <summary>
        /// Sets up the default fields for the Execute Reader Command.
        /// </summary>
        protected virtual void SetupExecuteReaderCommand(SqlParameter[] parameters, SqlCommand command, CommandType commandType = CommandType.Text)
        {
            command.CommandType = commandType;

            command.Parameters.AddRange(parameters.Where(x => x != null).ToArray());
        }

        /// <summary>
        /// Sets up the default fields for the Execute Non Query Command.
        /// </summary>
        protected virtual SqlParameter SetupExecuteNonQueryCommand(SqlParameter[] parameters, SqlCommand command, CommandType commandType = CommandType.Text)
        {
            command.CommandType = commandType;

            SqlParameter returnParam = new SqlParameter();
            returnParam.SqlDbType = SqlDbType.Int;
            returnParam.ParameterName = "@ReturnValue";
            returnParam.Direction = ParameterDirection.ReturnValue;

            command.Parameters.Add(returnParam);

            command.Parameters.AddRange(parameters.Where(x => x != null).ToArray());

            return returnParam;
        }
    }
}