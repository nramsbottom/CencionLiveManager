using System;

namespace Cencion.SwitchDecoder.Sdx
{
	public class DisconnectedEventArgs : EventArgs
	{

		private DisconnectedReason _reason;

		public DisconnectedEventArgs ( DisconnectedReason reason )
		{
			this._reason = reason;
		}

		public DisconnectedReason Reason
		{
			get { return this._reason; }
		}

	} // end class
} // end namespace
