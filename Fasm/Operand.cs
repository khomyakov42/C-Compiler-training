using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Operand
		{
			public int size = 0;

			public Operand() { }

			public Operand(int size)
			{
				this.size = size;
			}

			public static Operand operator +(Operand op1, Operand op2)
			{
				return new Operand(op1.size + op2.size);
			}

			public static Operand operator -(Operand op1, Operand op2)
			{
				return new Operand(op1.size - op2.size);
			}
		}
	}
}
