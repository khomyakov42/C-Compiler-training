using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		class Lab : AsmLine
		{
			string name = "";

			public Lab(string name)
			{
				this.name = name;
			}

			public override string ToString()
			{
				return this.name + ":";
			}
		}
	}
}
