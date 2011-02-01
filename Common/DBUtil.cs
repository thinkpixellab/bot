using System;
using System.Data.SqlClient;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    /// <summary>
    ///     Utility classes for dealing with databases.
    /// </summary>
    public static class DBUtil
    {
        /// <summary>
        ///     Creates a SQL command from a command format string and
        ///     parameters to avoid SQL injection attacks.
        /// </summary>
        /// <param name="commandFormatString">
        ///     SQL command string with format substitution points for
        ///     parameters.
        /// </param>
        /// <param name="parameters">Parameters to substitute into the SQL command string.</param>
        /// <returns><see cref="SqlCommand"/> using parameters.</returns>
        public static SqlCommand CreateSqlCommandWithParameters(
            string commandFormatString,
            params object[] parameters)
        {
            Contract.Requires<ArgumentNullException>(commandFormatString != null);
            Contract.Requires<ArgumentNullException>(parameters != null);

            SqlCommand command = new SqlCommand();

            string[] paramLabels = new string[parameters.Length];
            for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
            {
                paramLabels[paramIndex] = "@{0}".DoFormat(paramIndex);
                command.Parameters.AddWithValue(paramLabels[paramIndex], parameters[paramIndex]);
            }

            command.CommandText = commandFormatString.DoFormat(paramLabels);
            return command;
        } //*** createSqlCommandWithParameters
    } //*** class SqlUtil
} //*** PixelLab.Common