using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	
	class CodeGen
	{
		public class Code
		{
			List<AsmLine> code =  new List<AsmLine>();

			public void AddComand(int indent, Command.Name name, string opr1=null, string opr2=null)
			{
				Command.Operand op1 = opr1 == null ? null : new Command.Operand(opr1);
				Command.Operand op2 = opr2 == null ? null : new Command.Operand(opr2);
				this.code.Add(new Command(indent, name, op1, op2));
			}

			public void AddLine(int indent, string s)
			{
				this.code.Add(new AsmLine(indent, s));
			}

			public override string ToString()
			{
				string s = "";
				foreach (var cs in code)
				{
					s += cs.ToString() + "\n";
				}
				return s;
			}
		}

		public class AsmLine 
		{
			protected int indent = 0;
			protected string s;
			public AsmLine(int indent, string s)
			{
				this.s = s;
				this.indent = indent;
			}

			public override string ToString()
			{
				return new String(' ', indent) + s;
			}
		};

		public class Command: AsmLine
		{
			public class Operand
			{
				 string s;

				public Operand(string s)
				{
					this.s = s;
				}

				public override string ToString()
				{
					return s;
				}
			}

			public enum Name 
			{
				ADD,
				SUB,
			};

			Name name;
			Operand opr1 = null, opr2 = null;

			public Command(int indent, Name name, Operand opr1 = null, Operand opr2 = null) : base(indent, "")
			{
				this.name = name;
				this.opr1 = opr1;
				this.opr2 = opr2;
				this.s = GetStrName(name) + (opr1 == null ? "" : " " + opr1.ToString()) + (opr2 == null ? "" : ", " + opr2.ToString());
			}

			static string GetStrName(Name name)
			{
				switch(name)
				{
					case Name.ADD:
						return "add";

					case Name.SUB:
						return "sub";
				}
				return null;
			}
		}

		System.IO.StreamWriter ostream = null;
		Parser parser = null;
		StackTable.Iterator titr = null;
		Code code = new Code();

		public CodeGen(System.IO.StreamWriter stream, Parser parser)
		{
			this.parser = parser;
			parser.Parse();
			titr = new StackTable.Iterator(parser.tables);
			ostream = stream;
		}

		public void Generate()
		{
			code.AddLine(0, ".386");
			code.AddLine(0, ".mode flat, stdcall");




			code.AddLine(0, ".code");
			foreach (var v in titr.Current().vars.Values)
			{
				if (v.type is SymTypeFunc)
				{
					((SymVarGlobal)v).GenerateCode(code);
				}
			}

			code.AddLine(0, "\n");
			code.AddLine(0, "start:");
			code.AddLine(3, "invoke main");
			code.AddLine(0, "end start");

			Console.Write(code.ToString());
		}

	}
}
