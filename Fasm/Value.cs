using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Val : Operand
		{
			Operand address = null;
			string value = null;

			public Val(string value, int size)
			{
				this.value = value;
				this.size = size;
			}

			public Val(int value, int size)
			{
				this.value = value.ToString();
				this.size = size;
			}

			public Val(Syntax.Const constant, int size = -1)
			{
				this.value = constant.GetValue();
				this.size = size == -1 ? constant.GetType().GetSizeType() : size;
			}

			public Val(Addr address, int size = -1)
			{
				this.address = address;
				this.size = size == -1 ? address.size : size;
			}

			public override string ToString()
			{
				if (this.value != null)
				{
					return this.value;
				}

				return Type.GetType(this.size) + "[" + this.address.ToString() + "]";
			}
		}
		
	}
}
