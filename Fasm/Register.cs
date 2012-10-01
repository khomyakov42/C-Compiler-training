using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen
	{
		protected class Reg : Operand
		{
			public static Reg
				EAX = new Reg("eax", 4),
				EBX = new Reg("ebx", 4),
				ECX = new Reg("ecx", 4),
				EDX = new Reg("edx", 4),
				EDI = new Reg("edi", 4),
				EBI = new Reg("ebi", 4),
				EBP = new Reg("ebp", 4),
				ESP = new Reg("esp", 4),

				AH = new Reg("ah", 2),
				BH = new Reg("bh", 2),
				CH = new Reg("ch", 2),
				DH = new Reg("dh", 2),

				AL = new Reg("al", 1),
				BL = new Reg("bl", 1),
				CL = new Reg("cl", 1),
				DL = new Reg("dl", 1);
				
				

			string name = "";

			public Reg(string name, int size)
			{
				this.size = size;
				this.name = name;
			}

			public static Reg GetRegPart(Reg r, int size)
			{
				if (r == EAX || r == AL || r == AH)
				{
					if (size == 1) { return AL; }
					else if (size == 2) { return AH; }
					else if (size == 4) { return EAX; }
				}
				else if (r == EBX || r == BL || r == BH)
				{
					if (size == 1) { return BL; }
					else if (size == 2) { return BH; }
					else if (size == 4) { return EBX; }
				}
				else if (r == ECX || r == CL || r == CH)
				{
					if (size == 1) { return CL; }
					else if (size == 2) { return CH; }
					else if (size == 4) { return ECX; }
				}
				else if (r == EDX || r == DL || r == DH)
				{
					if (size == 1) { return DL; }
					else if (size == 2) { return DH; }
					else if (size == 4) { return EDX; }
				}

				return null;
			}

			public override string ToString()
			{
				return this.name;
			}
		}
	}
}
