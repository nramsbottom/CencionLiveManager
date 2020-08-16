using System;

namespace Cencion.SwitchDecoder.Sdx
{
	/// <summary>
	/// Summary description for SwitchMessageReceivedEventArgs.
	/// </summary>
	public class MessageReceivedEventArgs : EventArgs
	{
		private MessageType _type;
		private string _message;

		public MessageReceivedEventArgs ( MessageType type, string message )
		{
			this._type = type;
			this._message = message;
		}

		public MessageType Type
		{
			get { return this._type; }
		}

		public string Message
		{
			get { return this._message; }
		}

	}
}
