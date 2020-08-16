using System;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{
	/// <summary>
	/// Logs data to a SQL Server™ table
	/// </summary>
	public class DatabaseEventLogger : ISdxLogger
	{

		static TraceSwitch logLevel = new TraceSwitch("LogLevel", string.Empty); // logging level for this application

		private Parser _parser;
		private string _connectionString;

		private LineStateManager lineState = new LineStateManager(); // manages line data for the class

		public DatabaseEventLogger(string connectionString, Parser parser)
		{
			//
			//
			this._connectionString = connectionString;
			this._parser = parser;

			this._parser.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);

		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{

			SqlConnection	conn = null;
			SqlCommand		cmd = null;
			Queue			cmdQueue = new Queue();
			short			lineNumber;
			string			stdOutMsg = string.Empty;

			// ignore null event args
			if ( e == null )
				return;

			// ignore null message payload
			if ( e.Message == null )
				return;

			// ignore smdr messages
			if (e.Type == Sdx.MessageType.Smdr)
				return;

			// TODO: ensure that e.Type is not invalid
			// TODO: add an email function that will notify when an exception is thrown but not explicitly handled!


			// catch any blank messages
			if (e.Message.Length > 0)
			{
				if ( e.Message.Length >= 5 )
				{
					string lineNumberBuffer = e.Message.Substring(1, 4);
					
					// verify that the line number for this message is valid
					if ( !IsValidLineNumber( lineNumberBuffer ) )
					{
						stdOutMsg = string.Format("Invalid line number '{0}' (EVENT: {1})", lineNumberBuffer, e.Message);
					}
					else
					{

						lineNumber = Convert.ToInt16(lineNumberBuffer);

						try
						{
							// determine the message type and setup and database commands to execute
							// for this message.
							switch ( e.Type )
							{

								#region Sdx.MessageType.Cli

								case Sdx.MessageType.Cli:
								{
									this.lineState[ lineNumber, "cli" ] = e.Message.Substring(5);
								}
								break;

								#endregion


								#region Sdx.MessageType.ExtensionHangup

								case Sdx.MessageType.ExtensionHangup:
								{
									this.lineState.Clear( lineNumber );

									stdOutMsg += string.Format("Clearing extension {0}\n", lineNumber);

								}
								break;

								#endregion


								#region Sdx.MessageType.LineClosed

								case Sdx.MessageType.LineClosed:
								{

									string lineDirection = (string)this.lineState[lineNumber, "direction"];

									// if the line is outbound, start processing
									if ( lineDirection == "out" )
									{
										stdOutMsg += "Here comes the science bit...\n";

										// string dtconnected = (string)this.lineState[lineNumber, "connected"];

										string	dialledNumber	= (string)this.lineState[lineNumber, "diallednumber"];
										string	cli				= (string)this.lineState[lineNumber, "inboundcli"];
										//string	ddi				= (string)this.lineState[Convert.ToInt16(this.lineState[lineNumber, "connectedto"]),  "ddi"];
										string	ddi				= (string)this.lineState[lineNumber,  "ddi"];
										int		duration		= 0;
										string	dtlogged		= (string)this.lineState[lineNumber, "connected"];

										Console.WriteLine("Line {0} closing. Connected to {1} with ddi {2}", lineNumber, this.lineState[lineNumber, "connectedto"], this.lineState[Convert.ToInt16(this.lineState[lineNumber, "connectedto"]), "ddi"]);

										if (dtlogged == null)
										{
											stdOutMsg += string.Format("{0} didn't connect.\n", lineNumber);
											return;
										}

										// gets the date and time that the line connected (dd-MMM-yyyy HH:mm:ss)
										DateTime callConnected = DateTime.Parse( dtlogged );

										// subtracts it from 'now', leaving us with the difference
										TimeSpan callDuration = DateTime.Now.Subtract( callConnected );

										// round down (to the nearest second) and only store the integral portion of the number of seconds in the difference
										// this.lineState[lineNumber, "duration"] = Math.Abs( Math.Floor( duration.TotalSeconds ) );
										duration = (int)Math.Abs( Math.Floor( callDuration.TotalSeconds ) );

										//string inboundDdi = (string)this.lineState[lineNumber, "inboundcli"];

										stdOutMsg += "Connecting to database...\n";

										cmd = new SqlCommand();

										cmd.CommandText = "sp_LM_LogOutboundCall";
										cmd.CommandType = CommandType.StoredProcedure;
							
										cmd.Parameters.Add ( new SqlParameter( "@dialledNumber", dialledNumber ) );
										
										// the cli can potentially be null if the it is withheld
										// but it is usually 'X' if withheld
										if ( cli != null )
											cmd.Parameters.Add ( new SqlParameter( "@cli", cli ) );
										else
											cmd.Parameters.Add ( new SqlParameter( "@cli", DBNull.Value ) );

										// the ddi will be null if there is no inbound leg
										// as is the case with outbound 'admin' (from an extension) calls
										// that don't have any inbound call associated with it
										if ( ddi != null )
											cmd.Parameters.Add ( new SqlParameter( "@ddi", ddi ) );
										else
											cmd.Parameters.Add ( new SqlParameter( "@ddi", DBNull.Value ) );

										cmd.Parameters.Add ( new SqlParameter( "@dtlogged", dtlogged ) );
										cmd.Parameters.Add ( new SqlParameter( "@duration", duration ) );
										
										
										/*
										 * 
										if ( inboundDdi != null )
											cmd.Parameters.Add ( new SqlParameter( "@ddi", inboundDdi) );
										else
											cmd.Parameters.Add ( new SqlParameter( "@ddi", DBNull.Value) );
										
										// ignore internationals
										if (dialledNumber.Substring(0, 3) != "900")
											System.Diagnostics.Debug.Assert( dialledNumber.Length == 12 | dialledNumber.Length == 8 | dialledNumber.Length == 6, "Invalid diallednumber!"); // 12 digits because of 9 prefix
										
										cmd.Parameters.Add ( new SqlParameter( "@dialledNumber", dialledNumber ) );
										cmd.Parameters.Add ( new SqlParameter( "@dtlogged", dtlogged ) );
										cmd.Parameters.Add ( new SqlParameter( "@duration", duration ) );

										*/

										cmdQueue.Enqueue( cmd );

										cmd = new System.Data.SqlClient.SqlCommand();

										cmd.CommandType = CommandType.StoredProcedure;
										cmd.CommandText = "sp_LM_SetReaderIdleByTel";

										//cmd.Parameters.Add ( new SqlParameter("@tel", this.lineState[lineNumber, "diallednumber"]) );
										cmd.Parameters.Add ( new SqlParameter("@tel", dialledNumber ) );

										cmdQueue.Enqueue( cmd );

										stdOutMsg += string.Format("Logging call...\n");

										//print_r(lineNumber);
										stdOutMsg += string.Format("Line {0} closed.\n", lineNumber);

									}

									this.lineState.Clear( lineNumber );

								} // end processing line close event
									break;

								#endregion


								#region Sdx.MessageType.Connected

								case Sdx.MessageType.Connected:
								{									
									short	lineNumber2;
									string	lineDirection = (string)this.lineState[lineNumber ,"direction"];

									// grab the second line number
									lineNumberBuffer = e.Message.Substring(5);

									if ( IsValidLineNumber(lineNumberBuffer) )
									{
										lineNumber2 = Convert.ToInt16(lineNumberBuffer);
 
										this.lineState[lineNumber, "connectedto"] = lineNumber2;
										this.lineState[lineNumber, "connected"] = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");

										this.lineState[lineNumber2, "connectedto"] = lineNumber;

										// check to see if this line is designated as an outbound.
										// this will have been set is an E event is received for this line.
										if ( lineDirection == "out" )
										{
											//lineData[lineNumber + "-diallednumber"] = lineData[lineData[lineNumber + "-connectedto"] + "-diallednumber"];
											stdOutMsg += string.Format("Outbound line {0} connected to {1}\n", lineNumber, lineNumber2);
										}

									}
									else
									{
										// invalid second line number
										stdOutMsg += string.Format("Invalid line number - {0}", lineNumberBuffer);
									}

								}
								break;

								#endregion


								#region Sdx.MessageType.LineOpened

								case Sdx.MessageType.LineOpened:
								{
									this.lineState[lineNumber, "direction"] = "out";

									stdOutMsg += string.Format("New outbound line {0}\n", lineNumber);
								}
								break;

								#endregion


								#region Sdx.MessageType.Digit

								case Sdx.MessageType.Digit:
								{
									this.lineState[lineNumber, "diallednumber"] += e.Message.Substring(5);
								}
									break;

								#endregion


								#region Sdx.MessageType.Ddi

								case Sdx.MessageType.Ddi:
								{
									Console.WriteLine("Line {0} has a ddi of {1}",  lineNumber, e.Message.Substring(5) );
									this.lineState[ lineNumber, "ddi" ] = e.Message.Substring(5);
								}
									break;

									#endregion


								#region Sdx.MessageType.RemoteConnect

								case Sdx.MessageType.RemoteConnect:
								{
									short lineNumber2;
									string lineDirection = (string)this.lineState[lineNumber, "direction"];

									lineNumberBuffer = e.Message.Substring(5);

									if ( IsValidLineNumber(lineNumberBuffer) )
									{
										lineNumber2 = Convert.ToInt16(lineNumberBuffer);

										stdOutMsg = "Detected remote connect.\n";

										if ( lineDirection == "out" )
										{
											//this.lineState[lineNumber, "inboundddi"] = this.lineState[Convert.ToInt16(this.lineState[lineNumber2, "connectedto"]),"ddi"];
											this.lineState[lineNumber, "ddi"] = this.lineState[Convert.ToInt16(this.lineState[lineNumber2, "connectedto"]),"ddi"];

											System.Console.WriteLine("DDI:{0}", this.lineState[lineNumber, "inboundddi"]);

											if ( this.lineState[lineNumber2, "diallednumber"] != null )
											{
												// copy the number that the second line dialled to the current line
												this.lineState[lineNumber, "diallednumber"] = this.lineState[lineNumber2, "diallednumber"];
											}
											else
												stdOutMsg += string.Format("The dialled number is null!\n");

											// this is the outbound trunk connect time
											this.lineState[lineNumber, "connected"] = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
											
											// NEW
											if ( this.lineState[lineNumber2, "connectedto"] != null )
											{
												this.lineState[lineNumber, "inboundcli"] = this.lineState[Convert.ToInt16(this.lineState[lineNumber2, "connectedto"]),"cli"];
											}
											else
												this.lineState[lineNumber, "inboundcli"] = Convert.ToString(lineNumber2);

											stdOutMsg += string.Format("Outbound line {0} ({1}) connected to {2}\n", lineNumber, this.lineState[lineNumber, "diallednumber"], lineNumber2); 

											cmd = new SqlCommand();

											cmd.CommandType = CommandType.StoredProcedure;
											cmd.CommandText = "sp_LM_SetReaderBusyByTel";
							
											stdOutMsg += string.Format("sp_LM_SetReaderBusyByTel: @tel = {0}\n", this.lineState[lineNumber, "diallednumber"]);

											cmd.Parameters.Add ( new SqlParameter("@tel", this.lineState[lineNumber, "diallednumber"]) );

											cmdQueue.Enqueue ( cmd );

										}

									}
									else
									{
										stdOutMsg = string.Format("Invalid line number - {0}", lineNumberBuffer);
									}
								} // end remoteconnect event processing
									break;

								#endregion


								default:
								{
									// do nothing
								}
								break;

							} // end switch

							try
							{

								// are there any commands to run?
								if ( cmdQueue.Count > 0)
								{
									// yes, open a connection to the database server
									conn = new SqlConnection( this._connectionString );
									conn.Open();

									// begin dequeing event commands
									while ( cmdQueue.Count > 0 )
									{
										// dequeue command
										cmd = (SqlCommand)cmdQueue.Dequeue();

										// verify command
										if ( cmd != null )
										{
											cmd.Connection = conn;

											// execute command
											cmd.ExecuteNonQuery();
										}
									}

								}
							}
							catch (SqlException ex)
							{
								if (logLevel.TraceError)
									System.Diagnostics.Trace.WriteLine(ex.ToString());
							}
							finally
							{
								if ( conn != null ) // cleanup
									conn.Close();
							}

						}
						catch (Exception ex)
						{
							if (logLevel.TraceError)
								System.Diagnostics.Trace.WriteLine(ex.ToString());
						}
						finally
						{
							if ( conn != null ) // cleanup - finally block in 
								conn.Close();
						}
					}
					
					if (logLevel.TraceVerbose)
					{
						if ( stdOutMsg.Length > 0 )
							System.Diagnostics.Trace.Write ( stdOutMsg );
					}
				}
			} 
		}

		/// <summary>
		/// Determines if the specified line number is a valid number.
		/// </summary>
		/// <param name="lineNumber">The string containing the line number to be tested.</param>
		/// <returns>True is number is valid; False otherwise.</returns>
		private bool IsValidLineNumber(string lineNumber)
		{
			
			short shLineNumber;
			bool result;

			try
			{
				shLineNumber = Convert.ToInt16( lineNumber );
				result = true;
			}
			catch (Exception ex)
			{
				if (logLevel.TraceVerbose)
					System.Diagnostics.Trace.Write("Invalid line number - {0}", lineNumber);

				result = false;
			}

			return result;

		}


	} // end class

} // end namespace
