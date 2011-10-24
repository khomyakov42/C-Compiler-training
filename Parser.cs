using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class Parser
	{
		class Exception : System.Exception
		{
			public Exception(string message, int line, int pos) :
				base("Ошибка синтаксиса в строке " + line + " позиции " + pos + ": " + message + "\n") {}
		}

		class Buffer
		{
			Token next;
			Scaner scan;
			int line = 0, pos = 0;

			public Buffer(Scaner sc)
			{
				scan = sc;
				UpdateCursor();
				next = scan.Read();
			}

			private void UpdateCursor()
			{
				pos = scan.GetPos();
				line = scan.GetLine();
			}

			public Token Read()
			{
				Token res = next;
				UpdateCursor();
				next = scan.Read();
				return res;
			}

			public Token Peek()
			{
				return next;
			}

			public int GetPos()
			{
				return pos;
			}

			public int GetLine()
			{
				return line;
			}
		}

		Buffer buf;

		private Token CheckToken(Token t, Token.Type type)
		{
			return SynObj.CheckToken(t, type, Token.type_to_terms[type].ToString());
		}

		public Parser(Scaner sc)
		{
			buf = new Buffer(sc);
		}

		private void PassExpr()
		{
			while (buf.Peek().type != Token.Type.EOF && buf.Peek().type != Token.Type.SEMICOLON) { buf.Read(); };
			buf.Read();
		}

		public void Parse()
		{
			SynObj tree = new Root();

			while (buf.Peek().type != Token.Type.EOF)
			{
				try
				{
					tree.children.Add(ParseStmExpression());
				}
				catch (SynObj.Exception e)
				{
					Console.Write("Ошибка в строке " + buf.GetLine() + " позиции " + buf.GetPos()  + ": " + e.Message + "\n");
					PassExpr();
				}
			}

			printTree(tree);

			Console.Write('\n');
		}

		#region parse expression

		private SynObj ParseExpression()
		{
			SynObj node = null;
			ArrayList expr_list = new ArrayList();

			while(true)
			{
				node = ParseUnaryExpr();
				
				node = ParseBinaryOper(0, node);
				if (node != null)
				{
					expr_list.Add(node);
				}

				if (buf.Peek().type != Token.Type.COMMA)
				{
					break;
				}

				buf.Read();
			}

			return expr_list.Count == 0? null : new Expr(expr_list);
		}

		private SynObj ParseBinaryOper(int level, SynObj lnode)
		{
			while (true)
			{
				if (buf.Peek().type == Token.Type.QUESTION)
				{
					buf.Read();
					SynObj expr = ParseExpression();
					CheckToken(buf.Peek(), Token.Type.COLON);
					buf.Read();
					lnode = new TerOper(lnode, expr, ParseExpression()/*ParseNotAssignmentExpression()*/);
				}

				int op_level = GetOperatorPriority(buf.Peek());

				if (op_level < level)
				{
					return lnode;
				}

				Token oper = buf.Read();

				SynObj rnode = ParseUnaryExpr();

				int level_next_oper = GetOperatorPriority(buf.Peek());

				if (op_level < level_next_oper)
				{
					rnode = ParseBinaryOper(op_level + 1, rnode);
				}

				if (GetOperatorPriority(oper) == 2)// assign operator
				{
					lnode = new AssignOper(oper, lnode, rnode);
				}
				else
				{
					lnode = new BinaryOper(oper, lnode, rnode);
				}
			}
		}

		private SynObj ParsePrimaryExpr()
		{
			switch (buf.Peek().type)
			{
				case Token.Type.CONST_CHAR:
				case Token.Type.CONST_DOUBLE:
				case Token.Type.CONST_INT:
				case Token.Type.CONST_STRING:
					return new ConstExpr(buf.Read());

				case Token.Type.IDENTIFICATOR:
					return new IdentExpr(buf.Read());

				case Token.Type.LPAREN:
					buf.Read();
					SynObj res = ParseExpression();

					CheckToken(buf.Read(), Token.Type.RPAREN);
					return res;

				default:
					return null;
			}
		}

		private SynObj ParsePostfixExpr()
		{
			SynObj node = ParsePrimaryExpr();
			
			while(true){
				
				switch (buf.Peek().type)
				{
					case Token.Type.OP_INC:
					case Token.Type.OP_DEC:
						node = new PostfixOper(buf.Read(), node);
						break;

					case Token.Type.OP_DOT:
					case Token.Type.OP_REF:
						node = new RefOper(buf.Read(), node, buf.Peek().type == Token.Type.IDENTIFICATOR? new IdentExpr(buf.Read()): null);
						break;

					case Token.Type.LPAREN:
						buf.Read();

						if (buf.Peek().type == Token.Type.RPAREN)
						{
							buf.Read();
							node = new CallOper(node);
							break;
						}

						SynObj args = ParseExpression();
						CheckToken(buf.Peek(), Token.Type.RPAREN);
						buf.Read();
						node = new CallOper(node, args == null? new ArrayList(): args.children);
						break;

					case Token.Type.LBRACKET:
						
						node = new RefOper(buf.Read(), node, ParseExpression());
						CheckToken(buf.Peek(), Token.Type.RBRACKET);
						buf.Read();
						break;
						
					default:
						return node;
				}
			}
		}

		private SynObj ParseUnaryExpr()
		{
			switch (buf.Peek().type)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
					return new PrefixOper(buf.Read(), ParseUnaryExpr());

				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_STAR:
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
				case Token.Type.OP_TILDE:
				case Token.Type.OP_NOT:
					return new PrefixOper(buf.Read(), ParseUnaryExpr());

				case Token.Type.KW_SIZEOF:
					Token kw = buf.Read();

					if (buf.Peek().type != Token.Type.LPAREN)
					{
						return new PrefixOper(kw, ParseUnaryExpr());
					}

					buf.Read();
					SynObj res = new PrefixOper(kw, ParseTypeName());
					CheckToken(buf.Peek(), Token.Type.RPAREN);
					buf.Read();
					return res;

				default:
					return ParsePostfixExpr();
			}
		}

		private SynObj ParseTypeName()
		{
			switch (buf.Peek().type)
			{
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_INT:
				case Token.Type.KW_CHAR:
					return new TypeNameExp(buf.Read());

				case Token.Type.KW_STRUCT:
					return null;

				default:
					return null;
			}
		}

		private int GetOperatorPriority(Token t)
		{
			switch (t.type)
			{
				case Token.Type.OP_STAR:
				case Token.Type.OP_DIV:
				case Token.Type.OP_MOD:
					return 13;
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
					return 12;
				case Token.Type.OP_L_SHIFT:
				case Token.Type.OP_R_SHIFT:
					return 11;
				case Token.Type.OP_MORE:
				case Token.Type.OP_MORE_OR_EQUAL:
				case Token.Type.OP_LESS:
				case Token.Type.OP_LESS_OR_EQUAL:
					return 10;
				case Token.Type.OP_EQUAL:
				case Token.Type.OP_NOT_EQUAL:
					return 9;
				case Token.Type.OP_BIT_AND:
					return 8;
				case Token.Type.OP_XOR:
					return 7;
				case Token.Type.OP_BIT_OR:
					return 6;
				case Token.Type.OP_AND:
					return 5;
				case Token.Type.OP_OR:
					return 4;
				case Token.Type.OP_ASSIGN:
				case Token.Type.OP_BIT_AND_ASSIGN:
				case Token.Type.OP_BIT_OR_ASSIGN:
				case Token.Type.OP_DIV_ASSIGN:
				case Token.Type.OP_L_SHIFT_ASSIGN:
				case Token.Type.OP_MOD_ASSIGN:
				case Token.Type.OP_MUL_ASSIGN:
				case Token.Type.OP_PLUS_ASSIGN:
				case Token.Type.OP_R_SHIFT_ASSIGN:
				case Token.Type.OP_SUB_ASSIGN:
				case Token.Type.OP_XOR_ASSIGN:
					return 2;
				default:
					return -1;
			}
		}

		private void printTree(SynObj node, int level = 0, bool endbranch = false)
		{
			const int padd = 5;

			string s = "";
			if (level > 0)
				s = new String(' ', (level - 1) * padd) + new String(' ', padd - 1);

			Console.Write(s + "\"" + node.ToString() + "\"");

			for (int i = 0; i < node.children.Count; i++)
			{
				Console.Write('\n');
				printTree((SynObj)node.children[i], level + 1, i == node.children.Count - 1);
			}
		}

		#endregion

		#region parse statment

		private SynObj ParseStmExpression()
		{
			SynObj res = ParseExpression();
			CheckToken(buf.Read(), Token.Type.SEMICOLON);
			return res;
		}

		#endregion
	}
}