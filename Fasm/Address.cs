using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{

	partial class CodeGen
	{
		protected class Addr : Operand
		{
			Operand addr = null;
			Reg reg = null;
			string variable = null;
			int offset = 0;
			bool cast = false;

			public Addr(Addr addr)
			{
				this.reg = addr.reg;
				this.offset = addr.offset;
				this.addr = addr.addr;
				this.size = addr.size;
				this.cast = addr.cast;
				this.variable = addr.variable;
			}

			public Addr(string var, int size = 4)
			{
				this.variable = var;
				this.size = size;
			}

			public Addr(Val address)
			{
				this.addr = address;
				this.size = address.size;
			}

			public Addr(Reg reg, int size = 4, bool cast = false)
			{
				this.reg = reg;
				this.size = size;
				this.cast = cast;
			}

			public static Addr operator +(Addr addr, int offset)
			{
				Addr res = new Addr(addr);
				res.offset += offset;
				return res;
			}

			public static Addr operator -(Addr addr, int offset)
			{
				Addr res = new Addr(addr);
				res.offset -= offset;
				return res;
			}

			public override string ToString()
			{
				string res = "";
				if (reg != null) { res = reg.ToString(); }
				else if (addr != null) { res = addr.ToString(); }
				else if (variable != null) { res = variable; }
				if (this.offset != 0)
				{
					res += " + " + this.offset;
				}

				if (this.cast)
				{
					res = Type.GetType(this.size).ToString() + "[" + res + "]";
				}
				return res;
			}
		}
	}
}
