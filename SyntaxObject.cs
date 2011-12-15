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
		public SymType type = null;
		public SymType getType()
		{
			return type;
		}

		abstract public void Check();
	}

	abstract class SynOper : SynExpr
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
		}

		public override void Check()
		{
			if (!Symantic.CheckUnaryOper(operand.type, oper.type))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор \"" + oper.strval + "\" соответствующий данному операнду", oper.pos, oper.line);
				e.Data["delayed"] = true;
			}
		} 
	}

	class ExprList : SynExpr
	{
		public List<SynExpr> list;
		public ExprList(List<SynExpr> exprs)
		{
			list = exprs;
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
			this.type = list[0].type;
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

		public void SetLeftOperand(SynExpr l)
		{
			lnode = (SynExpr)CheckSynObj(l);
			line = lnode.line;
			pos = lnode.pos;
		}

		public void SetRightOperand(SynExpr r)
		{
			rnode = (SynExpr)CheckSynObj(r);
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
	}

	class AssignOper : BinaryOper
	{
		public AssignOper(Token op) : base(op) { }

	}

	class ConstExpr : SynExpr
	{
		protected Token token;

		public ConstExpr(Token t, SymType type)
		{
			token = t;
			line = t.line;
			pos = t.pos;
			this.type = type;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + token.strval;
		}

		public override void Check()
		{
			
		}
	}

	class IdentExpr : SynExpr
	{
		public Token token;
		public SymVar var = null;

		public IdentExpr(Token t, SymVar v)
		{
			line = t.line;
			pos = t.pos;
			var = v;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "требуется идентификатор");
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + token.strval;
		}

		public override void Check()
		{
			this.type = var.type;
		}
	}

	class PrefixOper : UnaryOper
	{
		public PrefixOper(Token op) : base(op) { }

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + oper.strval + "@" + SEP;
			s += operand.ToString(level + 1);
			return s;
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
	}

	class CallOper : SynExpr
	{
		SynExpr operand;
		ArrayList args = new ArrayList();

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

			this.type = operand.type;
		} 
	}

	class TerOper : SynExpr
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
	}

	class DotOper : SynOper
	{
		protected Token oper;
		protected SynExpr parent;
		protected IdentExpr child;

		public DotOper(Token op)
		{
			oper = op;
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
	}

	class RefOper : DotOper
	{
		public RefOper(Token t) : base(t) { }
		/*override public void SetParent(SynExpr parent)
		{
			this.parent = parent;
		}*/
	}

	class SqBrkOper : RefOper
	{
		public new SynExpr child;

		public SqBrkOper(Token t) : base(t) { }

		public void SetChild(SynExpr index)
		{
			child = index;
		}

		public override void Check()
		{
			if (!(parent.type is SymTypeArray))
			{
				Symbol.Exception e = new Symbol.Exception("отсутствует оператор \"[]\" соответствующий этим операндам", oper.pos, oper.line);
				e.Data["delayed"] = true;
				throw e;
			}

			this.type = ((SymTypeArray)parent.type).type;
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
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + SEP + "CAST" + SEP + type_name.ToString() + operand.ToString(level + 1);
		}

		public override void Check()
		{
			this.type = type_name;
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
	}

	class SynInit : SynExpr
	{
		SynExpr val;
		public SynInit(SynExpr v)
		{
			this.val = v;
			line = v.line;
			pos = v.pos;
		}

		public override string ToString(int level = 0)
		{
			return val.ToString();
		}

		public override void Check()
		{
			this.type = val.type;
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
	}

#endregion 

#region Statement

	abstract class SynStmt : SynObj { }

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
	}

	class StmtBLOCK : SynStmt
	{
		ArrayList list = new ArrayList();
		public StmtBLOCK(ArrayList stmts)
		{
			list = stmts;
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
	}

	class StmtCONTINUE : SynStmt 
	{
		public StmtCONTINUE() { }

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "CONTINUE";
		}
	}

	class StmtBREAK : SynStmt 
	{
		public StmtBREAK(){ }

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "BREAK";
		}
	}

#endregion

}
