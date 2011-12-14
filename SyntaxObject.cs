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

		public enum Type 
		{
			OP_PREFIX, OP_POSTFIX, OP_INFIX, OP_ASSIGN, OP_CAST, OP_TERN, OP_SIZEOF, F_CALL, CONST, IDENTIFIER,

			STMT_IF, STMT_BLOCK, STMT_FOR, STMT_WHILE, STMT_DO, STMT_SWITCH, STMT_CASE, STMT_RETURN, STMT_BREAK, STMT_CONTINUE
		};

		public Type type;
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
		abstract public SymType getType();
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

		override public SymType getType()
		{
			if (this.list.Count == 0)
			{
				return null;
			}

			return this.list[0].getType();
		}
	}

	class BinaryOper : SynExpr
	{
		protected SynExpr lnode = null, rnode = null;
		protected Token oper = null;

		public BinaryOper(Token op)
		{
			type = Type.OP_INFIX;
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

		public void Check()
		{
			if (!lnode.getType().Compatible(rnode.getType()))
			{
				throw new Symbol.Exception("тип \"" + lnode.getType().name + "\" несовместим с типом \"" + rnode.getType().ToString() +"\".", rnode.line, rnode.pos);
			}
		}

		override public SymType getType()
		{
			SymType lt = lnode.getType(), rt = rnode.getType();
			if (rt == null || lt == null)
			{
				return null;
			}

			switch (oper.type)
			{
 				case Token.Type.OP_DIV:
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
					if (lt is SymTypeDouble || rt is SymTypeDouble)
					{
						return lt is SymTypeDouble ? lt : rt;
					}

					return lt;

				case Token.Type.OP_MOD:

				case Token.Type.OP_LESS:
				case Token.Type.OP_LESS_OR_EQUAL:
				case Token.Type.OP_MORE:
				case Token.Type.OP_MORE_OR_EQUAL:
				case Token.Type.OP_EQUAL:
				case Token.Type.OP_NOT_EQUAL:
				case Token.Type.OP_OR:
				case Token.Type.OP_AND:

				case Token.Type.OP_XOR:
				case Token.Type.OP_BIT_OR:
				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_L_SHIFT:
				case Token.Type.OP_R_SHIFT:
					if (lt is SymTypeInt && rt is SymTypeInt)
					{
						return lt;
					}
					return null;

				default:
					return null;
			}
		}
	}

	class AssignOper : BinaryOper
	{
		public AssignOper(Token op)
			: base(op)
		{
			type = Type.OP_ASSIGN;
		}
	}

	class ConstExpr : SynExpr
	{
		protected Token token;
		public SymType type = null;

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

		override public SymType getType()
		{
			return type;
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
			type = Type.IDENTIFIER;
			var = v;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "требуется идентификатор");
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + token.strval;
		}

		override public SymType getType()
		{
			return var.type;
		}
	}

	abstract class UnaryOper : SynExpr
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
	}

	class PrefixOper : UnaryOper
	{
		public PrefixOper(Token op) : base(op) { type = Type.OP_PREFIX; }

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + oper.strval + "@" + SEP;
			s += operand.ToString(level + 1);
			return s;
		}

		override public SymType getType()
		{
			return operand.getType();
		}
	}

	class PostfixOper : UnaryOper
	{
		public PostfixOper(Token op) : base(op) { type = Type.OP_POSTFIX; }

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += SEP + "@" + oper.strval + SEP;
			s += operand.ToString(level + 1);
			return s;
		}

		override public SymType getType()
		{
			return operand.getType();
		}
	}

	class CallOper : SynExpr
	{
		SynExpr operand;
		ArrayList args = new ArrayList();

		public CallOper(SynExpr opnd, int line, int pos)
		{
			type = Type.F_CALL;
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

		override public SymType getType()
		{
			return operand.getType();
		}
	}

	class TerOper : SynExpr
	{
		SynExpr cond, lnode, rnode;

		public TerOper(SynExpr cond)
		{
			type = Type.OP_TERN;
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

		override public SymType getType()
		{
			return lnode.getType();
		}
	}

	class RefOper : SynExpr
	{
		protected Token oper;
		protected SynExpr parent, child;

		public RefOper(Token op)
		{
			type = Type.OP_POSTFIX;
			oper = op;
		}

		virtual public void SetParent(SynExpr parent)
		{
			this.parent = (SynExpr)CheckSynObj(parent);
		}

		virtual public void SetChild(SynExpr child)
		{
			this.child = (SynExpr)CheckSynObj(child, "требуется имя члена");
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

		override public SymType getType()
		{
			return child.getType();
		}
	}

	class DotOper : RefOper
	{
		public DotOper(Token t) : base(t) { }
		override public void SetParent(SynExpr parent)
		{
			this.parent = parent;
		}

		public void SetChild(IdentExpr child)
		{
			line = child.line;
			pos = child.pos;
		}
	}

	class SqBrkOper : RefOper
	{
		public SqBrkOper(Token t) : base(t) { }
	}

	class CastExpr : SynExpr
	{
		SynObj operand;
		SymType type_name;

		public CastExpr(SymType type_name, int line, int pos)
		{
			type = Type.OP_CAST;
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

		override public SymType getType()
		{
			return type_name;
		}
	}

	class SizeofOper : PrefixOper
	{
		new SymType operand;
		public SizeofOper(Token op) : base(op) 
		{
			type = Type.OP_SIZEOF;
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

		override public SymType getType()
		{
			return new SymTypeInt();
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

		override public SymType getType()
		{
			return val.getType();
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

		override public SymType getType()
		{
			return null;
		}
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
			type = Type.STMT_IF;
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
			type = Type.STMT_BLOCK;
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

		public StmtFOR()
		{
			type = Type.STMT_FOR;
		}

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
			type = Type.STMT_WHILE;
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
			type = Type.STMT_DO;
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
			type = Type.STMT_RETURN;
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
		public StmtCONTINUE()
		{
			type = Type.STMT_CONTINUE;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "CONTINUE";
		}
	}

	class StmtBREAK : SynStmt 
	{
		public StmtBREAK()
		{
			type = Type.STMT_BREAK;
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + "BREAK";
		}
	}

#endregion

}
