using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	public class Logger
	{
		List<System.Exception> list;

		public Logger()
		{
			this.list = new List<System.Exception>();
		}

		public void Add(System.Exception e)
		{
			if (e != null)
			{
				list.Add(e);
			}
		}

		override public string ToString()
		{
			string s = "";
			foreach (var e in this.list)
			{
				s += e.Message + '\n';
			}

			return s;
		}

		public bool isEmpty()
		{
			return list.Count == 0;
		}
	}
}
