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

		Scaner scan;

		private Token CheckToken(Token t, Token.Type type)
		{
			return SynObj.CheckToken(t, type, Token.type_to_terms[type].ToString());
		}

		public Parser(Scaner sc)
		{
			scan = sc;
		}

		private void PassExpr()
		{
			Token t = new Token();
			do
			{
				try
				{
					t = scan.Read();
				}
				catch (Scaner.Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
			while (t.type != Token.Type.EOF && t.type != Token.Type.SEMICOLON);
			
		}

		public void Parse()
		{
			Root tree = new Root();

			do
			{
				try
				{
					try
					{
						tree.children.Add(ParseStmExpression());
					}
					catch (SynObj.Exception e)
					{
						Console.WriteLine("Ошибка в строке " + scan.GetLine() + " позиции " + scan.GetPos() + ": " + e.Message);
						PassExpr();
					}
				}
				catch (Scaner.Exception e)
				{
					Console.WriteLine(e.Message);
					PassExpr();
				}
			}
			while (scan.Peek().type != Token.Type.EOF);

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

				if (scan.Peek().type != Token.Type.COMMA)
				{
					break;
				}

				scan.Read();
			}

			return expr_list.Count == 0? null : new StmtExpr(expr_list);
		}

		private SynObj ParseBinaryOper(int level, SynObj lnode)
		{
			while (true)
			{

				SynObj.CheckSynObj(lnode, "требуется выражение");

				if (scan.Peek().type == Token.Type.QUESTION)
				{
					scan.Read();
					SynObj expr = ParseExpression();
					CheckToken(scan.Peek(), Token.Type.COLON);
					scan.Read();
					lnode = new TerOper(lnode, expr, ParseExpression()/*ParseNotAssignmentExpression()*/);
				}

				int op_level = GetOperatorPriority(scan.Peek());

				if (op_level < level)
				{
					return lnode;
				}

				Token oper = scan.Read();

				SynObj rnode = SynObj.CheckSynObj(ParseUnaryExpr(), "требуется выражение");

				int level_next_oper = GetOperatorPriority(scan.Peek());

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
			switch (scan.Peek().type)
			{
				case Token.Type.CONST_CHAR:
				case Token.Type.CONST_DOUBLE:
				case Token.Type.CONST_INT:
				case Token.Type.CONST_STRING:
					return new ConstExpr(scan.Read());

				case Token.Type.IDENTIFICATOR:
					return new IdentExpr(scan.Read());

				case Token.Type.LPAREN:
					scan.Read();
					SynObj res = ParseExpression();

					CheckToken(scan.Read(), Token.Type.RPAREN);
					return res;

				default:
					return null;
			}
		}

		private SynObj ParsePostfixExpr()
		{
			SynObj node = ParsePrimaryExpr();
			
			while(true){
				
				switch (scan.Peek().type)
				{
					case Token.Type.OP_INC:
					case Token.Type.OP_DEC:
						node = new PostfixOper(scan.Read(), node);
						break;

					case Token.Type.OP_DOT:
					case Token.Type.OP_REF:
						node = new RefOper(scan.Read(), node, scan.Peek().type == Token.Type.IDENTIFICATOR? new IdentExpr(scan.Read()): null);
						break;

					case Token.Type.LPAREN:
						scan.Read();

						if (scan.Peek().type == Token.Type.RPAREN)
						{
							scan.Read();
							node = new CallOper(node);
							break;
						}

						SynObj args = ParseExpression();
						CheckToken(scan.Peek(), Token.Type.RPAREN);
						scan.Read();
						node = new CallOper(node, args == null? new ArrayList(): args.children);
						break;

					case Token.Type.LBRACKET:
						
						node = new RefOper(scan.Read(), node, ParseExpression());
						CheckToken(scan.Peek(), Token.Type.RBRACKET);
						scan.Read();
						break;
						
					default:
						return node;
				}
			}
		}

		private SynObj ParseUnaryExpr()
		{
			switch (scan.Peek().type)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
					return new PrefixOper(scan.Read(), ParseUnaryExpr());

				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_STAR:
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
				case Token.Type.OP_TILDE:
				case Token.Type.OP_NOT:
					return new PrefixOper(scan.Read(), ParseUnaryExpr());

				case Token.Type.KW_SIZEOF:
					Token kw = scan.Read();

					if (scan.Peek().type != Token.Type.LPAREN)
					{
						return new PrefixOper(kw, ParseUnaryExpr());
					}

					scan.Read();
					SynObj res = new PrefixOper(kw, ParseTypeName());
					CheckToken(scan.Peek(), Token.Type.RPAREN);
					scan.Read();
					return res;

				default:
					return ParsePostfixExpr();
			}
		}

		private SynObj ParseTypeName()
		{
			switch (scan.Peek().type)
			{
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_INT:
				case Token.Type.KW_CHAR:
					return new TypeNameExp(scan.Read());

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

		#region parse declaration

		private SynObj ParseDeclarationSpecifier()
		{
			switch (scan.Peek().type)
			{
				case Token.Type.KW_TYPEDEF:
				case Token.Type.KW_EXTERN:
				case Token.Type.KW_STATIC:
					return null;
				case Token.Type.KW_VOID:
				case Token.Type.KW_CHAR:
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_INT:
				case Token.Type.KW_ENUM:
				case Token.Type.KW_STRUCT:
					return null;
				case Token.Type.KW_CONST:
					return null;
				default:
					return null;
			}
		}

		#endregion

		#region parse statment

		private SynObj ParseStmExpression()
		{
			SynObj res = ParseExpression();
			CheckToken(scan.Read(), Token.Type.SEMICOLON);
			return res;
		}

		#endregion
	}
}