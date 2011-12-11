using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
/*	class CodeGen
	{
		System.IO.StreamWriter ostream = null;
		Parser parser = null;
		StackTable.Iterator titr = null;

		public CodeGen(System.IO.StreamWriter stream, Parser parser)
		{
			this.parser = parser;
			parser.Parse();
			titr = new StackTable.Iterator(parser.tables);
			ostream = stream;
		}

		public void Generate()
		{
			ostream.Write(".386\n.mode flat, stdcall\n\n");
			string data = ".data\n", code = ".code\n";
			foreach (var v in titr.Current().vars)
			{
				if (v.Value is SymVarGlobal)
				{
					data += v.Value.GetCode() + '\n';
				}
				else
				{
					code += v.Value.GetCode() + '\n';
				} 
			}

			ostream.Write(data);
			ostream.Write(code);
			ostream.Write("end main");
		}
	}*/
}
