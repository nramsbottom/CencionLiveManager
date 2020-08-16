using System;
using System.IO;

namespace Cencion.SwitchDecoder.Applications
{
	/// <summary>
	/// Summary description for FileTraceListener.
	/// </summary>
	public class FileTraceListener : System.Diagnostics.TraceListener
	{
		string			_filename	= null;
		FileStream		_fs			= null;
		StreamWriter	_sw			= null;

		public FileTraceListener(string filename)
		{
			//
			//
			try
			{
				this._filename = filename;
				this._fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
				this._sw = new StreamWriter(this._fs);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("FileTraceListener unable to open disk file - {0}", ex.Message);
			}

		} // end method

		public override void Write ( string text )
		{
			try
			{
				this._sw.Write( text );
				this._sw.Flush();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("FileTraceListener unable to write to disk - {0}", ex.Message);
			}
		} // end method

		public override void WriteLine( string text )
		{
			try
			{
				this._sw.WriteLine( text );
				this._sw.Flush();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("FileTraceListener unable to write to disk - {0}", ex.Message);
			}
		} // end method

	} // end class

} // end namespace
