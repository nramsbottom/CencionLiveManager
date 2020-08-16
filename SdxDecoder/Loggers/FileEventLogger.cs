using System;
using System.Threading;
using System.IO;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{
	/// <summary>
	/// Logs all switch output to a disk file, excluding SMDR messages.
	/// </summary>
	public class FileEventLogger : ISdxLogger
	{
		
		private Parser _parser;
		private string _filename;
		private FileStream _outputFile;
		private static StreamWriter _output;

		// this thread writes the current date and time to the file every minute
		private Thread _timeThread = new Thread( new ThreadStart(writeTime) );

		private static void writeTime()
		{
			// writes a timestamp the file with a hash symbol in front
			_output.WriteLine("# {0}", DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
			Thread.Sleep( 60 * 10000 ); // 60 seconds
		}

		public FileEventLogger(string filename, Parser parser)
		{
			//
			this._filename= filename;
			this._parser = parser;

			// open the data file
			this._outputFile = new FileStream( filename, FileMode.Append, FileAccess.Write, FileShare.Read );
			
			// allocate a reader
			_output = new StreamWriter( this._outputFile );

			// writeup events
			this._parser.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);

			// start a thread that writes the date and time to the output file every minute.
			_timeThread.Start();
		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{

			// ignore messages with a null payload
			if ( e == null )
				return;

			// ignore messages with a blank payload
			if ( e.Message == null )
				return;

			// TODO: ensure that e.Type is not invalid

			if ( e.Message.Trim().Length > 0 )
			{
				// log anything thats not smdr or a replay comment
				if ( e.Type != MessageType.Smdr && e.Type != MessageType.ReplayComment  )
				{
					_output.WriteLine( e.Message ); // write to the file
					_output.Flush(); // ensure that the data is written to the disk
				}
			}

		} // end method

	} // end class

} // end namespace
