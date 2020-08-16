using System;
 
namespace Cencion.SwitchDecoder.Sdx
{


	public enum MessageType
	{

		// all known message types.

		Unknown,
		Smdr,
		ReplayComment,

		ExtensionAnswer,
		ExtensionHangup, // K
		LineClosed, // A
		Connected, // C
		LineOpened, // E
		ExtensionPickup, // Q
		Cli,
		Digit, // O
		Ddi,
		//ExternalCallAnswer,
		RemoteConnect, // a
		//IncomingLine,
		IncomingCallFor,
		Ringing, // M
		Park // D
	}


	

	
	/// <summary>
	/// Summary description for SwitchParser.
	/// </summary>
	public class Parser
	{
		// raised when a message is received. amazingly ;)
		public event MessageReceivedEventHandler MessageReceived;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sr">Generic reader object</param>
		public Parser(ISwitchReader sr)
		{
			
			Sdx.ISwitchReader rdr = sr; // ??? Why was this done?

			// wireup event handler
			rdr.DataReceived += new Sdx.DataReceivedEventHandler( OnDataReceived );
		}

		/// <summary>
		/// Event handler used to attach to the switch reader and generate typed
		/// message events when one is detected.
		/// </summary>
		/// <param name="sender">Not used</param>
		/// <param name="e">Data received by the reader</param>
		private void OnDataReceived( object sender, Sdx.DataReceivedEventArgs e)
		{

			// ignore events with null payload
			if (e.Data == null)
				return;

			// ignore events with a blank (there is a difference to null y'know) payload
			if (e.Data.Length == 0)
				return;

			Sdx.MessageType msgType;

			// identify what kind of message we have got based on
			// the first character of the message 
			switch ( e.Data.Substring(0, 1) )
			{
				case " ":
				{
					msgType = Sdx.MessageType.Smdr;
				}
				break;

				case "Z":
				{
					msgType = Sdx.MessageType.Cli;
				}
				break;


				case "Y":
				{
					msgType = Sdx.MessageType.Ddi;
				}
				break;

				case "M":
				{
					msgType = Sdx.MessageType.IncomingCallFor;
				}
				break;

				case "B":
				{
					//msgType = Sdx.MessageType.IncomingLine;
					msgType = Sdx.MessageType.Ringing;
				}
				break;

				case "D":
				{
					msgType = Sdx.MessageType.Park;
				}
				break;

				case "K":
				{
					msgType = Sdx.MessageType.ExtensionHangup;
				}
				break;

				case "Q":
				{
					msgType = Sdx.MessageType.ExtensionPickup;
				}
				break;

				case "E":
				{
					msgType = Sdx.MessageType.LineOpened;
				}
				break;

				case "C":
				{
					msgType = Sdx.MessageType.Connected;
				}
				break;

				case "O":
				{
					msgType = Sdx.MessageType.Digit;
				}
				break;

				case "T":
				{
					msgType = Sdx.MessageType.ExtensionAnswer;
				}
				break;

				case "A":
				{
					msgType = Sdx.MessageType.LineClosed;
				}
				break;

				case "a":
				{
					msgType = Sdx.MessageType.RemoteConnect;
				}
				break;

				case "#":
				{
					msgType = Sdx.MessageType.ReplayComment;
				}
				break;

				default:
				{
					msgType = Sdx.MessageType.Unknown;
				}
				break;

			} // end switch

			// raise an event to indicate that a message has been received
			if ( MessageReceived != null)
				MessageReceived( this, new MessageReceivedEventArgs( msgType, e.Data) );
		
		} // end method

	} // end class

} // end namespace
