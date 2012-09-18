using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class Parser
	{
		public Logger logger;
		public Symbols.StackTable tstack;
		private Scaner scan;
		private bool parse_const_expr = false;
		private int count_string = 0;
		private bool parse_block = false;
		private bool parse_args_declaraton = false;
		private Symbols.Type ret_func_type = null;
		private bool parse_cycle = false;
		private Syntax.Root tree;
		
		public Parser(Scaner sc)
		{
			this.scan = sc;
			this.tree = new Syntax.Root();
			this.logger = new Logger();
			this.tstack = new Symbols.StackTable();
			this.InitTableStack();
		}

		private void InitTableStack()
		{
			tstack.AddSymbol(new Symbols.INT("int", 0, 0));
			tstack.AddSymbol(new Symbols.CHAR("char", 0, 0));
			tstack.AddSymbol(new Symbols.DOUBLE("double", 0, 0));
			tstack.AddSymbol(new Symbols.VOID("void", 0, 0));

			
			Symbols.ExternFunc f = new Symbols.ExternFunc("printf");
			Symbols.GlobalVar v = new Symbols.GlobalVar("printf", 0, 0);
			Symbols.ParamVar str = new Symbols.ParamVar();
			v.SetType(f);
			str.SetType(new Symbols.POINTER(new Symbols.CHAR()));
			f.AddArgument(str);
			f.SetUnspecifiedArgs();
			tstack.AddSymbol(v);

			f = new Symbols.ExternFunc("scanf");
			v = new Symbols.GlobalVar("scanf", 0, 0);
			v.SetType(f);
			f.AddArgument(str);
			f.SetUnspecifiedArgs();
			tstack.AddSymbol(v);

			f = new Symbols.ExternFunc("getchar");
			v = new Symbols.GlobalVar("getchar", 0, 0);
			v.SetType(f);
			f.SetType(new Symbols.INT());
			tstack.AddSymbol(v);
		}

		private void PassExpr(String stop_chars = ";")
		{
			Token t = new Token();
			int count_br = 0;
			do
			{
				try
				{
					t = scan.Read();
					if (t.type == Token.Type.LBRACE)
					{
						count_br++;
					}
					else if (t.type == Token.Type.RBRACE)
					{
						count_br--;
					}
				}
				catch (Scaner.Exception e)
				{
					scan.Pass();
					logger.Add(e);
				}
			}
			while (!(t.type == Token.Type.EOF || 
				(t.GetStrVal().IndexOf(stop_chars) != -1 && !this.parse_args_declaraton) || 
				(this.parse_block && (t.type == Token.Type.RBRACE || count_br <= 0)) ||
				(this.parse_args_declaraton && (t.type == Token.Type.COMMA || scan.Peek().type == Token.Type.RPAREN))
			));
		}

		private Token CheckToken(Token t, Token.Type type, bool get_next_token = false)
		{
			Syntax.Object.CheckToken(t, type, "требуется \"" + Token.Types[type].ToString() + '"');

			return get_next_token ? scan.Read() : t;
		}
		
		private void ToHandlerException(Compiler.Exception e, String stop_chars = ";")
		{
			if (e.line == -1 && e.index == -1)
			{
				e.line = this.scan.GetLine();
				e.index = this.scan.GetPos();
			}

			this.logger.Add(e);
			if (!(e is Symbols.Exception))
			{
				this.PassExpr(stop_chars);
			}
		}
		
		public void Parse()
		{
			do
			{
				try
				{
					while (scan.Peek().type != Token.Type.EOF)
					{
						ParseDeclaration();
					}
				}
				catch (Compiler.Exception e)
				{
					ToHandlerException(e);
				}

			}
			while (scan.Peek().type != Token.Type.EOF);
		}

		public void PrintTree(StreamWriter srteam) 
		{
			this.tstack.Print(srteam);
		}

#region parse expression

		private Syntax.Expression ParseExpression()
		{
			Syntax.Expression node = ParseUnaryExpr();
			if (node != null) {
				node = ParseBinaryOper(0, node);
			}
			return node;
		}

		private List<Syntax.Expression> ParseArgExpressionList()
		{
			List<Syntax.Expression> list = new List<Syntax.Expression>();

			while (true)
			{
				list.Add(ParseAssignmentExpression());
				if (scan.Peek().type != Token.Type.COMMA)
				{
					return list;
				}
				scan.Read();
			}
		}

		private Syntax.Expression ParseAssignmentExpression()
		{
			Syntax.Expression node = ParseUnaryExpr();
			if (node == null)
			{
				return null;
			}
			return ParseBinaryOper(0, node, true);
		}

		private Syntax.Expression ParseBinaryOper(int level, Syntax.Expression lnode, bool stop_if_comma = false)
		{
			Syntax.Expression root = null;

			while (true)
			{

				Syntax.Object.CheckObject(lnode);

				if (scan.Peek().type == Token.Type.QUESTION)
				{
					scan.Read();
					Syntax.Expression expr = ParseExpression();
					CheckToken(scan.Peek(), Token.Type.COLON, true);
					root = new Syntax.Ternary(lnode);
					((Syntax.Ternary)root).SetBranchTrue(expr);
					((Syntax.Ternary)root).SetBranchFalse(ParseExpression());
					lnode = root;
				}

				int op_level = GetOperatorPriority(scan.Peek());

				if (op_level < level || (stop_if_comma && scan.Peek().type == Token.Type.COMMA))
				{
					return lnode;
				}

				if (this.parse_const_expr && op_level == 2)
				{
					throw new Syntax.Exception("недопустимый оператор в константном выражении", scan.GetPos(), scan.GetLine());
				}

				Token oper = scan.Read();

				Syntax.Expression rnode = ParseUnaryExpr();

				int level_next_oper = GetOperatorPriority(scan.Peek());

				try
				{
					if (op_level < level_next_oper)
					{
						rnode = ParseBinaryOper(op_level + 1, rnode, stop_if_comma);
					}
				
					root = new Syntax.BinaryOperator(oper);
					((Syntax.BinaryOperator)root).SetLeftOperand(lnode);
					((Syntax.BinaryOperator)root).SetRightOperand(rnode);
				}
				catch (Syntax.Exception e)
				{
					ToHandlerException(e);
				}

				lnode = root;
			}
		}

		private Syntax.Expression ParsePrimaryExpr()
		{
			Syntax.Expression res = null;
			switch (scan.Peek().type)
			{
				case Token.Type.CONST_CHAR:
					res = new Syntax.Const(scan.Read(), tstack.GetType("char"));
					break;

				case Token.Type.CONST_DOUBLE:
					res = new Syntax.Const(scan.Read(), tstack.GetType("double"));
					break;

				case Token.Type.CONST_INT:
					res = new Syntax.Const(scan.Read(), tstack.GetType("int"));
					break;

				case Token.Type.CONST_STRING:
					this.count_string++;
					Symbols.ARRAY strt = new Symbols.ARRAY(tstack.GetType("char"));
					Token str = scan.Read();
					strt.SetSize(new Syntax.Const(str.GetStrVal().Length.ToString(), tstack.GetType("int")));
					res = new Syntax.Const(str, strt);
					break;

				case Token.Type.IDENTIFICATOR:
					Token t = scan.Read();
					Symbols.Var v = new Symbols.SuperVar();
					try
					{
						v = tstack.GetVariable(t);
					}
					catch (Symbols.Exception e)
					{
						this.logger.Add(e);
					}

					res = new Syntax.Identifier(t, v);
					break;

				case Token.Type.LPAREN:
					scan.Read();
					 res = ParseExpression();

					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					break;
			}
			return res;
		}

		private Syntax.Expression ParsePostfixExpr(Syntax.Expression expr_node = null)
		{
			Syntax.Expression node = expr_node == null? this.ParsePrimaryExpr(): expr_node, res = null;

			while(true){
				switch (scan.Peek().type)
				{ 
					case Token.Type.OP_INC:
					case Token.Type.OP_DEC:
						res = new Syntax.UnaryOperator(scan.Read(), false);
						((Syntax.UnaryOperator)res).SetOperand(node);
						break;

					case Token.Type.OP_DOT:
					case Token.Type.OP_REF:
						res = new Syntax.Refference(scan.Read());
						((Syntax.Refference)res).SetExpression(node);
						CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
						((Syntax.Refference)res).SetRefference(new Syntax.Identifier(scan.Read()));
						break;

					case Token.Type.LPAREN:
						res = new Syntax.Call(scan.Read());
						((Syntax.Call)res).SetOperand(node);
						try
						{
							if (scan.Peek().type == Token.Type.RPAREN)
							{
								((Syntax.Call)res).SetArgumentList(new List<Syntax.Expression>());
								scan.Read();
								break;
							}
							((Syntax.Call)res).SetArgumentList(this.ParseArgExpressionList());
						}
						catch (Symbols.Exception e)
						{
							this.logger.Add(e);
						}

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						break;

					case Token.Type.LBRACKET:
						res = new Syntax.Refference(scan.Read());
						((Syntax.Refference)res).SetExpression(node);
						((Syntax.Refference)res).SetRefference(this.ParseExpression());
						CheckToken(scan.Peek(), Token.Type.RBRACKET, true);
						break;
						
					default:
						return res == null? node: res;
				}

				if (res != null)
				{
					node = res;
				}
			}
		}

		private Syntax.Expression ParseUnaryExpr()
		{
			Syntax.UnaryOperator node = null;

			switch (scan.Peek().type)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_STAR:
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
				case Token.Type.OP_TILDE:
				case Token.Type.OP_NOT:
					node = new Syntax.UnaryOperator(scan.Read());
					node.SetOperand(this.ParseUnaryExpr());
					return node;

				case Token.Type.LPAREN:
					Syntax.Expression res = null;
					Token t = scan.Read();

					if (IsType(scan.Peek()))
					{
						res = new Syntax.Cast(t);
						Symbols.ParamVar var = ParseParameterDeclaration(); //this is hack
						((Syntax.Cast)res).SetTypeCast(var.GetType());
						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						((Syntax.Cast)res).SetOperand(ParseUnaryExpr());
						return res;
					}

					res = ParseExpression();
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					return ParsePostfixExpr(res);

				case Token.Type.KW_SIZEOF:
					node = new Syntax.UnaryOperator(scan.Read());

					if (scan.Peek().type != Token.Type.LPAREN)
					{
						node.SetOperand(ParseUnaryExpr());
						return node;
					}

					scan.Read();
					if (IsType(scan.Peek()))
					{
						((Syntax.UnaryOperator)node).SetOperand(new Syntax.Identifier(scan.Read()));
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
				case Token.Type.COMMA:
					return 1;
				default:
					return -1;
			}
		}

		private bool IsType(Token t)
		{
			switch (t.type)
			{
				case Token.Type.KW_STRUCT:
				case Token.Type.KW_ENUM:
					return true;
				
				default:
					Symbols.Type tt = null;
					if (tstack.ContainsType(t.GetStrVal())) 
					{
						tt = tstack.GetType(t.GetStrVal());
					}
					return tt != null && !(tt is Symbols.Func);
			}
		}

		private Syntax.Expression ParseConstExpr()
		{
			this.parse_const_expr = true;
			Syntax.Expression node = ParseUnaryExpr();
			if (node != null)
			{
				node = ParseBinaryOper(0, node, true);
			}
			this.parse_const_expr = false;

			return node;
		}

#endregion

#region parse statment

		private Syntax.Statement ParseStmExpression()
		{
			Syntax.Expression expr;
			try
			{
				expr = ParseExpression();
			}
			catch (Exception e)
			{
				if (scan.Peek().type == Token.Type.SEMICOLON) { scan.Read(); }
				throw e;
			}
			CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
			return new Syntax.StatementExpression(expr);
		}

		private Syntax.Statement ParseStmt()
		{
			Syntax.Statement node = null;
			const string NOT_CYCLE = "оператор можно использовать только внутри цикла";

			switch (scan.Peek().type)
			{
				case Token.Type.KW_WHILE:
					this.parse_cycle = true;
					try
					{
						scan.Read();
						CheckToken(scan.Peek(), Token.Type.LPAREN, true);

						Syntax.Expression cond = ParseExpression();
						node = new Syntax.WHILE(cond);

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						((Syntax.WHILE)node).SetCycleStatement(ParseStmt());
					}
					catch (Exception e)
					{
						this.parse_cycle = false;
						throw e;
					}
					break;

				case Token.Type.KW_DO:
					this.parse_cycle = true;
					try
					{
						scan.Read();

						node = new Syntax.DO(ParseStmt());
						CheckToken(scan.Peek(), Token.Type.KW_WHILE, true);
						CheckToken(scan.Peek(), Token.Type.LPAREN, true);

						((Syntax.DO)node).SetCondition(ParseExpression());
						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					}
					catch (Exception e)
					{
						this.parse_cycle = false;
						throw e;
					}
					break;

				case Token.Type.KW_FOR:
					this.parse_cycle = true;
					try
					{
						scan.Read();
						CheckToken(scan.Peek(), Token.Type.LPAREN, true);
						node = new Syntax.FOR();
						((Syntax.FOR)node).SetCounter(
							((Syntax.StatementExpression)ParseStmExpression()).GetExpression()
						);
						((Syntax.FOR)node).SetCondition(
							((Syntax.StatementExpression)ParseStmExpression()).GetExpression()
						);

						if (scan.Peek().type != Token.Type.RPAREN)
						{
							((Syntax.FOR)node).SetIncriment(ParseExpression());
						}

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						((Syntax.FOR)node).SetCycleStatement(ParseStmt());
					}
					catch (Exception e)
					{
						this.parse_cycle = false;
						throw e;
					}
					break;

				case Token.Type.KW_IF:
					scan.Read();
					CheckToken(scan.Peek(), Token.Type.LPAREN, true);
					node = new Syntax.IF(ParseExpression());
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					((Syntax.IF)node).SetBranchTrue(ParseStmt());
					if (scan.Peek().type == Token.Type.KW_ELSE)
					{
						scan.Read();
						((Syntax.IF)node).SetBranchFalse(ParseStmt());
					}
					break;

				case Token.Type.LBRACE:
					node = ParseCompound();
					break;

				case Token.Type.KW_RETURN:
					scan.Read();

					if (scan.Peek().type != Token.Type.SEMICOLON)
					{
						node = new Syntax.RETURN(this.ret_func_type);
						((Syntax.RETURN)node).SetValue(ParseExpression());
					}
					else
					{
						node = new Syntax.RETURN(this.ret_func_type);
						((Syntax.RETURN)node).SetValue(new Syntax.EmptyExpression());
					}

					CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					break;

				case Token.Type.KW_BREAK:
					if (!this.parse_cycle)
					{
						this.logger.Add(new Syntax.Exception(NOT_CYCLE, scan.GetPos(), scan.GetLine()));
					}

					try
					{
						scan.Read();
						node = new Syntax.BREAK();
						CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					}
					catch (Exception e)
					{
						if (this.parse_cycle)
						{
							throw e;
						}
					}
					break;

				case Token.Type.KW_CONTINUE:
					if (!this.parse_cycle)
					{
						this.logger.Add(new Syntax.Exception(NOT_CYCLE, scan.GetPos(), scan.GetLine()));
					}

					try
					{
						scan.Read();
						node = new Syntax.CONTINUE();
						CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
					}
					catch (Exception e)
					{
						if (this.parse_cycle)
						{
							throw e;
						}
					}
					break;

				default:
					node = ParseStmExpression();
					break;
			}

			return node;
		}

		private List<Syntax.Statement> ParseBlockItemList() 
		{
			List<Syntax.Statement> stmts = new List<Syntax.Statement>();
			bool begin_block = true;

			while (scan.Peek().type != Token.Type.RBRACE && scan.Peek().type != Token.Type.EOF)
			{
				try
				{
					if (IsType(scan.Peek()) && begin_block)
					{
						try
						{
							ParseDeclaration();
							
						}
						catch (Exception e)
						{
							ToHandlerException(e);
						}
						continue;
					}

					begin_block = false;
				
					stmts.Add(ParseStmt());
				}
				catch (Compiler.Exception e)
				{
					ToHandlerException(e);
				}
			}
			CheckToken(scan.Peek(), Token.Type.RBRACE, true);
			return stmts;
		}
		
		private Syntax.BLOCK ParseCompound(bool create_table = true)
		{
			if (scan.Peek().type != Token.Type.LBRACE)
			{
				return null;
			}
			scan.Read();

			if (create_table)
			{
				tstack.NewTable();
			}

			Syntax.BLOCK res = null;

			try
			{
				this.parse_block = true;
				res = new Syntax.BLOCK(ParseBlockItemList());
				this.parse_block = false;
			}
			catch (Exception e)
			{
				throw e;
			}

			if (create_table)
			{
				tstack.StepUp();
			}
			return res;
		}
		
#endregion
		
#region parse declaration

		private void ParseTypedefs()
		{
			while (scan.Peek().type == Token.Type.KW_TYPEDEF)
			{
				Symbols.TYPEDEF typedef = new Symbols.TYPEDEF(scan.Read());
				Symbols.Type type = ParseTypeSpecifier();
				Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> pair = ParseDeclarator();
				if (pair.last != null)
				{
					pair.last.last.SetType(type);
					type = pair.last.first;
				}
				typedef.SetName(pair.first.GetName());
				typedef.SetType(type);
				tstack.AddSymbol(typedef);
				CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
			}
		}

		private void ParseDeclaration()
		{
			while (scan.Peek().type == Token.Type.SEMICOLON)
			{
				scan.Read();
			}
			if (scan.Peek().type == Token.Type.EOF)
			{
				return;
			}

			Token first_token = scan.Peek();
			Symbols.Type type = ParseTypeSpecifier(), variable_type = null;

			Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> pair = null;
			bool function = true;

			while (true)
			{
				variable_type = type;

				if ((pair = ParseDeclarator()) != null)
				{
					if (pair.last != null)
					{
						pair.last.last.SetType(type);
						variable_type = pair.last.first;
					}

					///Когда мы устанавливаем финальный тип, то нужно пересчитать размер всех типов


					if (pair.first == null && !(variable_type is Symbols.RECORD))
					{
						throw new Syntax.Exception("требуется идентификатор", scan.GetPos(), scan.GetLine());
					}

					if (pair.first != null)
					{
						pair.first.SetType(variable_type);
					}

					if (scan.Peek().type == Token.Type.OP_ASSIGN) // init declarator
					{
						Symbols.Var v = pair.first;
						Syntax.Expression node = new Syntax.Identifier(
							new Token(v.GetIndex(), v.GetLine(), Token.Type.IDENTIFICATOR, v.GetName()),
							v
						);
						node = ParseBinaryOper(0, node, true);
						
						//pair.first.SetInitializer(ParseInitializer());
						pair.first.SetInitializer(node);
						function = false;
					}

					if (function && scan.Peek().type == Token.Type.LBRACE && tstack.IsGlobal())
					{
						pair.last.first.SetName(pair.first.GetName());
						tstack.AddSymbol(pair.first);
						tstack.NewTable();

						if (((Symbols.Func)pair.last.first).GetArguments()
							.Count(x => x.GetName() == pair.first.GetName()) == 0)
						{
							tstack.AddSymbol(pair.first);
						}

						foreach (Symbols.Var arg in ((Symbols.Func)pair.last.first).GetArguments()) 
						{
							tstack.AddSymbol(arg);
						}
						this.ret_func_type = ((Symbols.Func)pair.last.first).GetRefType();
						((Symbols.Func)pair.last.first).SetBody(ParseCompound(false));
						tstack.GetCurrentTable().RemoveSymbol(pair.first);
						((Symbols.Func)pair.last.first).SetTable(tstack.PopTable());
					}
					else
					{
						function = false;
					}


					if (!function && variable_type is Symbols.VOID)
					{
						throw new Symbols.Exception("недопустимо использование типа \"void\"",
							first_token.GetIndex(), first_token.GetLine());
					}
					

					if (pair.first != null && !function)
					{
						try
						{
							tstack.AddSymbol(pair.first);
						}
						catch (Symbols.Exception e)
						{
							this.logger.Add(e);
						}
					}
				}

				if (scan.Peek().type != Token.Type.COMMA || function)
				{
					break;
				}

				scan.Read();
				function = false;
			}

			if (!function)
			{
				CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
			}
		}

		private Symbols.Type ParseTypeSpecifier()
		{
			ParseTypedefs();
			switch (scan.Peek().type)
			{
				case Token.Type.KW_VOID:
				case Token.Type.KW_INT:
				case Token.Type.KW_DOUBLE:
				case Token.Type.KW_CHAR:
					return tstack.GetType(scan.Read());

				case Token.Type.KW_STRUCT:
					scan.Read();
					Token identifier = null;
					if (scan.Peek().type == Token.Type.IDENTIFICATOR)
					{
						identifier = scan.Read();
					}
					
					if (scan.Peek().type == Token.Type.LBRACE || identifier == null)
					{
						CheckToken(scan.Peek(), Token.Type.LBRACE, true);
						Symbols.RECORD res = null;
						if (identifier == null)
						{
							res = new Symbols.RECORD();
						}
						else
						{
							res = new Symbols.RECORD(identifier);
						}

						tstack.NewTable();

						try
						{
							ParseStructDeclarationList();
						}
						catch (Exception e)
						{
							res.SetFields(tstack.PopTable());
							throw e;
						}
						res.SetFields(tstack.PopTable());
						if (identifier != null)
						{
							tstack.AddTag(res);
						}
						CheckToken(scan.Peek(), Token.Type.RBRACE, true);
						return res;
					}
					else
					{
						return (Symbols.Type)tstack.GetTag(identifier);
					}
				default:
					if (scan.Peek().type != Token.Type.IDENTIFICATOR)
					{
						throw new Syntax.Exception("требуется объявление", scan.GetPos(), scan.GetLine());
					}
					return tstack.GetType(scan.Read());
			}
		}

		private void ParseStructDeclarationList()
		{
			Symbols.Type type = null;
			Symbols.Var variable = null;
			Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> pair = null;
			while (IsType(scan.Peek()) && (type = ParseTypeSpecifier()) != null)
			{
				while(true)
				{
					pair = ParseDeclarator();
					variable = pair.first;
					if (pair.last == null)
					{
						variable.SetType(type);
					}
					else
					{
						pair.last.last.SetType(type);
						variable.SetType(pair.last.first);
					}
					tstack.AddSymbol(variable);
					if (scan.Peek().type != Token.Type.COMMA) 
					{
						break;
					}
					scan.Read();
				}

				CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
			}
		}

		private Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> ParseDeclarator(
			bool is_abstract = false, bool parse_parameter = false)
		{
			Pair<Symbols.POINTER, Symbols.POINTER> pointer = ParsePointer();
			Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> pair = 
				ParseDirectDeclarator(is_abstract, parse_parameter);

			if (pair.last == null && pointer.first != null)
			{
				pair.last = new Pair<Symbols.RefType, Symbols.RefType>(pointer.first, pointer.last);
			}
			else if(pointer.first != null)
			{
				pair.last.last.SetType(pointer.first);
				pair.last.last = pointer.last;
			}

			return pair;
		}

		private Pair<Symbols.POINTER, Symbols.POINTER> ParsePointer()
		{
			Symbols.POINTER head = null, tail = null;
			while (scan.Peek().type == Token.Type.OP_STAR)
			{
				head = new Symbols.POINTER(scan.Read(), head);
				if (tail == null) { tail = head; }
			}

			return new Pair<Symbols.POINTER, Symbols.POINTER>(head, tail);
		}

		private Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> ParseDirectDeclarator(
			bool parse_abstract = false, bool parse_parameter = false)
		{
			Symbols.Var variable = null;
			Pair<Symbols.RefType, Symbols.RefType> tpair = null;
			Symbols.RefType type = null;

			if (scan.Peek().type == Token.Type.IDENTIFICATOR && !parse_abstract)
			{
				if (parse_parameter)
				{
					variable = new Symbols.ParamVar(scan.Read());
				}
				else if (tstack.IsGlobal())
				{
					variable = new Symbols.GlobalVar(scan.Read());
				}
				else
				{
					variable = new Symbols.LocalVar(scan.Read());
				}
			}
			else if (scan.Peek().type == Token.Type.LPAREN)
			{
				scan.Read();
				Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> res = 
					ParseDeclarator(parse_abstract, parse_parameter);

				variable = res.first;
				if (res.last != null)
				{
					if (tpair == null)
					{
						tpair = new Pair<Symbols.RefType, Symbols.RefType>(res.last.first, res.last.last);
					}
					else
					{
						tpair.last.SetType(res.last.first);
						tpair.last = res.last.last;
					}
				}

				CheckToken(scan.Peek(), Token.Type.RPAREN, false);
				if (res.last == null && res.first == null)
				{
					throw new Syntax.Exception("требуется выражение", scan.GetPos(), scan.GetLine());
				}
				scan.Read();
			}


			bool ret = false;
			while (true)
			{
				if (scan.Peek().type == Token.Type.LBRACKET)
				{
					type = new Symbols.ARRAY(scan.Read());
					((Symbols.ARRAY)type).SetSize(ParseAssignmentExpression());
					CheckToken(scan.Peek(), Token.Type.RBRACKET, true);
					
				}
				else if (scan.Peek().type == Token.Type.LPAREN)
				{
					type = new Symbols.Func("", scan.Peek().pos, scan.Peek().line);
					scan.Read();
					tstack.NewTable();
					((Symbols.Func)type).SetArguments(ParseParameterList());
					((Symbols.Func)type).SetTable(tstack.PopTable());
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
				}
				else
				{
					ret = true;
				}

				if (type != null )
				{
					if (tpair != null)
					{
						tpair.last.SetType(type);
						tpair.last = type;
					}
					else
					{
						tpair = new Pair<Symbols.RefType, Symbols.RefType>(type, type);
					}
				}
				type = null;

				if (ret)
				{
					return new Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>>(variable, tpair);
				}
			}
		}

		private List<Symbols.ParamVar> ParseParameterList()
		{
			List<Symbols.ParamVar> res = new List<Symbols.ParamVar>();
			Symbols.ParamVar param = null;
			this.parse_args_declaraton = true;
			while (scan.Peek().type != Token.Type.RPAREN)
			{
				try
				{
					param = ParseParameterDeclaration();
				}
				catch (Exception e)
				{
					ToHandlerException(e);
					continue;
				}

				if (param == null)
				{
					break;
				}

				res.Add(param);
				if (scan.Peek().type != Token.Type.COMMA)
				{
					break;
				}
				scan.Read();
			}
			this.parse_args_declaraton = false;
			return res;
		}

		private Symbols.ParamVar ParseParameterDeclaration()
		{
			Token first_token = scan.Peek();
			Symbols.Type type = ParseTypeSpecifier();
			Symbols.ParamVar variable = null;
			
			if (type != null)
			{
				Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> res;
				res = ParseDeclarator(parse_parameter:true);
				variable = (Symbols.ParamVar)res.first;

				if (res.last != null)
				{
					res.last.last.SetType(type);
					type = res.last.first;
				}

				if (type is Symbols.VOID)
				{
					this.logger.Add(new Symbols.Exception("недопустимо использование типа \"void\"",
								first_token.GetIndex(), first_token.GetLine()));
					type = new Symbols.SuperType();
				}

				if (variable == null)
				{
					variable = new Symbols.ParamVar("", type.GetIndex(), type.GetLine());
				}
			}
			variable.SetType(type);
			return variable;
		}

		private Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> ParseDirectAbstractDeclarator()
		{
			return ParseDirectDeclarator(true);
		}

		private List<Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>>> ParseInitDeclaratorList()
		{
			List<Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>>> res = 
				new List<Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>>>();
			Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> pair = null;
			while (true)
			{
				if ((pair = ParseInitDecalarator()) != null)
				{
					res.Add(pair);
				}

				if (scan.Peek().type != Token.Type.COMMA)
				{
					break;
				}
				scan.Read();
			}

			return res;
		}

		private Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> ParseInitDecalarator()
		{
			Pair<Symbols.Var, Pair<Symbols.RefType, Symbols.RefType>> declarator = ParseDeclarator();
			if (scan.Peek().type == Token.Type.OP_ASSIGN)
			{
				scan.Read();
				declarator.first.SetInitializer(ParseInitializer());
			}
			return declarator;
		}

		private Syntax.Expression ParseInitializer()
		{
			Syntax.Expression res = null;
			if (scan.Peek().type == Token.Type.LBRACE)
			{
				scan.Read();
				res = new Syntax.Initializer();
				Syntax.InitializerList init_list = null;
				while (true)
				{
					if ((init_list = ParseInitializerList()) != null)
					{
						((Syntax.Initializer)res).AddInitializer(init_list);
					}

					if (scan.Peek().type != Token.Type.COMMA)
					{
						break;
					}
					scan.Read();
				}
				
				CheckToken(scan.Peek(), Token.Type.RBRACE, true);
			}
			else
			{
				res = ParseAssignmentExpression();
			}

			return res;
		}

		private Syntax.InitializerList ParseInitializerList()
		{
			Syntax.InitializerList res = null;
			while (true)
			{
				Syntax.InitializerList.Designation designation = ParseDesignation();
				Syntax.Expression const_val = null;
				if (designation != null)
				{
					CheckToken(scan.Peek(), Token.Type.OP_ASSIGN, true);
					const_val = ParseInitializer();
					Syntax.Object.CheckObject(const_val);
				}
				else
				{
					const_val = ParseConstExpr();
				}
				 
				if (const_val != null)
				{
					if (res == null) 
					{
						res = new Syntax.InitializerList(); 
					}
					res.AddInitializer(const_val, designation);
				}

				if (scan.Peek().type != Token.Type.COMMA || const_val == null)
				{
					break;
				}
				scan.Read();
			}

			return res;
		}

		private Syntax.InitializerList.Designation ParseDesignation()
		{
			Syntax.InitializerList.Designation designation = null, subdisignation = null;
			while (true)
			{
				if (scan.Peek().type == Token.Type.COMMA)
				{
					CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
					subdisignation = new Syntax.InitializerList.Designation();
					subdisignation.SetDesignator(new Syntax.Identifier(scan.Read()));
				}
				else if (scan.Peek().type == Token.Type.LBRACKET)
				{
					scan.Read();
					Syntax.Expression const_expr = ParseConstExpr();
					Syntax.Object.CheckObject(const_expr);
					subdisignation = new Syntax.InitializerList.Designation();
					subdisignation.SetDesignator(const_expr);
					CheckToken(scan.Peek(), Token.Type.RBRACKET, true);
				}

				if (subdisignation == null)
				{
					break;
				}
				if (designation == null)
				{
					designation = subdisignation;
				}
				else
				{
					designation.SetDesignator(subdisignation);
				}
				subdisignation = null;
			}

			return designation;
		}
#endregion
	}
}