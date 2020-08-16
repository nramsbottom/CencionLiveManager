using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{
	/// <summary>
	/// Logs messages to the console.
	/// </summary>
	public class ConsoleEventLogger : ISdxLogger
	{
		
		private Parser _parser;

		public ConsoleEventLogger(Parser parser)
		{
			//
			//
			this._parser = parser;
			this._parser.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);

		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{

			// ignore null eventargs
			if ( e == null )
				return;

			// ignore messages with a null payload
			if ( e.Message == null )
				return;

			// TODO: ensure that e.Type is not invalid

			// catch any blank messages
			if (e.Message.Length > 0)
			{
				
				// write date and time to the screen
				Console.Write ( DateTime.Now.ToString("[dd-MMM-yyyy HH:mm:ss]") + " MSG: " );

				// write the message to the screen
				switch ( e.Type )
				{

					case Sdx.MessageType.Smdr:
					{
						// ignore it
						Console.WriteLine("SMDR");
					}
					break;

					default:
					{
						Console.WriteLine( e.Message );
					}
					break;
				}
			}
			
		} 

	} // end class

} // end namespace
