using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{

	/// <summary>
	/// Summary description for DatabaseEventLogger.
	/// </summary>
	public class PlainEnglishEventLogger : ISdxLogger
	{
		
		private Parser _parser;

		private Hashtable lineData = new Hashtable();

		public PlainEnglishEventLogger(Parser parser)
		{
			//
			//
			this._parser = parser;
			this._parser.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);

		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{

			string stdOutMsg = string.Empty;

			if ( e == null )
				return;

			if ( e.Message == null )
				return;

			// TODO: ensure that e.Type is not invalid

			// catch any blank messages
			if (e.Message.Length > 0)
			{

				string lineNumber = string.Empty;

				if ( e.Message.Length >= 5 )
				{
					lineNumber = e.Message.Substring(1, 4);
				}

				switch (e.Type)
				{

					case Sdx.MessageType.ExtensionHangup:
					{
						stdOutMsg += string.Format("Clearing extension {0}\n", lineNumber);
						clearline(lineNumber);
					}
					break;

					case Sdx.MessageType.Cli:
					{
						stdOutMsg = string.Format("Line {0} has CLI of {1}\n", lineNumber, e.Message.Substring(5));

						if ( e.Message.Substring(5).IndexOf('X') > -1 )
							stdOutMsg += " <-- WARNING: CLI contains extension!\n";

						lineData[lineNumber + "-cli"] = e.Message.Substring(5);
					}
						break;

					case Sdx.MessageType.Ddi:
					{
						stdOutMsg += string.Format("Line {0} has DDI of {1}\n", lineNumber, e.Message.Substring(5));
						lineData[lineNumber + "-ddi"] = e.Message.Substring(5);
					}
						break;

					case Sdx.MessageType.Digit:
					{
						lineData[lineNumber + "-diallednumber"] += e.Message.Substring(5);
						stdOutMsg += string.Format("{0} dialled {1}\n", lineNumber, e.Message.Substring(5));
					}
					break;

					case Sdx.MessageType.Connected:
					{
						//stdOutMsg += string.Format("Connected - {0}", e.Message);
						
						string lineNumber2 = e.Message.Substring(5);
						string lineDirection = (string)lineData[lineNumber + "-direction"];

						lineData[lineNumber + "-connectedto"] = lineNumber2;
						lineData[lineNumber + "-connected"] = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");

						if ( lineDirection == "out" )
						{
							//lineData[lineNumber + "-diallednumber"] = lineData[lineData[lineNumber + "-connectedto"] + "-diallednumber"];
							stdOutMsg += string.Format("Outbound line {0} connected to {1}\n", lineNumber, lineNumber2);
						}

					}
						break;

					case Sdx.MessageType.LineOpened:
					{
						
						lineData[lineNumber + "-direction"] = "out";

						stdOutMsg += string.Format("New outbound line {0}\n", lineNumber);
					}
						break;

					case Sdx.MessageType.RemoteConnect:
					{
						string lineNumber2 = e.Message.Substring(5);
						string lineDirection = (string)lineData[lineNumber + "-direction"];

						stdOutMsg = "Detected remote connect.\n";

						if ( lineDirection == "out" )
						{
							if ( lineData[lineNumber2 + "-diallednumber"] != null )
							{
								lineData[lineNumber + "-diallednumber"] = lineData[lineNumber2 + "-diallednumber"];
							}
							else
								stdOutMsg += string.Format("The dialled number is null!\n");

							lineData[lineNumber + "-connected"] = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
							stdOutMsg += string.Format("Outbound line {0} connected to {1}\n", lineNumber, lineNumber2); 

							System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection ("Server=CODY;Database=LiveManager;UID=sa;PWD=password;");
							System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();

							cmd.CommandType = CommandType.StoredProcedure;
							cmd.CommandText = "sp_LM_SetReaderBusyByTel";
							
							try
							{
								stdOutMsg += string.Format("sp_LM_SetReaderBusyByTel: @tel = {0}\n", lineData[lineNumber + "-diallednumber"]);

								cmd.Parameters.Add ( new SqlParameter("@tel", lineData[lineNumber + "-diallednumber"]) );

								cmd.Connection = conn;
								conn.Open();

								cmd.ExecuteNonQuery();
							}
							catch (SqlException ex)
							{
								stdOutMsg += string.Format("Database Error - {0}\n", ex.Message);
								}
							finally
							{
								conn.Close();
								conn.Dispose();
							}
						}
					}
					break;

					case Sdx.MessageType.LineClosed:
					{
						string lineDirection = (string)lineData[lineNumber + "-direction"];

						if ( lineDirection == "out" )
						{
							stdOutMsg += "Here comes the science bit...\n";

							string dtconnected = (string)lineData[lineNumber + "-connected"];
							
							if (dtconnected == null)
							{
								stdOutMsg += string.Format("{0} didn't connect.\n", lineNumber);
								return;
							}

							DateTime connected = DateTime.Parse( dtconnected );
							TimeSpan duration = DateTime.Now.Subtract(connected);

							//lineData[lineNumber + "-duration"] = duration.Seconds;
							lineData[lineNumber + "-duration"] = Math.Abs( Math.Floor( duration.TotalSeconds ) );
							
							string inboundDdi = (string)lineData[(string)lineData[lineNumber + "-connectedto"] + "-ddi"];
							string sql = "insert into calls (ddi, duration, dtlogged, direction) values ('" + lineData[lineNumber + "-diallednumber"] + "', " + lineData[lineNumber + "-duration"] + ", '" + lineData[lineNumber + "-connected"] + "',0);";

							//stdOutMsg += string.Format("Connected to.ddi = {0}", inboundDdi);

							stdOutMsg += "Connecting to database...\n";

							System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection ("Server=CODY;Database=LiveManager;UID=sa;PWD=password;");
							System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql);

							cmd.CommandText = "sp_LM_LogOutboundCall";
							cmd.CommandType = CommandType.StoredProcedure;
							
							if ( inboundDdi != null )
								cmd.Parameters.Add ( new SqlParameter( "@ddi", inboundDdi) );
							else
								cmd.Parameters.Add ( new SqlParameter( "@ddi", DBNull.Value) );

							// stdOutMsg += string.Format("Dialled number: {0}", (string)lineData[lineNumber + "-diallednumber"]);

							cmd.Parameters.Add ( new SqlParameter( "@dialledNumber", (string)lineData[lineNumber + "-diallednumber"]) );
							cmd.Parameters.Add ( new SqlParameter( "@dtlogged", (string)lineData[lineNumber + "-connected"]) );
							cmd.Parameters.Add ( new SqlParameter( "@duration", duration.TotalSeconds) );

							try
							{

								cmd.Connection = conn;
								conn.Open();

								cmd.ExecuteNonQuery();

								cmd = new System.Data.SqlClient.SqlCommand();

								cmd.CommandType = CommandType.StoredProcedure;
								cmd.CommandText = "sp_LM_SetReaderIdleByTel";
								cmd.Connection = conn;
								cmd.Parameters.Add ( new SqlParameter("@tel", lineData[lineNumber + "-diallednumber"]) );

								stdOutMsg += string.Format("Logging call...\n");
								cmd.ExecuteNonQuery();

								conn.Close();
								conn.Dispose();
							}
							catch (SqlException ex)
							{
								// stdOutMsg += string.Format(ex.ToString());
								stdOutMsg += string.Format("Logging failed. Error '{0}'\n", e.Message);
							}

							print_r(lineNumber);
							clearline(lineNumber);

							stdOutMsg += string.Format("Line {0} closed.\n", lineNumber);

						}
					}
						break;

					case Sdx.MessageType.ReplayComment:
					{
						string msg = e.Message;

						if ( e.Message.Length >= 11)
						{
							if ( e.Message.Substring(1, 11) == "BlockThread" )
							{
								System.Threading.Thread.Sleep(10000);
							}
						}
					}
					break;
				}

				Console.Write ( stdOutMsg );
			}
			
		} 

		int executeNonQuery(SqlCommand cmd)
		{
			System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection ("Server=CODY;Database=LiveManager;UID=sa;PWD=password;");
			int result = -1;

			try
			{
				conn.Open();

				cmd.Connection = conn;
			
				result = cmd.ExecuteNonQuery();
			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex.ToString());
			}
			
			return result;
		}

		string copydata(string linenumber, string attribute)
		{
			return (string)lineData[linenumber + "-" + attribute];
		}

		void clearline(string linenumber)
		{


			ArrayList keyList = new ArrayList();
			string[] keys;
			int i;

			foreach (string key in lineData.Keys)
			{
				if ( key.IndexOf(linenumber) == 0 )
				{
					keyList.Add(key);
				}
			}
			
			keys = (string[])keyList.ToArray(typeof(string));

			for (i=0;i<keys.Length;i++)
			{
				lineData[ (string)keys[i] ] = null;
			}

		}

		void print_r(string linenumber)
		{
			foreach (string key in lineData.Keys)
			{
				if ( key.IndexOf(linenumber) == 0 )
				{
					Console.WriteLine("\t[{0}]={1},", key, lineData[key]);
				}
			}
		}

	} // end class

} // end namespace
