using System;

namespace Cencion.SwitchDecoder.Applications
{
	/// <summary>
	/// Summary description for ConsoleTraceListener.
	/// </summary>
	public class ConsoleTraceListener : System.Diagnostics.TraceListener
	{
		public ConsoleTraceListener()
		{
			//
			//
		}

		public override void Write ( string text )
		{
			System.Console.Out.Write( text );
		}

		public override void WriteLine( string text )
		{
			System.Console.Out.WriteLine( text );
		}
	}
}
