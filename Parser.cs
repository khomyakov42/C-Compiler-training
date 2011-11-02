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

		private Token CheckToken(Token t, Token.Type type, bool get_next_token = false)
		{
			SynExpr.CheckToken(t, type, Token.type_to_terms[type].ToString());

			return get_next_token ? scan.Read() : t;
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
						tree.AddChild(ParseStmt());
					}
					catch (SynExpr.Exception e)
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

			Console.Write(tree.ToString() + '\n' + '\n');
		}

		#region parse expression

		private SynExpr ParseExpression(bool bind = true, bool cond_expr = false)
		{
			SynExpr node = null;
			ArrayList expr_list = new ArrayList();

			while(true)
			{
				node = ParseUnaryExpr();

				if (node == null && !bind)
				{
					return new ExprList(expr_list);
				}
				
				node = ParseBinaryOper(0, node, cond_expr);
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

			return expr_list.Count == 0 ? null : new ExprList(expr_list);
		}

		private SynExpr ParseBinaryOper(int level, SynExpr lnode, bool cond_expr = false)
		{
			SynExpr root = null;

			while (true)
			{

				SynExpr.CheckSynObj(lnode);

				if (scan.Peek().type == Token.Type.QUESTION)
				{
					scan.Read();
					SynExpr expr = ParseExpression();
					CheckToken(scan.Peek(), Token.Type.COLON, true);

					root = new TerOper(lnode);
					((TerOper)root).SetTrueExpr(expr);
					((TerOper)root).SetFalseExpr(ParseExpression());
					lnode = root;
				}

				int op_level = GetOperatorPriority(scan.Peek());

				if (op_level < level)
				{
					return lnode;
				}

				if (cond_expr && op_level == 2)
				{
					throw new SynObj.Exception("недопустимый оператор в константном выражении");
				}

				Token oper = scan.Read();

				SynExpr rnode = (SynExpr)SynObj.CheckSynObj(ParseUnaryExpr());

				int level_next_oper = GetOperatorPriority(scan.Peek());

				if (op_level < level_next_oper)
				{
					rnode = ParseBinaryOper(op_level + 1, rnode, cond_expr);
				}

				if (GetOperatorPriority(oper) == 2)// assign operator
				{
					root = new AssignOper(oper);
					((AssignOper)root).SetLeftOperand(lnode);
					((AssignOper)root).SetRightOperand(rnode);
				}
				else
				{
					root = new BinaryOper(oper);
					((BinaryOper)root).SetLeftOperand(lnode);
					((BinaryOper)root).SetRightOperand(rnode);
				}
				lnode = root;
			}
		}

		private SynExpr ParsePrimaryExpr()
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
					SynExpr res = ParseExpression();

					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					return res;

				default:
					return null;
			}
		}

		private SynExpr ParsePostfixExpr(SynExpr expr_node = null)
		{
			SynExpr node = expr_node == null? ParsePrimaryExpr(): expr_node, res = null;


			while(true){
				
				switch (scan.Peek().type)
				{
					case Token.Type.OP_INC:
					case Token.Type.OP_DEC:
						res = new PostfixOper(scan.Read());
						((PostfixOper)res).SetOperand(node);
						break;

					case Token.Type.OP_DOT:
					case Token.Type.OP_REF:
						res = new RefOper(scan.Read());
						((RefOper)res).SetParent(node);
						((RefOper)res).SetChild(scan.Peek().type == Token.Type.IDENTIFICATOR? new IdentExpr(scan.Read()) : null);
						break;

					case Token.Type.LPAREN:
						scan.Read();
						res = new CallOper(node);

						if (scan.Peek().type == Token.Type.RPAREN)
						{
							scan.Read();
							break;
						}

						SynExpr args = ParseExpression();

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						((CallOper)res).AddArgument(args);
						break;

					case Token.Type.LBRACKET:
						res = new RefOper(scan.Read());
						((RefOper)res).SetParent(node);
						((RefOper)res).SetChild(ParseExpression());

						CheckToken(scan.Peek(), Token.Type.RBRACKET, true);
						break;
						
					default:
						return res == null? node: res;
				}

				node = res;
			}
		}

		private SynExpr ParseUnaryExpr(bool prev_is_unary_oper = true)
		{
			PrefixOper node = null;
			switch (scan.Peek().type)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
					node = new PrefixOper(scan.Read());
					node.SetOperand(ParseUnaryExpr(false));
					return node;

				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_STAR:
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
				case Token.Type.OP_TILDE:
				case Token.Type.OP_NOT:
					node = new PrefixOper(scan.Read());
					node.SetOperand(ParseUnaryExpr());
					return node;
				
				case Token.Type.LPAREN:
					scan.Read();
					SynExpr res = null;

					if (IsTypeName(scan.Peek()))
					{
						res = new CastExpr(ParseTypeName());
						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						((CastExpr)res).SetOperand(ParseUnaryExpr());
						return res;
					}

					res = ParseExpression();
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					return ParsePostfixExpr(res);

				case Token.Type.KW_SIZEOF:
					Token kw = scan.Read();

					if (scan.Peek().type != Token.Type.LPAREN)
					{
						node = new SizeofOper(kw);
						node.SetOperand(ParseUnaryExpr(false));
						return node;
					}

					scan.Read();
					node = new SizeofOper(kw);
					if (IsTypeName(scan.Peek()))
					{
						node.SetOperand(ParseTypeName());
					}
					else
					{
						node.SetOperand(ParseExpression());
					}

					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					return node;

				default:
					return ParsePostfixExpr();
			}
		}

		private SynExpr ParseTypeName()
		{
			switch (scan.Peek().type)
			{
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_INT:
				case Token.Type.KW_CHAR:
					return new IdentExpr(scan.Read());

				case Token.Type.KW_STRUCT:
					return null;

				default:
					return null;
			}
		}

		private bool IsTypeName(Token t)
		{
			switch (t.type)
			{
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_INT:
				case Token.Type.KW_CHAR:
				case Token.Type.KW_STRUCT:
					return true;
				default:
					return false;
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

		#endregion

		#region parse statment

		private SynObj ParseStmExpression()
		{
			SynObj res = ParseExpression(false);
			CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
			return res;
		}

		private SynObj ParseStmt()
		{
			SynObj node = null;

			switch (scan.Peek().type)
			{
				case Token.Type.KW_WHILE:
					scan.Read();
					CheckToken(scan.Peek(), Token.Type.LPAREN, true);

					node = new StmtWHILE(ParseExpression());
					
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					((StmtWHILE)node).SetBlock(ParseStmt());
					break;

				case Token.Type.KW_DO:
					scan.Read();
					
					node = new StmtDO(ParseStmt());
					CheckToken(scan.Peek(), Token.Type.KW_WHILE, true);
					CheckToken(scan.Peek(), Token.Type.LPAREN, true);
					
					((StmtDO)node).SetCond(ParseExpression());
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					break;

				case Token.Type.KW_FOR:
					scan.Read();
					CheckToken(scan.Peek(), Token.Type.LPAREN, true);
					node = new StmtFOR();
					((StmtFOR)node).SetCounter(ParseStmExpression());
					((StmtFOR)node).SetCond(ParseStmExpression());

					if (scan.Peek().type != Token.Type.RPAREN)
					{
						((StmtFOR)node).SetIncriment(ParseExpression());
					}

					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					((StmtFOR)node).SetBlock(ParseStmt());
					break;

				case Token.Type.KW_IF:
					scan.Read();
					CheckToken(scan.Peek(), Token.Type.LPAREN, true);
					node = new StmtIF(ParseExpression());
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					((StmtIF)node).SetBranchTrue(ParseStmt());
					if (scan.Peek().type == Token.Type.KW_ELSE)
					{
						scan.Read();
						((StmtIF)node).SetBranchFalse(ParseStmt());
					}
					break;

				case Token.Type.LBRACE:
					scan.Read();
					ArrayList stmts = new ArrayList();
					while (scan.Peek().type != Token.Type.RBRACE && scan.Peek().type != Token.Type.EOF)
					{
						stmts.Add(ParseStmt());
					}
					CheckToken(scan.Peek(), Token.Type.RBRACE, true);
					node = new StmtBLOCK(stmts);
					break;

				case Token.Type.KW_RETURN:
					scan.Read();
					if (scan.Peek().type != Token.Type.SEMICOLON)
					{
						node = new StmtRETURN(ParseExpression());
					}
					else
					{
						node = new StmtRETURN();
					}

					CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					break;

				case Token.Type.KW_BREAK:
					scan.Read();
					node = new StmtBREAK();
					CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					break;

				case Token.Type.KW_CONTINUE:
					scan.Read();
					node = new StmtCONTINUE();
					CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					break;

				default:
					node = ParseStmExpression();
					break;
			}

			return node;
		}
		#endregion
	}
}