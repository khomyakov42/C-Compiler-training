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

	abstract class SynExpr: SynObj { }

	class ExprList : SynExpr
	{
		ArrayList list;
		public ExprList(ArrayList exprs)
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

		public ConstExpr(Token t)
		{
			type = Type.CONST;
			switch (t.type)
			{
				case Token.Type.CONST_INT:
				case Token.Type.CONST_CHAR:
				case Token.Type.CONST_DOUBLE:
				case Token.Type.CONST_STRING:
					token = t;
					break;

				default:
					throw new SynObj.Exception("требуется константное выражение");
			}
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + token.strval;
		}
	}

	class IdentExpr : SynExpr
	{
		protected Token token;

		public IdentExpr(Token t)
		{
			type = Type.IDENTIFIER;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "требуется идентификатор");
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + token.strval;
		}
	}

	abstract class UnaryOper : SynExpr
	{
		protected Token oper;
		protected SynExpr operand;

		public UnaryOper(Token op)
		{
			oper = op;
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
	}

	class CallOper : SynExpr
	{
		SynExpr operand;
		ArrayList args = new ArrayList();

		public CallOper(SynExpr opnd)
		{
			type = Type.F_CALL;
			operand = opnd;
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
	}

	class TerOper : SynExpr
	{
		SynExpr cond, lnode, rnode;

		public TerOper(SynExpr cond)
		{
			type = Type.OP_TERN;
			this.cond = cond;
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
	}

	class RefOper : SynExpr
	{
		Token oper;
		SynExpr parent, child;

		public RefOper(Token op)
		{
			type = Type.OP_POSTFIX;
			oper = op;
		}

		public void SetParent(SynExpr parent)
		{
			this.parent = (SynExpr)CheckSynObj(parent);
		}

		public void SetChild(SynExpr child)
		{
			this.child = (SynExpr)CheckSynObj(child, "требуется имя члена");
		}

		public override string ToString(int level = 0)
		{
			string s = getIndentString(level);
			s += oper.strval;
			s += parent.ToString(level + 1);
			s += child.ToString(level + 1);
			return s;
		}
	}

	class CastExpr : SynExpr
	{
		SynObj type_name, operand;

		public CastExpr(SynObj type_name)
		{
			type = Type.OP_CAST;
			this.type_name = CheckSynObj(type_name);
		}

		public void SetOperand(SynExpr operand)
		{
			this.operand = CheckSynObj(operand);
		}

		public override string ToString(int level = 0)
		{
			return getIndentString(level) + SEP + "CAST" + SEP + type_name.ToString(level + 1) + operand.ToString(level + 1);
		}
	}

	class SizeofOper : PrefixOper
	{
		public SizeofOper(Token op) : base(op) { type = Type.OP_SIZEOF; }
	}

	class SynInit : SynExpr
	{
		SynExpr val;
		public SynInit(SynExpr v)
		{
			this.val = v;
		}

		public override string ToString(int level = 0)
		{
			return val.ToString();
		}
	}

	class SynInitList : SynExpr
	{
		List<SynInit> list = new List<SynInit>();

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
