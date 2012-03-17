using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	abstract class SynObj
	{
		public class Exception : Compiler.Exception
		{
			public Exception(string s) : base(s) { }
		}
		public int line = -1, pos = -1;
		protected const int INDENT = 5;
		protected const char SEP = '\"';

		public static Token CheckToken(Token t, Token.Type t_type, string s)
		{
			if (t.type != t_type)
			{
				throw new SynObj.Exception(s);
			}

			return t;
		}

		public static SynObj CheckSynObj(SynObj obj, string s = "требуется выражение")
		{
			if (obj == null)
			{
				throw new Exception(s);
			}

			return obj;
		}

		public abstract string ToString(int level = 0);

		protected string getIndentString(int level)
		{
			return '\n' + new String(' ', level * INDENT);
		}
	}

	class Root : SynObj
	{
		ArrayList statments = new ArrayList();

		public Root() { }

		public void AddChild(SynObj stmt)
		{
			statments.Add(stmt);
		}

		public override string ToString(int level = 0)
		{
			string s = "";
			foreach (var stmt in statments)
			{
				s += ((SynObj)stmt).ToString(0);
			}
			return s;
		}
	}

#region Expression

	abstract class SynExpr: SynObj 
	{
		public SymType type = new SymSuperType();
		public bool lvalue = false;
		public SymType getType()
		{
			return type;
		}

		abstract public void Check();

		abstract public void GenerateCode(CodeGen.Code code, bool address=false);

		public static void GeneratePopResult(CodeGen.Code code)
		{
			code.AddComand("pop", "ebx");
			code.AddComand("pop", "eax"); 
		}

	}

	abstract class SynConstExpr : SynExpr
	{
		virtual public int ComputeConstIntValue()
		{
			throw new FatalException();
		}
	}

	abstract class SynOper : SynConstExpr
	{
	}

	abstract class UnaryOper : SynOper
	{
		protected Token oper;
		protected SynExpr operand;

		public UnaryOper(Token op)
		{
			oper = op;
			line = op.line;
			pos = op.pos;
		}

		public void SetOperand(SynExpr operand)
		{
			this.operand = (SynExpr)CheckSynObj(operand);
			Check();
		}

		public override void Check()
		{
			if (!Symantic.CheckUnaryOper(operand.type, oper.type))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор \"" + oper.strval + "\" соответствующий данному операнду", oper.pos, oper.line);
				e.Data["delayed"] = true;
			}

			if ((oper.type == Token.Type.OP_INC || oper.type == Token.Type.OP_DEC || oper.type == Token.Type.OP_BIT_AND) && !operand.lvalue)
			{
				Symbol.Exception e = new Symbol.Exception("для \"" + oper.strval +"\" требуется левостороннее значение", operand.pos, operand.line);
				e.Data["delayed"] = true;
				throw e;
			}
		} 
	}

	class ExprList : SynConstExpr
	{
		public List<SynExpr> list;
		public ExprList(List<SynExpr> exprs)
		{
			list = exprs;
			Check();
		}

		override public string ToString(int level=0)
		{
			string s = getIndentString(level);
			s += "list";

			foreach (var ch in this.list)
			{
				s += ((SynExpr)ch).ToString(level + 1);
			}

			return s;
		}

		public override void Check()
		{
			if (list.Count > 0)
			{
				this.type = list[0].type;
			}
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			int i = list.Count;
			foreach (var expr in list)
			{
				expr.GenerateCode(code, address);

				i--;
				if (i == 0)
				{
					break;
				}
				code.AddComand("pop", CodeGen.REG_THROW_TOP_STACK_VAL);
			}
		}

		public override int ComputeConstIntValue()
		{
			if (list.Count != 1 || !(list[0] is SynConstExpr))
			{
				throw new FatalException();
			}

			return ((SynConstExpr)list[0]).ComputeConstIntValue();
		}
	}

	class BinaryOper : SynOper
	{
		protected SynExpr lnode = null, rnode = null;
		protected Token oper = null;

		public BinaryOper(Token op)
		{
			oper = op;
		}

		public BinaryOper() { }

		public void SetLeftOperand(SynExpr l)
		{
			lnode = (SynExpr)CheckSynObj(l);
			line = lnode.line;
			pos = lnode.pos;
		}

		public void SetRightOperand(SynExpr r)
		{
			rnode = (SynExpr)CheckSynObj(r);
			Check();
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + oper.strval + SEP;
			s += lnode.ToString(level + 1);
			s += rnode.ToString(level + 1);
			return s;
		}

		public override void Check()
		{
			if(!Symantic.CheckBinaryOper(lnode.type, rnode.type, oper.type))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор\"" + oper.strval + "\" соответствующий данным операторандам", oper.pos, oper.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = Symantic.GetTypeBinaryOper(lnode.type, rnode.type, oper.type);
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			lnode.GenerateCode(code, address);
			rnode.GenerateCode(code, address);
			code.AddComment("\"" + Token.type_to_terms[this.oper.type] + "\"");
			SynExpr.GeneratePopResult(code);
			
			string reg_res = "eax", true_label, false_label, end_label;
			switch (this.oper.type)
			{
				case Token.Type.OP_PLUS:
					code.AddComand("add", "eax", "ebx");
					break;
				case Token.Type.OP_SUB:
					code.AddComand("sub", "eax", "ebx");
					break;
				case Token.Type.OP_STAR:
					code.AddComand("imul", "eax", "ebx");
					break;
				case Token.Type.OP_DIV:
					code.AddComand("cdq");
					code.AddComand("idiv", "ebx");
					break;
				case Token.Type.OP_MOD:
					code.AddComand("cdq");
					code.AddComand("idiv", "ebx");
					reg_res = "edx";
					break;
				case Token.Type.OP_L_SHIFT:
					code.AddComand("mov", "ecx", "ebx");
					code.AddComand("sal", "eax", "cl");
					break;
				case Token.Type.OP_R_SHIFT:
					code.AddComand("mov", "ecx", "ebx");
					code.AddComand("sar", "eax", "cl");
					break;
				case Token.Type.OP_XOR:
					code.AddComand("xor", "eax", "ebx");
					break;
				case Token.Type.OP_BIT_AND:
					code.AddComand("and", "eax", "ebx");
					break;
				case Token.Type.OP_AND:
					false_label = code.GenerateLabel(); end_label = code.GenerateLabel();
					code.AddComand("cmp", "eax", "0");
					code.AddComand("je", false_label);
					code.AddComand("cmp", "ebx", "0");
					code.AddComand("je", false_label);
					code.AddComand("push", "1");
					code.AddComand("jmp", end_label);
					code.AddLabel(false_label);
					code.AddComand("push", "0");
					code.AddLabel(end_label);
					break;
				case Token.Type.OP_OR:
					true_label = code.GenerateLabel(); end_label = code.GenerateLabel();
					code.AddComand("cmp", "eax", "1");
					code.AddComand("je", true_label);
					code.AddComand("cmp", "ebx", "1");
					code.AddComand("je", true_label);
					code.AddComand("push", "0");
					code.AddComand("jmp", end_label);
					code.AddLabel(true_label);
					code.AddComand("push", "0");
					code.AddLabel(end_label);
					break;
				case Token.Type.OP_BIT_OR:
					code.AddComand("or", "eax", "ebx");
					break;

				case Token.Type.OP_NOT_EQUAL:
				case Token.Type.OP_EQUAL:
				case Token.Type.OP_LESS:
				case Token.Type.OP_MORE:
				case Token.Type.OP_LESS_OR_EQUAL:
				case Token.Type.OP_MORE_OR_EQUAL:
					code.AddComand("cmp", "eax", "ebx");
					true_label = code.GenerateLabel(); end_label = code.GenerateLabel(); false_label = code.GenerateLabel();
					switch (this.oper.type)
					{
						case Token.Type.OP_NOT_EQUAL:
							code.AddComand("jne", true_label);
							break;
						case Token.Type.OP_EQUAL:
							code.AddComand("je", true_label);
							break;
						case Token.Type.OP_LESS:
							code.AddComand("jl", true_label);
							break;
						case Token.Type.OP_MORE:
							code.AddComand("jg", true_label);
							break;
						case Token.Type.OP_LESS_OR_EQUAL:
							code.AddComand("jle", true_label);
							break;
						case Token.Type.OP_MORE_OR_EQUAL:
							code.AddComand("jge", true_label);
							break;
					}
					code.AddComand("jmp", false_label);
					code.AddLabel(true_label);
					code.AddComand("mov", "eax", "1");
					code.AddComand("jmp", end_label);
					code.AddLabel(false_label);
					code.AddComand("mov", "eax", "0");
					code.AddLabel(end_label);
					break;

				default:
					throw new NotImplementedException();
			}
			code.AddComand("push", reg_res);
		}

		public override int ComputeConstIntValue()
		{
			if (!(lnode is SynConstExpr && rnode is SynConstExpr))
			{
				throw new FatalException();
			}

			int a = ((SynConstExpr)lnode).ComputeConstIntValue(), b = ((SynConstExpr)rnode).ComputeConstIntValue();

			switch (oper.type)
			{
				case Token.Type.OP_PLUS:
					return a + b;
				case Token.Type.OP_SUB:
					return a - b;
				case Token.Type.OP_STAR:
					return a * b;
				case Token.Type.OP_MOD:
					return a % b;
				case Token.Type.OP_DIV:
					return a / b;
				case Token.Type.OP_AND:
					return a == 0 || b == 0 ? 0 : 1;
				case Token.Type.OP_OR:
					return a == 0 && b == 0 ? 0 : 1;
				case Token.Type.OP_EQUAL:
					return a != b? 0 : 1;
				case Token.Type.OP_NOT_EQUAL:
					return a == b ? 0 : 1;
				case Token.Type.OP_L_SHIFT:
					return a << b;
				case Token.Type.OP_R_SHIFT:
					return a >> b;
				case Token.Type.OP_BIT_AND:
					return a & b;
				case Token.Type.OP_BIT_OR:
					return a | b;
				case Token.Type.OP_XOR:
					return a ^ b;
				default:
					throw new FatalException();
			}
		}
	}

	class AssignOper : BinaryOper
	{
		public AssignOper(Token op) : base(op) { lvalue = true; }
		public AssignOper() : base() 
		{
			oper = new Token(Token.Type.OP_ASSIGN);
			lvalue = true;
		}

		public override void Check()
		{
			if(!Symantic.CheckBinaryOper(lnode.type, rnode.type, oper.type))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор\"" + oper.strval + "\" соответствующий данным операторандам", oper.pos, oper.line);
				e.Data["delayed"] = true;
				throw e;
			}
			
			if (!lnode.lvalue)
			{
				Symbol.Exception e = new Symbol.Exception("выражение должно быть допустимым для изменения левосторонним значением", lnode.pos, lnode.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = Symantic.GetTypeBinaryOper(lnode.type, rnode.type, oper.type);
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			Token.Type t = Token.Type.NONE;
			switch (oper.type)
			{
				case Token.Type.OP_PLUS_ASSIGN:
					t = Token.Type.OP_PLUS;
					break;
				case Token.Type.OP_SUB_ASSIGN:
					t = Token.Type.OP_SUB;
					break;
				case Token.Type.OP_MUL_ASSIGN:
					t = Token.Type.OP_STAR;
					break;
				case Token.Type.OP_MOD_ASSIGN:
					t = Token.Type.OP_MOD;
					break;
				case Token.Type.OP_DIV_ASSIGN:
					t = Token.Type.OP_DIV;
					break;
				case Token.Type.OP_BIT_AND_ASSIGN:
					t = Token.Type.OP_BIT_AND;
					break;
				case Token.Type.OP_BIT_OR_ASSIGN:
					t = Token.Type.OP_BIT_OR;
					break;
				case Token.Type.OP_XOR_ASSIGN:
					t = Token.Type.OP_XOR;
					break;
				case Token.Type.OP_L_SHIFT_ASSIGN:
					t = Token.Type.OP_L_SHIFT;
					break;
				case Token.Type.OP_R_SHIFT_ASSIGN:
					t = Token.Type.OP_R_SHIFT;
					break;
			}

			if (t != Token.Type.NONE)
			{
				BinaryOper binop = new BinaryOper(new Token(t));
				binop.SetLeftOperand(lnode);
				binop.SetRightOperand(rnode);
				rnode = binop;
			}

			lnode.GenerateCode(code, true);
			rnode.GenerateCode(code, false);
			code.AddComment("'='");
			SynExpr.GeneratePopResult(code);
			code.AddComand("mov", "[eax]", "ebx");
			code.AddComand("push", "[eax]");
		}
	}

	class ConstExpr : SynConstExpr
	{
		protected Token token;
		public string value = "";

		public ConstExpr(SymType type, string val)
		{
			value = val;
			this.type = type;
			token = new Token();
		}

		public ConstExpr(Token t, SymType type)
		{
			token = t;
			line = t.line;
			pos = t.pos;
			value = t.strval;
			this.type = type;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + value;
		}

		public override void Check()
		{
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			code.AddComand("push", value);
		}

		public override int ComputeConstIntValue()
		{
			if (type is SymTypeInt)
			{
				return Int32.Parse(value);
			}
			else if (type is SymTypeChar)
			{
				return Int32.Parse(value);
			}

			throw new FatalException();
		}
	}

	class ConstString : IdentExpr
	{
		public ConstString(SymVar var) : base(var) { }

		public override void GenerateCode(CodeGen.Code code, bool address = false)
		{
			code.AddComand("push", "offset " + var.name);
		}
	}

	class IdentExpr : SynConstExpr
	{
		public Token token;
		public SymVar var = null;

		public IdentExpr(Token t)
		{
			lvalue = true;
			line = t.line;
			pos = t.pos;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "требуется идентификатор");
		}

		public IdentExpr(SymVar v)
		{
			lvalue = true;
			var = v;
			Check();
		}

		public IdentExpr(Token t, SymVar v)
		{
			lvalue = true;
			line = t.line;
			pos = t.pos;
			var = v;
			token = t;
			Check();
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + (token == null? "const": token.strval);
		}

		public override void Check()
		{
			this.type = var.type;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			if (address)
			{
				code.AddComand("lea", "eax", var.name);
				code.AddComand("push", "eax");
			}
			else
			{
				code.AddComand("push", var.name);
			}
		}

		public override int ComputeConstIntValue()
		{
			if (var is SymVarConst)
			{
				return ((SymVarConst)var).value.ComputeConstIntValue();
			}

			throw new FatalException();
		}
	}

	class PrefixOper : UnaryOper
	{
		public PrefixOper(Token op) : base(op) 
		{
			if (op.type == Token.Type.OP_STAR)
			{
				lvalue = true;
			}
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + oper.strval + "@" + SEP;
			s += operand.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			if (oper.type == Token.Type.OP_BIT_AND || oper.type == Token.Type.OP_DEC || oper.type == Token.Type.OP_INC)
			{
				
				if (oper.type != Token.Type.OP_BIT_AND)
				{
					BinaryOper op = new BinaryOper(new Token(oper.type == Token.Type.OP_INC ? Token.Type.OP_PLUS : Token.Type.OP_SUB));
					op.SetLeftOperand(operand);
					op.SetRightOperand(new ConstExpr(new Token(Token.Type.CONST_INT, "1"), new SymTypeInt()));
					AssignOper assign = new AssignOper(new Token(Token.Type.OP_ASSIGN));
					assign.SetLeftOperand(operand);
					assign.SetRightOperand(op);
					assign.GenerateCode(code);
					return;
				}

				operand.GenerateCode(code, true);
			}
			else
			{
				operand.GenerateCode(code, false);
				code.AddComand("pop", "eax");
				switch (oper.type)
				{
					case Token.Type.OP_NOT:
					case Token.Type.OP_TILDE:
						code.AddComand("not", "eax");
						break;
					case Token.Type.OP_PLUS:
						break;
					case Token.Type.OP_SUB:
						code.AddComand("neg", "eax");
						break;
					case Token.Type.OP_STAR:
						code.AddComand("mov", "eax", "[eax]");
						break;
				}
				code.AddComand("push", "eax");
			}
		}

		public override int ComputeConstIntValue()
		{
			if (oper.type != Token.Type.OP_NOT || operand is SynConstExpr)
			{
				throw new FatalException();
			}

			return ((SynConstExpr)operand).ComputeConstIntValue() == 0 ? 0 : 1;
		}
	}

	class PostfixOper : UnaryOper
	{
		public PostfixOper(Token op) : base(op) { }

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + "@" + oper.strval + SEP;
			s += operand.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{

			List<SynExpr> list = new List<SynExpr>();
			BinaryOper op = new BinaryOper(new Token(oper.type == Token.Type.OP_INC ? Token.Type.OP_PLUS : Token.Type.OP_SUB));
			op.SetLeftOperand(operand);
			op.SetRightOperand(new ConstExpr(new Token(Token.Type.CONST_INT, "1"), new SymTypeInt()));
			list.Add(op);
			AssignOper assign = new AssignOper(new Token(Token.Type.OP_ASSIGN));
			assign.SetLeftOperand(operand);
			assign.SetRightOperand(op);
			list.Add(assign);
			new ExprList(list).GenerateCode(code);
			/*

			operand.GenerateCode(code, true);
			code.AddComand("pop", "eax");
			code.AddComand("mov", "ebx", "[eax]");
			code.AddComand("push", "ebx");
			string op = "";
			switch (oper.type)
			{
				case Token.Type.OP_INC:
					op = "inc";
					break;
				case Token.Type.OP_DEC:
					op = "dec";
					break;
				default:
					throw new FatalException();
			}
			code.AddComand(op, "ebx");
			code.AddComand("mov", "[eax]", "ebx");*/
		}
	}

	class CallOper : SynExpr
	{
		SynExpr operand;
		List<SynExpr> args = new List<SynExpr>();

		public CallOper(SynExpr opnd, int line, int pos)
		{
			operand = opnd;
			this.line = line;
			this.pos = pos;
		}

		public void AddArgument(SynExpr arg)
		{
			args.Add(arg);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "CALL";
			s += '\n' + operand.ToString(level + 1);

			foreach (var arg in args)
			{
				s += ((SynObj)arg).ToString(level + 1);
			}
			return s;
		}

		public override void Check()
		{
			if (!(operand.type is SymTypeFunc))
			{
				Symbol.Exception e = new Symbol.Exception(
					(operand is IdentExpr? ((IdentExpr)operand).token.strval :"выражение") + " не является функцией",
					operand.pos, operand.line);

				e.Data["delayed"] = true;
				throw e;
			}

			foreach (var a in args)
			{
				/// тут должна быть проверка переменных=))
			}

			this.type = operand.type;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			for (int i = args.Count - 1; i >= 0; i--)
			{
				args[i].GenerateCode(code);
			}
			operand.GenerateCode(code, true);
			code.AddComand("pop", "eax");
			code.AddComand("call", "eax");

			bool ret = false;
			if (!(((SymTypeFunc)operand.type).type is SymTypeVoid))
			{
				code.AddComand("pop", "eax");
				ret = true;
			}

			code.AddComand("push", ret ? "eax" : CodeGen.REG_THROW_TOP_STACK_VAL);
		}

	}

	class TerOper : SynOper
	{
		SynExpr cond, lnode, rnode;

		public TerOper(SynExpr cond)
		{
			this.cond = cond;
			this.line = cond.line;
			this.pos = cond.pos;
		}

		public void SetTrueExpr(SynExpr node)
		{
			lnode = (SynExpr)CheckSynObj(node);
		}

		public void SetFalseExpr(SynExpr node)
		{
			rnode = (SynExpr)CheckSynObj(node);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "ter_oper";
			s += cond.ToString(level + 1);
			s += lnode.ToString(level + 1);
			s += rnode.ToString(level + 1);
			return s;
		}

		public override void Check()
		{
			throw new NotImplementedException();
			/* нужно попытать прикаставать выозвращаемые результаты к одному типу*/
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			cond.GenerateCode(code);
			code.AddComand("pop", "eax");
			code.AddLine(".IF eax!=0");
			lnode.GenerateCode(code);
			code.AddLine(".ELSE");
			rnode.GenerateCode(code);
			code.AddLine(".ENDIF");
		}

		public override int ComputeConstIntValue()
		{
			if (((SynConstExpr)cond).ComputeConstIntValue() == 0)
			{
				return ((SynConstExpr)rnode).ComputeConstIntValue();
			}
			else
			{
				return ((SynConstExpr)lnode).ComputeConstIntValue();
			}
		}
	}

	class DotOper : SynExpr
	{
		protected Token oper;
		protected SynExpr parent;
		protected IdentExpr child;

		public DotOper(Token op)
		{
			oper = op;
			lvalue = true;
		}

		virtual public void SetParent(SynExpr parent)
		{
			this.parent = (SynExpr)CheckSynObj(parent);
		}

		virtual public void SetChild(IdentExpr child)
		{
			this.child = (IdentExpr)CheckSynObj(child, "требуется имя члена");
			line = child.line;
			pos = child.pos;
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += oper.strval;
			s += parent.ToString(level + 1);
			s += child.ToString(level + 1);
			return s;
		}

		public override void Check()
		{
			if (!(parent.type is SymTypeStruct))
			{
				Symbol.Exception e = new Symbol.Exception("выражение слева от \"." + child.token.strval + "\" должно представлять структуру", parent.pos, parent.line);
				e.Data["delayed"] = true;
				throw e;
			}

			if (!(((SymTypeStruct)parent.type).fields.ContainsIdentifier(child.token.strval)))
			{
				Symbol.Exception e = new Symbol.Exception("\"" + child.token.strval + "\" не является членом \"" + 
					((SymTypeStruct)parent.type).name + "\"", parent.pos, parent.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = child.type;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			parent.GenerateCode(code, true);
			code.AddComment("\".\"");
			code.AddComand("pop", "eax");
			SymTypeStruct t = ((SymTypeStruct)parent.type);

			int size = 0;
			foreach (SymVar field in t.fields.vars.Values)
			{
				if (field.name == child.var.name)
				{
					break;
				}
				size += field.type.GetSize();
			}
			
			code.AddComand("lea", "eax", "[eax" + (size == 0 ? "]" : "+" + size + "]"));
			if (address)
			{
				code.AddComand("push", "eax");
			}
			else
			{
				code.AddComand("push", "[eax]");
			}
		}
	}

	class RefOper : DotOper
	{
		public RefOper(Token t) : base(t) 
		{
			lvalue = true;
		}

		public override void Check()
		{
			if (!(parent.type is SymTypePointer && ((SymTypePointer)parent.type).type is SymTypeStruct))
			{
				Symbol.Exception e = new Symbol.Exception("выражение слева от \"->" + child.token.strval + "\" должно указывать на структуру", parent.pos, parent.line);
				e.Data["delayed"] = true;
				throw e;
			}

			if (! ((SymTypeStruct)((SymTypePointer)parent.type).type).fields.ContainsIdentifier(child.token.strval))
			{
				Symbol.Exception e = new Symbol.Exception("\"" + child.token.strval + "\" не является членом \"" +
					((SymTypeStruct)((SymTypePointer)parent.type).type).name + "\"", parent.pos, parent.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = child.type;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			PrefixOper op = new PrefixOper(new Token(Token.Type.OP_BIT_AND));
			op.SetOperand(parent);
			DotOper dop = new DotOper(new Token(Token.Type.OP_DOT));
			dop.SetParent(op);
			dop.SetChild(child);
			dop.GenerateCode(code, address);
		}
	}

	class SqBrkOper : RefOper
	{
		public new SynExpr child;

		public SqBrkOper(Token t) : base(t) 
		{
			lvalue = true;
		}

		public void SetChild(SynExpr index)
		{
			child = index;
		}

		public override void Check()
		{
			if (!(parent.type is SymTypeArray || parent.type is SymTypePointer))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор \"[]\" соответствующий этим операндам", oper.pos, oper.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = ((SymRefType)parent.type).type;
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += oper.strval;
			s += parent.ToString(level + 1);
			s += child.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			parent.GenerateCode(code, true);
			BinaryOper oper = new BinaryOper(new Token(Token.Type.OP_STAR));
			int p = parent.getType().GetSize();
			if (this.parent.getType() is SymTypeArray && ((SymTypeArray)this.parent.getType()).type is SymTypeArray)
			{
				p *= ((SymTypeArray)this.parent.getType()).GetSizeArray();
			}
			oper.SetLeftOperand(child);
			oper.SetRightOperand(new ConstExpr(new SymTypeInt(), p.ToString()));
			oper.GenerateCode(code);
			code.AddComment("\"[]\"");
			SynExpr.GeneratePopResult(code);
			code.AddComand("add", "eax", "ebx");

			if (address)
			{
				code.AddComand("push", "eax");
			}
			else
			{
				code.AddComand("push", "[eax]");
			}
		}
	}

	class CastExpr : SynExpr
	{
		SynObj operand;
		SymType type_name;

		public CastExpr(SymType type_name, int line, int pos)
		{
			this.type_name = type_name;
		}

		public void SetOperand(SynExpr operand)
		{
			this.operand = CheckSynObj(operand);
			Check();
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + SEP + "CAST" + SEP + type_name.ToString() + operand.ToString(level + 1);
		}

		public override void Check()
		{
			this.type = type_name;
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			throw new NotImplementedException();
		}
	}

	class SizeofOper : PrefixOper
	{
		new SymType operand;
		public SizeofOper(Token op) : base(op) 
		{
			line = op.line;
			pos = op.pos;
		}

		public void SetOperand(SymType operand)
		{
			this.operand = operand;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "SIZEOF" + operand.ToString();
		}

		public override void Check()
		{
			this.type = new SymTypeInt();
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			throw new NotImplementedException();
		}
	}

	class SynInit : SynExpr
	{
		public SynExpr val = null;

		public SynInit(SynExpr v)
		{
			this.val = v;
			line = v.line;
			pos = v.pos;
		}

		public SynInit() { }

		public override string ToString(int level = 0)
		{
			return val.ToString();
		}

		public override void Check()
		{
			this.type = val.type;
		}

		public static string GenerateBaseInitCode(SymType t)
		{
			if (t is SymTypeScalar || t is SymTypePointer || t is SymTypeEnum)
			{
				return "?";
			}
			else if (t is SymTypeFunc)
			{
				return "";
			}
			else if (t is SymTypeStruct)
			{
				return "<>";
			}
			else if (t is SymTypeArray)
			{
				return "dup(" + "?" + ")";
			}

			throw new NotImplementedException();
		}

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			val.GenerateCode(code);
		}

		public int ComputeConstIntValue()
		{
			if (val is SynConstExpr || (val is IdentExpr && ((IdentExpr)val).var is SymVarConst))
			{
				return ((SynConstExpr)val).ComputeConstIntValue();
			}

			throw new FatalException();
		}
	}

	class SynInitList : SynExpr
	{
		List<SynInit> list = new List<SynInit>();

		public SynInitList(int line, int pos)
		{
			this.line = line;
			this.pos = pos;
		}

		public void AddInit(SynInit init)
		{
			list.Add((SynInit)CheckSynObj(init));
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "INIT_LIST { ";

			foreach (var x in list)
			{
				s += '\n' + ((SynObj)x).ToString(level + 1);
			}

			return s + " }";
		}

		public override void Check() { }

		public override void GenerateCode(CodeGen.Code code, bool address=false)
		{
			throw new NotImplementedException();
		}
	}

#endregion 

#region Statement

	abstract class SynStmt : SynObj 
	{
		abstract public void GenerateCode(CodeGen.Code code);
	}

	class StmtIF : SynStmt
	{
		SynExpr cond;
		SynObj lnode, rnode;

		public StmtIF(SynExpr cond)
		{
			this.cond = (SynExpr)CheckSynObj(cond);
		}

		public void SetBranchTrue(SynObj br)
		{
			lnode = CheckSynObj(br);
		}

		public void SetBranchFalse(SynObj br) 
		{
			rnode = CheckSynObj(br);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "IF";
			s += cond.ToString(level + 1);
			s += lnode.ToString(level + 1);
			s += rnode == null? "": rnode.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtBLOCK : SynStmt
	{
		List<SynObj> list = new List<SynObj>();
		public SymTable table = null;

		public StmtBLOCK(List<SynObj> stmts, SymTable tb)
		{
			list = stmts;
			table = tb;
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "BLOCK";

			foreach (var x in list)
			{
				s += ((SynObj)x).ToString(level + 1);
			}

			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			foreach (SynObj stmt in list)
			{
				if (stmt is SynExpr)
				{
					((SynExpr)stmt).GenerateCode(code);
				}
				else
				{
					((SynStmt)stmt).GenerateCode(code);
				}
			}
		}
	}

	class StmtFOR : SynStmt
	{
		List<SynObj> counter = new List<SynObj>();
		SynObj block;

		public StmtFOR() { }

		public void SetCounter(SynObj opnd)
		{
			counter.Insert(0, CheckSynObj(opnd));
		}

		public void SetCond(SynObj cond)
		{
			counter.Insert(1, CheckSynObj(cond));
		}

		public void SetIncriment(SynObj incr)
		{
			counter.Insert(2, CheckSynObj(incr));
		}

		public void SetBlock(SynObj block)
		{
			this.block = CheckSynObj(block);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "FOR";
			foreach (var c in counter)
			{
				s += ((SynObj)c).ToString(level + 1);
			}

			s += block.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtWHILE : SynStmt
	{
		SynExpr cond;
		SynObj block;

		public StmtWHILE(SynExpr cond)
		{
			this.cond = (SynExpr)CheckSynObj(cond);
		}

		public void SetBlock(SynObj block)
		{
			this.block = CheckSynObj(block);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "WHILE";
			s += cond.ToString(level + 1);
			s += block.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtDO : SynStmt
	{
		SynExpr cond;
		SynObj block;

		public StmtDO(SynObj block)
		{
			this.block = CheckSynObj(block);
		}

		public void SetCond(SynExpr cond)
		{
			this.cond = (SynExpr)CheckSynObj(cond);
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "DO WHILE";
			s += cond.ToString(level + 1);
			s += block.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtRETURN : SynStmt
	{
		SynExpr val;
	
		public StmtRETURN(SynExpr val = null)
		{
			this.val = val;
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += "RETURN";
			s += val == null? "": val.ToString(level + 1);
			return s;
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			val.GenerateCode(code);
		}
	}

	class StmtCONTINUE : SynStmt 
	{
		public StmtCONTINUE() { }

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "CONTINUE";
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtBREAK : SynStmt 
	{
		public StmtBREAK(){ }

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "BREAK";
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			throw new NotImplementedException();
		}
	}

	class StmtExpr : SynStmt
	{
		SynExpr expr;

		public StmtExpr(SynExpr _expr = null)
		{
			this.expr = _expr;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + (expr == null ? ";" : expr.ToString(level) + ";");
		}

		public override void GenerateCode(CodeGen.Code code)
		{
			expr.GenerateCode(code);
			code.AddComand("pop", CodeGen.REG_THROW_TOP_STACK_VAL);
			code.AddComment("\";\"");
		}
	}
#endregion

}
