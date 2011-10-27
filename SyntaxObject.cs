using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class SynObj
	{
		public enum Type
		{
			OP_PREFIX, OP_POSTFIX, OP_INFIX, OP_ASSIGN, OP_CAST, OP_TERN, F_CALL, CONST, IDENTIFIER
		};

		public class Exception : System.Exception
		{
			public Exception(string s) : base(s) { }
		}

		public ArrayList children = new ArrayList();
		public Type type;

		public static Token CheckToken(Token t, Token.Type t_type, string s)
		{
			if (t.type != t_type)
			{
				throw new SynObj.Exception("отсутствует " + s);
			}

			return t;
		}

		public static SynObj CheckSynObj(SynObj obj, string s)
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

	#region expression

	class SynExpr: SynObj
	{
		public Token token;

		public override string ToString()
		{
			return token.strval;
		}

		protected void AddChild(SynObj syn)
		{
			if (syn != null)
			{
				children.Add(syn);
			}
		}
	}

	class AssignOper : BinaryOper
	{
		public AssignOper(Token op, SynObj l, SynObj r) : base(op, l, r)
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

	abstract class UnaryOper : SynExpr
	{
		public UnaryOper(Token op, SynObj opd)
		{
			token = op;
			AddChild(CheckSynObj(opd, "требуется выражение"));
		}
	}

	class PrefixOper : UnaryOper
	{
		public PrefixOper(Token op, SynObj opnd) : base(op, opnd) { type = Type.OP_PREFIX; }

		override public string ToString() { return token.strval + "@"; }
	}

	class PostfixOper : UnaryOper
	{
		public PostfixOper(Token op, SynObj opnd) : base(op, opnd) { type = Type.OP_POSTFIX; }

		override public string ToString() { return "@" + token.strval; }
	}

	class BinaryOper : SynExpr
	{
		public BinaryOper(Token op, SynObj l, SynObj r)
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
		public RefOper(Token op, SynObj l, SynObj r)
		{
			type = Type.OP_POSTFIX;
			token = op;
			
			AddChild(CheckSynObj(l, "требуется выражение"));
			AddChild(CheckSynObj(r, "требуется имя члена"));
		}
	}

	class TerOper : SynExpr
	{
		public TerOper(SynObj opnd, SynObj branch1, SynObj branch2)
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

	class SynExrList : SynExpr
	{
		public SynExrList(ArrayList list)
		{
			children = list;
		}

		public override string ToString()
		{
			return "EXPR";
		}
	}

#endregion

	#region statement

	class SynStmt : SynObj
	{
		string stmt = "";
		override public string ToString()
		{
			return 
		}
	}

	class StmtExpr : 
	{
		public StmtExpr(ArrayList ch)
		{
			children = ch;
		}

		public override string ToString()
		{
			return "EXPR";
		}
	}

	#endregion
}
