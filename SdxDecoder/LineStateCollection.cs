using System;
using System.Collections;
using System.Collections.Specialized;

namespace Cencion.SwitchDecoder.Sdx
{
	/// <summary>
	/// Summary description for LineStateCollection.
	/// </summary>
	public class LineStateCollection : NameObjectCollectionBase 
	{
		
		public void Add ( short lineNumber, LineState state )
		{
			//this.BaseAdd ( Convert.ToString(lineNumber), state );
			this.BaseSet( Convert.ToString(lineNumber), state );
			
		}

		public void Remove ( short lineNumber )
		{
			this.BaseRemove ( Convert.ToString(lineNumber) );
		}

		public LineState this [ string lineNumber ]
		{
			get 
			{ 
				return (LineState)this.BaseGet(lineNumber); 
			}
		}

		public LineState this [ short lineNumber ]
		{
			get 
			{ 
				return (LineState)this.BaseGet( Convert.ToString(lineNumber) ); 
			}
		}

	}

}
