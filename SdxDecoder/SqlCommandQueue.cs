using System;

using System.Collections;
using System.Collections.Specialized;

using System.Data;
using System.Data.SqlClient;

namespace Cencion
{
	/// <summary>
	/// Summary description for SqlCommandQueue.
	/// </summary>
	public class SqlCommandQueue
	{
		private Queue _cmdQueue = new Queue();
		private SqlConnection _conn;

		public SqlCommandQueue()
		{
			//
			//
		}
		
		public SqlConnection Connection
		{
			get { return this._conn; }
			set { this._conn = value; }
		}

		public void Enqueue( SqlCommand cmd )
		{
			_cmdQueue.Enqueue( cmd );
		}

		public void Execute()
		{

			SqlCommand cmd;

			// are there any commands to run?
			if ( this._cmdQueue.Count > 0)
			{
				if ( this._conn.State != ConnectionState.Open )
					this._conn.Open();

				// begin dequeing event commands
				while ( this._cmdQueue.Count > 0 )
				{
					// dequeue command
					cmd = (SqlCommand)this._cmdQueue.Dequeue();
					
					// verify command
					if ( cmd != null )
					{
						cmd.Connection = this._conn;

						// execute command
						cmd.ExecuteNonQuery();
					}
				}
			}
		} // end Execute()

	} // end SqlCommandQueue

}
