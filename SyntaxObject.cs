using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class SynObj
	{
		public class Exception : System.Exception
		{
			public Exception(string s) : base(s) { }
		}

		public enum Type 
		{
			OP_PREFIX, OP_POSTFIX, OP_INFIX, OP_ASSIGN, OP_CAST, OP_TERN, F_CALL, CONST, IDENTIFIER,

			STMT_IF, STMT_BLOCK, STMT_FOR, STMT_WHILE, STMT_DO, STMT_SWITCH, STMT_CASE, STMT_RETURN, STMT_BREAK, STMT_CONTINUE
		};

		public Type type;
		public ArrayList children = new ArrayList();

		protected void AddChild(SynObj syn)
		{
			if(syn != null)
			{
				children.Add(syn);
			}
		}

		protected void InsertChildren(int pos, SynObj syn)
		{
			if(syn != null)
			{
				children.Insert(pos, syn);
			}
		}

		public static Token CheckToken(Token t, Token.Type t_type, string s)
		{
			if (t.type != t_type)
			{
				throw new SynObj.Exception("отсутствует " + s);
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
	}

	class Root : SynObj
	{
		public override string ToString()
		{
			return "ROOT";
		}
	}

#region Expression

	class SynExpr: SynObj
	{
		public Token token;

		public override string ToString()
		{
			return token.strval;
		}
	}

	class ExprList : SynExpr
	{
		public ExprList(ArrayList expr)
		{
			children = expr;
		}

		public override string ToString()
		{
			return "EXPR";
		}
	}

	class AssignOper : BinaryOper
	{
		public AssignOper(Token op, SynExpr l, SynExpr r)
			: base(op, l, r)
		{
			type = Type.OP_ASSIGN;
		}
	}

	class ConstExpr : SynExpr
	{
		public ConstExpr(Token t)
		{
			type = Type.CONST;
			token = t;
		}
	}

	class IdentExpr : SynExpr
	{
		public IdentExpr(Token t)
		{
			type = Type.IDENTIFIER;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "идентификатор");
		}
	}

	class TypeNameExp : SynExpr
	{
		public TypeNameExp(Token t)
		{
			type = Type.IDENTIFIER;
			token = t;
		}
	}

	class UnaryOper : SynExpr
	{
		public UnaryOper(Token op, SynExpr opd)
		{
			token = op;
			AddChild(CheckSynObj(opd, "требуется выражение"));
		}
	}

	class PrefixOper : UnaryOper
	{
		public PrefixOper(Token op, SynExpr opnd) : base(op, opnd) { type = Type.OP_PREFIX; }

		override public string ToString() { return token.strval + "@"; }
	}

	class PostfixOper : UnaryOper
	{
		public PostfixOper(Token op, SynExpr opnd) : base(op, opnd) { type = Type.OP_POSTFIX; }

		override public string ToString() { return "@" + token.strval; }
	}

	class BinaryOper : SynExpr
	{
		public BinaryOper(Token op, SynExpr l, SynExpr r)
		{
			type = Type.OP_INFIX;
			token = op;
			AddChild(CheckSynObj(l, "требуется выражение"));
			AddChild(CheckSynObj(r, "требуется выражение"));
		}
	}

	class CallOper : SynExpr
	{

		class ListArgs: SynObj
		{
			public ListArgs(ArrayList args = null)
			{
				children = args;
			}

			public override string ToString()
			{
				return "ARGS";
			}
		}

		public CallOper(SynObj opnd, ArrayList args = null)
		{
			type = Type.F_CALL;
			AddChild(opnd);
			AddChild(new ListArgs(args));
		}

		public override string ToString()
		{
			return "CALL";
		}
	}

	class RefOper : SynExpr
	{
		public RefOper(Token op, SynExpr l, SynExpr r)
		{
			type = Type.OP_POSTFIX;
			token = op;
			
			AddChild(CheckSynObj(l, "требуется выражение"));
			AddChild(CheckSynObj(r, "требуется имя члена"));
		}
	}

	class TerOper : SynExpr
	{
		public TerOper(SynExpr opnd, SynExpr branch1, SynExpr branch2)
		{
			type = Type.OP_TERN;
			AddChild(opnd);
			AddChild(CheckSynObj(branch1, "требуется выражение"));
			AddChild(CheckSynObj(branch2, "требуется выражение"));
		}

		public override string ToString()
		{
			return "?:";
		}
	}

#endregion 

#region Statement

	class SynStmt : SynObj
	{
		public override string ToString()
		{
			switch(type)
			{
				case Type.STMT_IF:
					return "IF";
				case Type.STMT_BLOCK:
					return "BLOCK";
				case Type.STMT_CASE:
					return "CASE";
				case Type.STMT_SWITCH:
					return "SWITCH";
				case Type.STMT_WHILE:
					return "WHILE";
				case Type.STMT_DO:
					return "DO WHILE";
				case Type.STMT_FOR:
					return "FOR";
				case Type.STMT_RETURN:
					return "RETURN";
				case Type.STMT_CONTINUE:
					return "CONTINUE";
				case Type.STMT_BREAK:
					return "BREAK";
				default:
					return "";
			}
		}
	}

	class StmtIF : SynStmt
	{
		public StmtIF(SynExpr opnd)
		{
			type = Type.STMT_IF;
			AddChild(CheckSynObj(opnd));
		}

		public void SetBranchTrue(SynObj br)
		{
			children.Insert(1, CheckSynObj(br));
		}

		public void SetBranchFalse(SynObj br) 
		{
			children.Insert(2, br);
		}
	}

	class StmtBLOCK : SynStmt
	{
		public StmtBLOCK(ArrayList stmts)
		{
			type = Type.STMT_BLOCK;
			children = stmts;
		}
	}

	class StmtFOR : SynStmt
	{
		class Counter : SynObj { }

		public StmtFOR()
		{
			type = Type.STMT_FOR;
			AddChild(new Counter());
		}

		public void SetCounter(SynObj opnd)
		{
			GetCounter().children.Insert(0, CheckSynObj(opnd));
		}

		public void SetCond(SynObj opnd)
		{
			GetCounter().children.Insert(1, CheckSynObj(opnd));
		}

		public void SetIncriment(SynObj opnd)
		{
			GetCounter().children.Insert(2, opnd);
		}

		public void SetBlock(SynObj opnd)
		{
			children.Insert(1, CheckSynObj(opnd));
		}

		private Counter GetCounter()
		{
			return (Counter)children[0];
		}
	}

	class StmtWHILE : SynStmt
	{
		public StmtWHILE(SynObj cond)
		{
			type = Type.STMT_WHILE;
			InsertChildren(0, CheckSynObj(cond));
		}

		public void SetBlock(SynObj opnd)
		{
			InsertChildren(1, CheckSynObj(opnd));
		}
	}

	class StmtDO : SynStmt
	{
		public StmtDO(SynObj block)
		{
			type = Type.STMT_DO;
			InsertChildren(1, CheckSynObj(block));
		}

		public void SetCond(SynObj cond)
		{
			InsertChildren(0, CheckSynObj(cond));
		}
	}

	class StmtCASE : SynStmt
	{
		public StmtCASE(SynObj expr)
		{
			type = Type.STMT_CASE;
			InsertChildren(0, CheckSynObj(expr));
		}

		public void SetBlock(SynObj block)
		{
			InsertChildren(1, block);
		}
	}

	class StmtSWITCH : SynStmt
	{
		public StmtSWITCH(SynObj obj)
		{
			type = Type.STMT_SWITCH;
			InsertChildren(0, CheckSynObj(obj));
		}

		public void AddCase(StmtCASE c)
		{
			AddChild(CheckSynObj(c));
		}
	}

	class StmtRETURN : SynStmt
	{
		public StmtRETURN(SynObj expr)
		{
			type = Type.STMT_RETURN;
			AddChild(expr);
		}
	}

	class StmtCONTINUE : SynStmt 
	{
		public StmtCONTINUE()
		{
			type = Type.STMT_CONTINUE;
		}
	}

	class StmtBREAK : SynStmt 
	{
		public StmtBREAK()
		{
			type = Type.STMT_BREAK;
		}
	}

#endregion
}
