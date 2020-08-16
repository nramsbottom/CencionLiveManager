using System;
using System.IO;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{

	/// <summary>
	/// Logs SMDR SDX messages to a disk file.
	/// </summary>
	public class SmdrFileLogger : ISdxLogger
	{
		private string logFilename;
		private Parser swParser;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">The file to log messages to. The file will be created if it doesn't exist.</param>
		/// <param name="parser">The event parser to receive messages from.</param>
		public SmdrFileLogger(string filename, Parser parser)
		{
			//
			//
			this.logFilename = filename;
			this.swParser = parser;

			swParser.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);

		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			switch ( e.Type )
			{
				case Sdx.MessageType.Smdr:
				{

					// TODO: What are the chances that two event threads might try and access this file at the same time?

					FileStream fs = new FileStream( this.logFilename, FileMode.Append, FileAccess.Write, FileShare.Read );
					StreamWriter writer = new StreamWriter( fs, System.Text.Encoding.ASCII );

					writer.WriteLine( e.Message );
					writer.Flush();
					writer.Close();
				
					fs.Close();
				}
				break;

				default:
				{
					// do nothing
				}
				break;
			}
		} // end method
	} // end class
} // end namespace
