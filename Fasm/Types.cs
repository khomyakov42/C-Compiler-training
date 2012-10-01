using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{


		protected class Type
		{
			public static Type
				BYTE = new Type("byte", 1),
				WORD = new Type("word", 2),
				DWORD = new Type("dword", 4),
				QWORD = new Type("qword", 8),

				DB = new Type("db", 1),
				DW = new Type("dw", 2),
				DD = new Type("dd", 4),
				DQ = new Type("dq", 8);


			public int size = 0;
			protected string name = "";

			public Type(string name, int size) 
			{
				this.size = size;
				this.name = name;
			}

			public override string ToString()
			{
				return this.name;
			}

			public static Type GetType(int size)
			{
				switch (size)
				{
					case 1:
						return BYTE;
					case 2:
						return WORD;
					case 4:
						return DWORD;
					case 8:
						return QWORD;
					default:
						throw new ArgumentException("size only 1, 2, 4, 8");
				}
			}

			public static Type GetDeclType(int size)
			{
				switch (size)
				{
					case 1:
						return DB;
					case 2:
						return DW;
					case 4:
						return DD;
					case 8:
						return DQ;
					default:
						throw new ArgumentException("size only 1, 2, 4, 8");
				}
			}
		}
	}
}
