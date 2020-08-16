using System;

namespace Cencion.SwitchDecoder.Sdx
{

	public class DataReceivedEventArgs : EventArgs
	{
		private string _data;

		public string Data
		{
			get { return this._data; } 
		}

		public DataReceivedEventArgs ( string data )
		{
			this._data = data;
		}

	} // end class

} // end namespace
