﻿using MySql.Data.MySqlClient;
using Pchp.Core;
using Pchp.Library.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using Pchp.Library.Resources;

namespace Peachpie.Library.MySql
{
    /// <summary>
    /// 
    /// </summary>
    [PhpHidden]
    sealed class MySqlConnectionResource : ConnectionResource
    {
        readonly MySqlConnectionManager _manager;
        readonly MySqlConnection _connection;

        public MySqlConnectionResource(MySqlConnectionManager manager, string connectionString) : base(connectionString, "mysql connection")
        {
            _manager = manager;
            _connection = new MySqlConnection(this.ConnectionString);
        }

        protected override void FreeManaged()
        {
            base.FreeManaged();
            _manager.RemoveConnection(this);
        }

        public override void ClosePendingReader()
        {
            var myreader = (MySqlDataReader)_pendingReader;
            if (myreader != null)
            {
                myreader.Close();   // we have to call Close() on MySqlDataReader, it is declared as non-virtual!
                _pendingReader = myreader = null;
            }
        }

        protected override IDbConnection ActiveConnection => _connection;

        protected override ResultResource GetResult(ConnectionResource connection, IDataReader reader, bool convertTypes)
        {
            return new MySqlResultResource(connection, reader, convertTypes);
        }

        protected override IDbCommand CreateCommand(string commandText, CommandType commandType)
        {
            return new MySqlCommand()
            {
                Connection = _connection,
                CommandText = commandText,
                CommandType = commandType
            };
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        internal string ServerVersion => _connection.ServerVersion;

        /// <summary>
        /// Returns the id of the server thread this connection is executing on.
        /// </summary>
        internal int ServerThread => _connection.ServerThread;

        /// <summary>
        /// Pings the server.
        /// </summary>
        internal bool Ping()
        {
            return _connection.Ping();
        }

        /// <summary>
		/// Queries server for a value of a global variable.
		/// </summary>
		/// <param name="name">Global variable name.</param>
		/// <returns>Global variable value (converted).</returns>
		internal object QueryGlobalVariable(string name)
        {
            // TODO: better query:

            var result = ExecuteQuery("SHOW GLOBAL VARIABLES LIKE '" + name + "'", true);

            // default value
            if (result.FieldCount != 2 || result.RowCount != 1)
            {
                return null;
            }

            return result.GetFieldValue(0, 1);
        }
    }
}
