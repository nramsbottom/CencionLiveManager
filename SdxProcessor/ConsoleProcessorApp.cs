using System;
using System.Collections;
using System.Collections.Specialized;

namespace Cencion.SwitchDecoder.Applications
{
	class ConsoleProcessorApp
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			#region Command Line Processing

			NameValueCollection arguments = ParseCommandLine();

			string host = string.Empty;
			short port = -1;
			string replayFile = arguments["r"]; // get the -r argument value
			string traceFile = arguments["t"]; // get the -r argument value
			
			// validate replay filename
			if (replayFile != null)
			{
				if (replayFile.Length > 0)
				{
					if ( !System.IO.File.Exists(replayFile) )
					{
						Usage();
						Console.WriteLine("Invalid replay file.\n\n");
						return;
					}
				}
				else
				{
					Usage();
					Console.WriteLine("Invalid replay file.\n\n");
					return;
				}
			}
			else
			{

				host = arguments["h"];

				// validate SwitchTcpReader data source port number
				try
				{
					port = Convert.ToInt16( arguments["p"] );
				}
				catch (Exception ex)
				{
					port = -1;
				}

				// validate SwitchTcpReader data source host
				if ( host != null )
				{
					if ( host.Length == 0 )
					{
						Usage();
						Console.WriteLine("Invalid host.\n\n");
						return;
					}
				}
				else
				{
					Usage();
					Console.WriteLine("Invalid host.\n\n");
					return;
				}

				if ( port <= 0 )
				{
					Usage();
					Console.WriteLine("Invalid port number.\n\n");
					return;
				}

			}

			#endregion

			Console.WriteLine( "Cencion SDX Event Processor ©2004\n\n");

			ArrayList loggers = new ArrayList();
			Cencion.SwitchDecoder.Sdx.ISwitchReader reader;
			Cencion.SwitchDecoder.Sdx.Parser parser;

			// was a replay file specified?
			if ( replayFile != null )
			{
				// yes, replay the file. replays take precedence over live data
				Console.WriteLine("Replaying file '{0}'...", replayFile);
				reader = new Cencion.SwitchDecoder.Sdx.FileReader(replayFile);
			}
			else
			{
				// no, connect to the specified host
				Console.WriteLine("Connecting to host: {0} on tcp {1}...\n\n", host, port);
				reader = new Cencion.SwitchDecoder.Sdx.TcpReader(host, port);
			}

			parser = new Cencion.SwitchDecoder.Sdx.Parser(reader);

			// setup loggers

			System.Diagnostics.Trace.Listeners.Add ( new ConsoleTraceListener() );
			
			// was a tracefile specified?
			if ( traceFile != null )
			{
				// yes, setup a FileTraceListener
				System.Diagnostics.Trace.Listeners.Add ( new FileTraceListener(traceFile) );
			}

			// define a SQL Server™ connection string for the DatabaseEventLogger 
			string dbConnectionString = string.Format("Server={0};Database={1};UID={2};PWD={3};", arguments["s"], arguments["db"], arguments["usr"], arguments["pwd"]);

			loggers.Add ( new Sdx.Loggers.DatabaseEventLogger( dbConnectionString, parser ) );
			
			// setup smdr logging
			loggers.Add ( new Sdx.Loggers.SmdrFileLogger( @"..\smdrLog.txt", parser ) );
			
			// setup everything *but* smdr logging
			loggers.Add ( new Sdx.Loggers.FileEventLogger( @"C:\smdr-cs.txt", parser ) );

			// setup console logging
			loggers.Add ( new Sdx.Loggers.ConsoleEventLogger ( parser ) );
			
			// wireup any other event handlers
			reader.Disconnected += new Sdx.DisconnectedEventHandler ( OnSwitchDisconnected );

			// begin reading data and raising events to the loggers
			reader.StartReading();

		}

		private static void Usage()
		{

			//generate usage message
			string usageMsg = @"Usage: <appname>.exe [-h <host> -p <port> -s <server>] [-r <replay filename>] -db <database> -usr <username> -pwd <password> [-t <tracefile>]";

			// emit to the console
			Console.WriteLine( usageMsg );
			Console.WriteLine("\n");
		}

		private static void OnSwitchDisconnected( object sender, Cencion.SwitchDecoder.Sdx.DisconnectedEventArgs e )
		{
			Console.WriteLine( "Disconnected from switch. Reason: {0} \n\n", e.Reason );
		}

		private static NameValueCollection ParseCommandLine()
		{
			
			int i;
			string[] cmdLineParams = Environment.GetCommandLineArgs(); // get the command line arguments for the program
			NameValueCollection cmdLineParamList = new NameValueCollection();
			string name;
			string val;

			// run through the command line arguments
			for (i=0;i<cmdLineParams.Length;i++)
			{
				// if this argument is an odd numbered element
				if (i % 2 > 0) 
				{
					name = cmdLineParams[i];

					// make sure that the argument isn't blank
					if ( name.Length > 1 )
					{
						// check to see if the argument is prefixed with a single hypen
						if ( name.Substring(0,1) == "-" )
						{
							// it is, use this as the key for an argument entry
							name = name.Substring(1);
						}
					}
 
					// ensure that we dont run off the end of the array (i.e. argument has a key, but no value)
					if (cmdLineParams.Length > i + 1)
						val = cmdLineParams[i + 1]; // set the value to be the next element after this one
					else
						val = string.Empty; // set the value to be nothing

					// use set instead of add to prevent duplicate key exceptions. causes duplicate args to be overwritten
					cmdLineParamList.Set( name, val ); 

					// skip over the next item (we dont want to process it because it's the value for *this* argument)
					i++;
				}
			}

			// return the populated NameValueCollection
			return cmdLineParamList;
		}


	} // end class
} // end namespace
