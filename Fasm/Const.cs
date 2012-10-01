using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Const
		{
			protected int size = 0;
			protected string value = "?";
			public string name = "";
			protected Type type;

			public Const(string name, Type type,string value = "?", int size = 1)
			{
				this.size = size;
				this.name = name;
				this.value = value;
				this.type = type;
			}


			public override string ToString()
			{
				return this.name + " " + this.type.ToString() + " " + this.value;
			}
		}
	}
}
