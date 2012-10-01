using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Instr : AsmLine
		{
			protected Operand op1 = null, op2 = null, op3 = null;
			protected Com command = null;

			public Instr(Com com, Operand op1 = null, Operand op2 = null, Operand op3 = null)
			{
				this.command = com;
				this.op1 = op1;
				this.op2 = op2;
				this.op3 = op3;
			}


			public override string ToString()
			{
				string res = this.command.ToString();
				if (this.op1 != null)
				{
					res += " " + this.op1.ToString();
				}

				if (this.op2 != null)
				{
					res += ", " + this.op2.ToString();
				}

				if (this.op3 != null)
				{
					res += ", " + this.op3.ToString();
				}

				return res;
			}
		}
	}
}
