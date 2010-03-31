/*
The MIT License

Copyright (c) 2010 Pixel Lab

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Data.SqlClient;

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
            SqlCommand command = new SqlCommand();

            Util.RequireNotNullOrEmpty(commandFormatString, "commandFormatString");
            Util.RequireNotNull(parameters, "parameters");

            string[] paramLabels = new string[parameters.Length];
            for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
            {
                paramLabels[paramIndex] = string.Format("@{0}", paramIndex);
                command.Parameters.AddWithValue(paramLabels[paramIndex], parameters[paramIndex]);
            }

            command.CommandText = string.Format(commandFormatString, paramLabels);
            return command;

        } //*** createSqlCommandWithParameters

    } //*** class SqlUtil

} //*** PixelLab.Common