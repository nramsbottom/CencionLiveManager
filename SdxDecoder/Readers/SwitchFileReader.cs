using System;
using System.IO;

namespace Cencion.SwitchDecoder.Sdx
{
	/// <summary>
	/// Allows replaying of switch output logs through code.
	/// </summary>
	public class FileReader : ISwitchReader // uses common interface
	{
		public event DataReceivedEventHandler DataReceived; 
		public event DisconnectedEventHandler Disconnected; 

		private string _filename; // the input filename

		public FileReader(string filename)
		{
			//
			this._filename = filename;
		}

		/// <summary>
		/// Begins aquiring data from the data source.
		/// </summary>
		public void StartReading()
		{
			// open the file for read access
			FileStream fs = new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			
			// attach a reader
			StreamReader rdr = new StreamReader(fs);

			// continue while data is availible
			while (rdr.Peek() > -1)
			{
				// read data
				string line = rdr.ReadLine();

				// data aquired from data source
				if ( this.DataReceived != null )
					this.DataReceived ( this, new DataReceivedEventArgs(line) );

			}

			// close and cleanup
			rdr.Close();
			fs.Close();

			// signal that we are no longer connected to the data source
			if ( this.Disconnected != null )
				this.Disconnected(this, new DisconnectedEventArgs(DisconnectedReason.Shutdown) );
		}

	} // end class

} // end namespace
