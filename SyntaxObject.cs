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
			OP_PREFIX, OP_POSTFIX, OP_INFIX, OP_ASSIGN, OP_CAST, OP_TERN, F_CALL, CONST, IDENTIFIER
		};

		public Type type;
		public Token token;
		public ArrayList children = new ArrayList();

		protected void AddChild(SynObj syn)
		{
			if(syn != null)
			{
				children.Add(syn);
			}
		}

		public override string ToString()
		{
			return token.strval;
		}

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

	class Expr : SynObj
	{
		public Expr(ArrayList expr)
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
		public AssignOper(Token op, SynObj l, SynObj r) : base(op, l, r)
		{
			type = Type.OP_ASSIGN;
		}
	}

	class ConstExpr : SynObj
	{
		public ConstExpr(Token t)
		{
			type = Type.CONST;
			token = t;
		}
	}

	class IdentExpr : SynObj
	{
		public IdentExpr(Token t)
		{
			type = Type.IDENTIFIER;
			token = CheckToken(t, Token.Type.IDENTIFICATOR, "идентификатор");
		}
	}

	class TypeNameExp : SynObj
	{
		public TypeNameExp(Token t)
		{
			type = Type.IDENTIFIER;
			token = t;
		}
	}

	abstract class UnaryOper : SynObj
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

	class BinaryOper : SynObj
	{
		public BinaryOper(Token op, SynObj l, SynObj r)
		{
			type = Type.OP_INFIX;
			token = op;
			AddChild(CheckSynObj(l, "требуется выражение"));
			AddChild(CheckSynObj(r, "требуется выражение"));
		}
	}

	class CallOper : SynObj
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

	class RefOper : SynObj
	{
		public RefOper(Token op, SynObj l, SynObj r)
		{
			type = Type.OP_POSTFIX;
			token = op;
			
			AddChild(CheckSynObj(l, "требуется выражение"));
			AddChild(CheckSynObj(r, "требуется имя члена"));
		}
	}

	/*class CastOper : SynObj
	{
		public CastOper(SynObj type_cast, SynObj opnd)
		{
			type = Type.OP_CAST;
			AddChild(opnd);
			AddChild(type_cast);
		}

		public override string ToString()
		{
			return "CAST";
		}
	}
	*/
	
	class TerOper : SynObj
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
}
