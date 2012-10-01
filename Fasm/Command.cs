using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Com
		{
			public static Com
				PUSH = new Com("push"),
				POP = new Com("pop"),
				FLD = new Com("fld"),
				FSTP = new Com("fstp"),
				FILD = new Com("fild"),
				FISTP = new Com("fistp"),

				SUB = new Com("sub"),
				ADD = new Com("add"),
				FADD = new Com("fadd"),
				FSUB = new Com("fsub"),

				MOV = new Com("mov"),
				LEA = new Com("lea"),
				CALL = new Com("call"),
				RET = new Com("ret")
				;


			protected string name = "";

			public Com(string name)
			{
				this.name = name;
			}

			public override string ToString()
			{
				return this.name;
			}
		}
	}
}
