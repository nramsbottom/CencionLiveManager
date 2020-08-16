using System;
using System.Data;
using System.Collections;

namespace Cencion.SwitchDecoder.Sdx
{

	public class LineStateManager
	{
		private Hashtable lineData = new Hashtable();

		public object this[short lineNumber, string attributeName]
		{
			get 
			{ 
				string key  = Convert.ToString(lineNumber);
					   key += "-" + attributeName;

				// format of key is <linenumber>-<attributename> (e.g. 3007-ddi)
				return this.lineData[ key ];
			}

			set 
			{
				// build key here cause casting shorts to a string just doesn't want to work inline!
				string key  = Convert.ToString(lineNumber);
					   key += "-" + attributeName;

				/*
				if ((lineNumber == 3654) && (attributeName == "ddi"))
					System.Diagnostics.Debug.Assert(false, "here!");

				Console.WriteLine("Line {0} Property {1} set to {2}", lineNumber, key, value);
				*/

				this.lineData[ key ] = value;
			}
		}


		public void Clear(short lineNumber)
		{
			ArrayList keyList = new ArrayList();
			string[] keys;
			int i;
			string ln = lineNumber.ToString();

			// scan through all state data
			foreach (string key in lineData.Keys)
			{
				// find state data where the linenumber starts at char index 0
				if ( key.IndexOf(ln) == 0 )
				{
					// add this state item to the list of items to be cleared
					keyList.Add(key);
				}
			}
			
			// get a string array of all the keys to be purged
			keys = (string[])keyList.ToArray(typeof(string));

			for (i=0;i<keys.Length;i++)
			{
				// nullify the data. this is done instead of deleting
				// for efficency
				lineData[ (string)keys[i] ] = null;
			}

		}

		/*
		void Dump()
		{
			// find state data where the linenumber starts at char index 0
			if ( key.IndexOf(ln) == 0 )
			{
				// add this state item to the list of items to be cleared
				Console.WriteLine("Key: {0}\t\tValue: {1}", key, lineData[key]);
			}
		}
		*/

	} // end class


} // end namespace
