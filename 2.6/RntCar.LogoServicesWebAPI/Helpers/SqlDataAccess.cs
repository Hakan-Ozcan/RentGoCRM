using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RntCar.LogoServicesWebAPI.Helpers
{
    public class SqlDataAccess : IDisposable
    {
        /// <summary>
        /// The SQL connection
        /// </summary>
        private SqlConnection conn;

        /// <summary>
        /// To dispose only unmanaged resources 
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="SqlDataAccess"/> class.
        /// </summary>
        ~SqlDataAccess()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        /// Opens the SQL connection.
        /// </summary>
        /// <param name="connectionString">The SQL connection string.</param>
        public void OpenConnection(string connectionString)
        {
            try
            {
                this.conn = new SqlConnection(connectionString);
                this.conn.Open();
            }
            catch (SqlException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the collection of list items as a data table.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>The collection of list items as a data table.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is generated from compiled code and is not parameterized")]
        public DataTable GetDataTable(string query)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(query, this.conn);
                cmd.CommandTimeout = 60 * 3600;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                return dt;
            }
            catch (SqlException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the collection of list items as a data table with parameters.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>The collection of list items as a data table</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public DataTable GetDataTable(string query, params SqlParameter[] parameters)
        {
            DataTable resultSet = new DataTable();

            using (SqlCommand cmdMssql = new SqlCommand(query, this.conn))
            {
                try
                {
                    cmdMssql.Connection = this.conn;
                    cmdMssql.CommandTimeout = 60 * 3600;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] != null)
                        {
                            cmdMssql.Parameters.Add(parameters[i]);
                        }
                    }

                    using (SqlDataAdapter adapterMssql = new SqlDataAdapter())
                    {
                        adapterMssql.SelectCommand = cmdMssql;
                        adapterMssql.Fill(resultSet);
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Gets the collection of list items as a data table as a result of store procedure with parameters.
        /// </summary>
        /// <param name="procedureName">Name of the store procedure</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>The collection of list items as a data table.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public DataTable GetDataTableSp(string procedureName, params SqlParameter[] parameters)
        {
            DataTable resultSet = new DataTable();

            using (SqlCommand cmdMssql = new SqlCommand())
            {
                try
                {
                    cmdMssql.CommandTimeout = 60 * 3600;

                    cmdMssql.CommandType = CommandType.StoredProcedure;

                    cmdMssql.CommandText = procedureName;

                    cmdMssql.Connection = this.conn;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] != null)
                        {
                            cmdMssql.Parameters.Add(parameters[i]);
                        }
                    }

                    using (SqlDataAdapter adapterMssql = new SqlDataAdapter(cmdMssql))
                    {
                        adapterMssql.SelectCommand = cmdMssql;

                        adapterMssql.Fill(resultSet);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Gets the collection of list items as a data table as a result of store procedure.
        /// </summary>
        /// <param name="procedureName">Name of the store procedure.</param>
        /// <returns>The collection of list items as a data table.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is generated from compiled code and is not parameterized")]
        public DataTable GetDataTableSp(string procedureName)
        {
            DataTable resultSet = new DataTable();

            using (SqlCommand commandMssql = new SqlCommand())
            {
                try
                {
                    commandMssql.CommandTimeout = 60 * 3600;

                    commandMssql.CommandType = CommandType.StoredProcedure;

                    commandMssql.CommandText = procedureName;

                    commandMssql.Connection = this.conn;

                    commandMssql.Parameters.Clear();

                    using (SqlDataAdapter adapterMssql = new SqlDataAdapter(commandMssql))
                    {
                        adapterMssql.SelectCommand = commandMssql;

                        adapterMssql.Fill(resultSet);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Executes the SQL statement.
        /// </summary>
        /// <param name="query">The SQL statement.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is generated from compiled code and is not parameterized")]
        public void ExecuteNonQuery(string query)
        {
            try
            {
                using (SqlCommand cmdMssql = new SqlCommand(query, this.conn))
                {
                    cmdMssql.CommandTimeout = 60 * 3600;

                    cmdMssql.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Executes the SQL statement with parameters.
        /// </summary>
        /// <param name="query">The SQL statement.</param>
        /// <param name="parameters">The SQL parameters.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public void ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmdMssql = new SqlCommand(query, this.conn))
            {
                try
                {
                    cmdMssql.CommandTimeout = 60 * 3600;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] != null)
                        {
                            cmdMssql.Parameters.Add(parameters[i]);
                        }
                    }

                    cmdMssql.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }
            }
        }

        /// <summary>
        /// Executes store procedure with parameters.
        /// </summary>
        /// <param name="procedureName">Name of the store procedure.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public int ExecuteNonQuerySP(string procedureName, params SqlParameter[] parameters)
        {
            using (SqlCommand cmdMssql = new SqlCommand())
            {
                try
                {
                    cmdMssql.CommandTimeout = 60 * 3600;

                    cmdMssql.CommandType = CommandType.StoredProcedure;

                    cmdMssql.CommandText = procedureName;

                    cmdMssql.Connection = this.conn;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] != null)
                        {
                            cmdMssql.Parameters.Add(parameters[i]);
                        }
                    }

                    return cmdMssql.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }
            }
        }

        /// <summary>
        /// Executes the SQL query, and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is generated from compiled code and is not parameterized")]
        public object ExecuteScalar(string query)
        {
            object result = null;

            using (SqlCommand cmdMssql = new SqlCommand(query, this.conn))
            {
                try
                {
                    cmdMssql.CommandTimeout = 60 * 3600;

                    result = cmdMssql.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the SQL query with parameters, and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            object result = null;

            using (SqlCommand cmdMssql = new SqlCommand(query, this.conn))
            {
                try
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        cmdMssql.Parameters.Add(parameters[i]);
                    }

                    cmdMssql.CommandTimeout = 60 * 3600;

                    result = cmdMssql.ExecuteScalar();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }

                return result;
            }
        }

        /// <summary>
        /// Executes the store procedure with parameters, and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <param name="procedureName">Name of the store procedure.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Users are responsible for ensuring the inputs to this method are SQL Injection sanitized")]
        public object ExecuteScalarSp(string procedureName, params SqlParameter[] parameters)
        {
            object result = null;

            using (SqlCommand cmdMssql = new SqlCommand())
            {
                try
                {
                    cmdMssql.CommandTimeout = 60 * 3600;
                    cmdMssql.CommandType = CommandType.StoredProcedure;
                    cmdMssql.CommandText = procedureName;
                    cmdMssql.Connection = this.conn;
                    cmdMssql.Parameters.Clear();

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        cmdMssql.Parameters.Add(parameters[i]);
                    }

                    result = cmdMssql.ExecuteScalar();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmdMssql.Parameters.Clear();
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the collection of list items as a data set.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <returns>the collection of list items as a data set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is generated from compiled code and is not parameterized")]
        public DataSet ExecuteDataSet(string query)
        {
            DataSet resultSet = new DataSet();

            using (SqlCommand commandMSSQL = new SqlCommand(query, this.conn))
            {
                try
                {
                    using (SqlDataAdapter adapterMSSQL = new SqlDataAdapter())
                    {
                        adapterMSSQL.SelectCommand = commandMSSQL;

                        adapterMSSQL.Fill(resultSet);
                    }
                }
                catch
                {
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Closes the SQL connection.
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                this.conn.Close();
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. 
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            /*
             * Dispose(bool disposing) executes in two distinct scenarios.
                If disposing equals true, the method has been called directly
                or indirectly by a user's code. Managed and unmanaged resources
                can be disposed.
                If disposing equals false, the method has been called by the
                runtime from inside the finalizer and you should not reference
                other objects. Only unmanaged resources can be disposed.
             * */
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.conn != null)
                    {
                        this.conn.Dispose();
                        this.conn = null;
                    }
                }

                this.disposed = true;
            }
        }
    }
}