using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Fasm
{
	partial class CodeGen : ICodeGen
	{
		protected List<string> code = new List<string>();
		protected Stack<Type> stack = new Stack<Type>();
		protected int offset = 0;

		protected Dictionary<string, Const> consts = new Dictionary<string, Const>();
		protected int count_str = 0;
		protected int count_label = 0;

		protected List<AsmLine> icode = new List<AsmLine>();
		protected Dictionary<Symbols.Var, Addr> address = new Dictionary<Symbols.Var, Addr>();

		protected List<string> imports = new List<string>();


		protected Stack<Reg> free_regs = new Stack<Reg>();


		protected Symbols.StackTable table = null;
		public CodeGen(Symbols.StackTable table)
		{
			this.table = table;

			free_regs.Push(Reg.ECX);
			free_regs.Push(Reg.EDX);
			free_regs.Push(Reg.EBX);
			free_regs.Push(Reg.EAX);
		}

		private Reg GetFreeReg()
		{
			return free_regs.Pop();
		}

		private Reg GetReg(Reg r)
		{
			this.free_regs.Pop();
			return r;
		}

		private void FreeReg(Reg r)
		{
			this.free_regs.Push(r);
		}

		private void AddCode(Com command, Operand op1 = null, Operand op2 = null, Operand op3 = null)
		{
			this.icode.Add(new Instr(command, op1, op2, op3));
		}

		public void Import(Symbols.Var func) 
		{ 
			Symbols.Func f = (Symbols.Func)func.GetType();
			this.address.Add(func, new Addr(func.GetName()));
			this.imports.Add(func.GetName() + ", '" + func.GetName() + "'");
		}

		public void Decl(Symbols.Var var) 
		{
			int size = var.GetType() is Symbols.CHAR ? 4 : var.GetType().GetSizeType();

			this.offset += size;

			AddCode(Com.SUB, new Addr(Reg.ESP), new Val(size, 4));
			this.address.Add(var, new Addr(Reg.EBP, size) - this.offset);
		}

		public void StartProc(Symbols.Func f) 
		{
			string label = this.Label(f.GetName());
			AddCode(Com.PUSH, new Addr(Reg.EBP, 4));
			AddCode(Com.MOV, new Addr(Reg.EBP, 4), new Addr(Reg.ESP));
			this.offset = 0;
		}

		public void EndProc() 
		{
			AddCode(Com.MOV, new Addr(Reg.ESP), new Addr(Reg.EBP));
			AddCode(Com.POP, new Addr(Reg.EBP));
			AddCode(Com.RET);
		}


		public void ToInt(int size = 4) 
		{
			this.stack.Pop();
			AddCode(Com.FLD, new Val(new Addr(Reg.ESP), size:8));				//fld qword[esp]
			AddCode(Com.ADD, new Addr(Reg.ESP), new Val(4, size:4));			//add esp, 4
			AddCode(Com.FISTP, new Addr(Reg.ESP, size:4, cast:true));		//fistp dword[esp]
			this.stack.Push(Type.GetType(4));
		}

		public void ToFloat(int size = 8)
		{
			this.stack.Pop();
			AddCode(Com.FILD, new Val(new Addr(Reg.ESP)));						//fild dword[esp]
			AddCode(Com.SUB, new Addr(Reg.ESP), new Val(4, size: 4));		//sub esp, 4
			AddCode(Com.FSTP, new Addr(Reg.ESP, size:8, cast:true));			//fstp qword[esp]
			this.stack.Push(Type.GetType(8));
		}

		public void Call(Symbols.Type ret_type)
		{
			Reg r = GetFreeReg();
			AddCode(Com.POP, new Addr(r));
			AddCode(Com.CALL, new Val(new Addr(r), 4));   
		}

		public void Push(Syntax.Const c) 
		{
			int size_t = c.GetType().GetSizeType();
			if (c.GetType() is Symbols.INT || c.GetType() is Symbols.CHAR)
			{
				this.icode.Add(new Instr(Com.PUSH, new Val(c, size_t)));
				this.stack.Push(Type.GetType(size_t));
			}
			else if (c.GetType() is Symbols.DOUBLE)
			{
				AddCode(Com.PUSH, new Val(c.GetValue(), size:4));				//push val
				AddCode(Com.FLD, new Addr(Reg.ESP, size:4, cast:true));		//fld dword[esp]
				AddCode(Com.SUB, Reg.ESP, new Val(4, size: 4));					//sub esp, 4
				AddCode(Com.FSTP, new Addr(Reg.ESP, size:8, cast:true));		//fstp qword[esp]
				this.stack.Push(Type.GetType(size_t));
			}
			else if (c.GetType() is Symbols.ARRAY && ((Symbols.ARRAY)c.GetType()).GetRefType() is Symbols.CHAR)
			{
				if (!this.consts.ContainsKey(c.GetValue())) {
					string name = "@const" + ++this.count_str;
					string val = c.GetValue();
					val = "\"" + val + "\", 0";
					this.consts.Add(c.GetValue(), new Const(name, Type.DB, val, c.GetValue().Length));
				}

				AddCode(Com.PUSH, new Addr(this.consts[c.GetValue()].name, size:4));
				this.stack.Push(Type.GetType(4));
			}
		}

		public void Push(Syntax.Identifier identifier) 
		{
			Reg r1 = GetFreeReg();
			Symbols.Var var = identifier.GetVariable();
			AddCode(Com.LEA, new Addr(r1), new Val(this.address[var]));		//lea eax, cast[addr]
			AddCode(Com.PUSH, new Addr(r1));											//push eax
			FreeReg(r1);
			this.stack.Push(Type.GetType(var.GetType().GetSizeType()));
		}

		public void Pop() 
		{
			AddCode(Com.ADD, new Addr(Reg.ESP), new Val(this.stack.Pop().size, size:4));
		}

		public void Mov() 
		{
			Type op1 = this.stack.Pop(), op2 = this.stack.Pop();

			if (op1 == Type.QWORD && op2 == op1)
			{
				Reg r = GetFreeReg();
				AddCode(Com.FLD, new Addr(Reg.ESP, size:8, cast: true));			//fld qword[esp]
				AddCode(Com.MOV, new Addr(r), new Val(new Addr(Reg.ESP) + 8, size: 4)); // mov eax, dword[esp + 8]
				AddCode(Com.FSTP, new Addr(r, size: 8, cast: true));	//fstp qword[esp + 8]
				this.stack.Push(op2);
			} 
			else
			{
				Reg r1 = GetFreeReg(), r2 = GetFreeReg();	
				AddCode(Com.POP, new Addr(r1));												//pop eax
				AddCode(Com.POP, new Addr(r2));												//pop ebx
				AddCode(Com.MOV, new Addr(r2, size: op2.size, cast: true),			//mov dword[ebx], eax
					new Addr(Reg.GetRegPart(r1, op2.size)));
				AddCode(Com.PUSH, new Addr(r2));
				this.stack.Push(op2);
				FreeReg(r1);
				FreeReg(r2);
			}
		}

		public void Add() 
		{
			if (this.stack.Peek() is Symbols.DOUBLE)
			{
				this.code.Add("fld qword[esp]");
				this.code.Add("fadd qword[esp + 8]");
				this.code.Add("sub esp, 8");
				this.code.Add("fstp qword[esp]");
			}
			else
			{
				this.code.Add("pop eax");
				this.code.Add("pop ebx");
				this.code.Add("add eax, ebx");
				this.code.Add("push eax");
			}
		}

		public void Sub() { }

		public void Mul() { }

		public void Div() { }

		public void Mod() { }

		public void Neg() { }

		public void Xor() { }

		public void LeftShift() { }

		public void RightShift() { }

		public void More() { }

		public void Less() { }

		public void Equels() { }

		public void Cond() { }

		protected string Label(string name)
		{
			this.icode.Add(new Lab(name));
			return name;
		}

		public string Label() 
		{
			string label = "@label" + ++this.count_label;
			return this.Label(label);
		}

		public void Jump(string label) { }

		public void Generate(System.IO.StreamWriter outs) 
		{
			foreach (Symbols.Symbol v in this.table.GetCurrentTable().GetVariables())
			{
				v.GenerateCode(this);
			}


			outs.WriteLine("format PE console");
			outs.WriteLine("entry start");
			outs.WriteLine("include 'include/win32a.inc'");


			if (this.consts.Count > 0)
			{
				outs.WriteLine();
				outs.WriteLine("section '.rdata' data readable");
				foreach (Const c in this.consts.Values)
				{
					outs.WriteLine("\t" + c.ToString());
				}
			}

//			outs.WriteLine();
//			outs.WriteLine("section '.data' data readable writeable");

			outs.WriteLine();
			outs.WriteLine("section '.idata' data readable import");
			outs.WriteLine("\tlibrary kernel32, 'kernel32.dll', msvcrt, 'msvcrt.dll'");
			outs.WriteLine("\timport kernel32, ExitProcess, 'ExitProcess'");

			if (this.imports.Count > 0)
			{
				outs.Write("\timport msvcrt, ");
				foreach (string import in this.imports)
				{
					outs.Write("\\");
					outs.Write(outs.NewLine);
					outs.Write("\t, " + import);
				}

				outs.WriteLine();
			}


			if (this.icode.Count > 0)
			{
				outs.WriteLine();
				outs.WriteLine("section '.text' code executable");

				foreach (AsmLine instr in this.icode)
				{
					outs.WriteLine((instr is Lab ? "" : "\t") + instr.ToString());
				}
			}

			outs.WriteLine("start:");
			outs.WriteLine("\tcall main");
			outs.WriteLine("\tpush 0");
			outs.WriteLine("\tcall [ExitProcess]");
		}
	}
}
