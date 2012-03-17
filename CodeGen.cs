using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	
	class CodeGen
	{
		public const string REG_THROW_TOP_STACK_VAL = "edi";
		public class Code
		{
			protected int indent = 0;
			private static int count_label = 0;
			List<AsmLine> code =  new List<AsmLine>();

			public Code(int indent = 0)
			{
				this.indent = indent;
			}

			public void AddComand(string oper, string opr1=null, string opr2=null, int _indent=-1)
			{
				Command.Operand op1 = opr1 == null ? null : new Command.Operand(opr1);
				Command.Operand op2 = opr2 == null ? null : new Command.Operand(opr2);
				this.code.Add(new Command(oper, op1, op2, _indent == -1? indent: _indent));
			}

			public void AddLine(string s, int _indent = -1)
			{
				this.code.Add(new AsmLine(s, _indent == -1 ? indent : _indent));
			}

			public void AddLabel(string label)
			{
				this.code.Add(new AsmLine(label + ":", 0));
			}

			public void NewLine()
			{
				this.code.Add(new AsmLine("\r\n", 0));
			}

			public void AddComment(string s, int _indent = -1)
			{
				AddLine(";" + s, _indent);
			}

			public string GenerateLabel(string prefix="label")
			{
				return prefix + Code.count_label++;
			}

			public void SetIndent(int _indent)
			{
				indent = _indent;
			}

			public override string ToString()
			{
				string s = "";
				foreach (var cs in code)
				{
					s += cs.ToString() + "\r\n";
				}
				return s;
			}

			public static Code operator +(Code cl, Code cr)
			{
				Code res = new Code();
				res.code = cl.code;
				res.code.AddRange(cr.code);
				return res;
			}
		}

		public class AsmLine 
		{
			protected int indent = 0;
			protected string s;
			public AsmLine(string s, int indent)
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

			Operand opr1 = null, opr2 = null;

			public Command(string oper, Operand opr1 = null, Operand opr2 = null, int indent=-1): base("", indent)
			{
				this.opr1 = opr1;
				this.opr2 = opr2;
				this.s = oper + (opr1 == null ? "" : " " + opr1.ToString()) + (opr2 == null ? "" : ", " + opr2.ToString());
			}
		}

		System.IO.StreamWriter ostream = null;
		Parser parser = null;
		StackTable.Iterator titr = null;
		
		public CodeGen(System.IO.StreamWriter stream, Parser parser)
		{
			this.parser = parser;
			parser.Parse();
			titr = parser.tables.Begin();
			Console.Write(parser.tables.ToString());
			ostream = stream;
		}

		public void Generate()
		{
			if (!parser.logger.isEmpty())
			{
				Console.Write(parser.logger.ToString());
				return;
			}

			Code data = new Code(), code = new Code(), res = new Code(), init = new Code();
			init.SetIndent(3);
			res.AddLine(".386");
			res.AddLine(".model flat, stdcall");
			res.AddLine("include " + System.IO.Path.Combine(Program.PATH_TO_MASM_LINCLUDE, "msvcrt.inc") , 3);//)"C:\\masm32\\include\\msvcrt.inc", 3);
			res.AddLine("includelib " + System.IO.Path.Combine(Program.PATH_TO_MASM_LIB, "msvcrt.lib"), 3);//C:\\masm32\\lib\\msvcrt.lib", 3);

			code.AddLine(".code");
			data.AddLine(".data");
			data.SetIndent(3);
			code.SetIndent(3);
			

			do
			{
				foreach (var t in titr.Current().types.Values)
				{
					if (t is SymTypeEnum || t is SymTypeStruct)
					{
						t.GenerateDeclarationCode(data);
					}
				}

				foreach (var c in titr.Current().consts.Values)
				{
					if (c.type is SymTypeArray) //for string const
					{
						c.GenerateCode(data);
					}
				}
			}
			while (titr.MoveNext());


			titr = parser.tables.Begin();
			foreach (var v in titr.Current().vars.Values)
			{
				if (!(v.type is SymTypeFunc))
				{
					v.GenerateCode(data);
					v.GenerateInitialize(init);
				}
			}

			titr = parser.tables.Begin();
			foreach (var v in titr.Current().vars.Values)
			{
				if (v.type is SymTypeIncludeFunc)
				{
					((SymVarGlobal)v).GenerateCode(code);
				}
				else if (v.type is SymTypeFunc)
				{
					((SymVarGlobal)v).GenerateCode(code);
				}
			}

			res = res + data;
			res = res + code;
			res.NewLine();
			res.AddLine("start:");
			res = res + init;
			res.AddLine("call main", 3);
			res.AddLine("RET", 3);
			res.AddLine("end start");
			Console.Write(res.ToString());
			ostream.Write(res.ToString());
		}
	}
}
