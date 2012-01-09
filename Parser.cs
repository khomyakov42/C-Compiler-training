using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class Pair<T1, T2>
	{
		public T1 first;
		public T2 last;

		public Pair(T1 _first, T2  _last)
		{
			first = _first;
			last = _last;
		}

		public Pair(){
			first = default(T1);
			last = default(T2);
		}
	}

	class Three<T1, T2, T3>: Pair<T1, T3>{
		public T2 middle;
		public Three(T1 _first, T2 _middle, T3 _last)
		{
			first = _first;
			middle = _middle;
			last = _last;
		}

		public Three(){
			first = default(T1);
			middle = default(T2);
			last = default(T3);
		}
	}

	class Parser
	{
		public class Exception : Compiler.Exception
		{
			public Exception(string message, int line, int pos) :
				base("Ошибка синтаксиса в строке " + line + " позиции " + pos + ": " + message + "\n") {}
		}

		public class Logger
		{
			List<System.Exception> list;

			public Logger()
			{
				this.list = new List<System.Exception>();
			}

			public void Add(System.Exception e)
			{
				if(e != null)
				{
					list.Add(e);
				}
			}

			override public string ToString()
			{
				string s = "";
				foreach (var e in this.list)
				{
					s += e.Message + '\n';
				}

				return s;
			}

			public bool isEmpty()
			{
				return list.Count == 0;
			}
		}

		Scaner scan;
		public Logger logger;
		public StackTable tables = new StackTable();
		bool parse_const_expr = false;
		int count_string = 0;

		private Token CheckToken(Token t, Token.Type type, bool get_next_token = false)
		{
			SynObj.CheckToken(t, type, "требуется \"" + Token.type_to_terms[type].ToString() + '"');

			return get_next_token ? scan.Read() : t;
		}

		public Parser(Scaner sc)
		{
			scan = sc;
			logger = new Logger();
		}

		private void PassExpr()
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
			while (t.type != Token.Type.EOF && (t.type != Token.Type.SEMICOLON && t.type != Token.Type.RBRACE || count_br != 0));
			
		}

		private void ToHandlerException(Compiler.Exception e)
		{
			if (e is SynObj.Exception)
			{
				this.logger.Add(new Parser.Exception(e.Message, scan.GetLine(), scan.GetPos()));
				PassExpr();
			}
			else if (e is Symbol.Exception)
			{
				this.logger.Add(new Parser.Exception(e.Message, ((Symbol.Exception)e).line, ((Symbol.Exception)e).pos));
				if (!e.Data.Contains("delayed"))
				{
					PassExpr();
				}
			}
			else if (e is Scaner.Exception)
			{
				scan.Pass();
				this.logger.Add(e);
				PassExpr();
			}
		}

		public void Parse()
		{
			Root tree = new Root();
			do
			{
				try
				{
					while (scan.Peek().type != Token.Type.EOF)
					{
						ParseDeclaration(false);
					}
				}
				catch (Compiler.Exception e)
				{
					ToHandlerException(e);
				}

			}
			while (scan.Peek().type != Token.Type.EOF);

		//	Console.Write(tables.ToString());
		//	Console.Write(logger.ToString());
		}

		#region parse expression

		private SynExpr ParseExpression(bool bind = true, bool parse_list = false)
		{
			SynExpr node = null;
			List<SynExpr> expr_list = new List<SynExpr>();

			while(true)
			{
				node = ParseUnaryExpr();

				if (node == null && !bind)
				{
					return new ExprList(expr_list);
				}
				
				node = ParseBinaryOper(0, node);
				if (node != null)
				{
					expr_list.Add(node);
				}

				if (scan.Peek().type != Token.Type.COMMA || !parse_list)
				{
					break;
				}

				scan.Read();
			}

			if (parse_list)
			{
				return expr_list.Count == 0 ? null : new ExprList(expr_list);
			}

			return expr_list.Count == 0 ? null : (SynExpr)expr_list[0];
		}

		private SynExpr ParseBinaryOper(int level, SynExpr lnode)
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

				if (this.parse_const_expr && op_level == 2)
				{
					throw new SynObj.Exception("недопустимый оператор в константном выражении");
				}

				Token oper = scan.Read();

				SynExpr rnode = (SynExpr)SynObj.CheckSynObj(ParseUnaryExpr());

				int level_next_oper = GetOperatorPriority(scan.Peek());

				if (op_level < level_next_oper)
				{
					rnode = ParseBinaryOper(op_level + 1, rnode);
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

				try
				{
					((BinaryOper)root).Check();
				}
				catch (Symbol.Exception e)
				{
					ToHandlerException(e);
				}

				lnode = root;
			}
		}

		private SynExpr ParsePrimaryExpr()
		{
			SynExpr res = null;
			switch (scan.Peek().type)
			{
				case Token.Type.CONST_CHAR:
					res = new ConstExpr(scan.Read(), new SymTypeChar());
					break;

				case Token.Type.CONST_DOUBLE:
					res = new ConstExpr(scan.Read(), new SymTypeDouble());
					break;

				case Token.Type.CONST_INT:
					res = new ConstExpr(scan.Read(), new SymTypeInt());
					break;

				case Token.Type.CONST_STRING:
					Token t = scan.Read();
					SymVarConst var = new SymVarConst(new Token(Token.Type.CONST_STRING, "str" + count_string));

					SymTypeArray str = new SymTypeArray(tables.GetType("char"));
					str.SetSize(new ConstExpr(tables.GetType("int"), "" + (t.strval.Length - 2 + 3)));
					var.SetType(str);

					res = new ConstString(var);
					var.SetInitValue(new SynInit(new ConstExpr(new SymTypePointer(new SymTypeChar()), t.strval)));
					tables.AddConst(var);
					break;

				case Token.Type.IDENTIFICATOR:
					SymVar v = null;
					if (!tables.ContainsIdentifier(scan.Peek().strval) && !tables.ContainsConst(scan.Peek().strval))
					{
						tables.AddVar(new SymSuperVar(scan.Peek()));
						this.logger.Add(new Symbol.Exception(scan.Peek().strval + ": необъявленный идентификатор", scan.GetLine(), scan.GetPos()));
						res = new IdentExpr(scan.Peek(), tables.GetIdentifier(scan.Read().strval));
						break;
					}

					v = tables.ContainsConst(scan.Peek().strval) ? tables.GetConst(scan.Peek().strval) : tables.GetIdentifier(scan.Peek().strval);
					Token id = scan.Read();
					res = new IdentExpr(id, v);
					break;

				case Token.Type.LPAREN:
					scan.Read();
					 res = ParseExpression();

					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					break;
			}

			if (res != null)
			{
				res.Check();
			}

			return res;
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
						if (scan.Peek().type == Token.Type.OP_DOT)
						{
							res = new DotOper(scan.Read());
						}
						else
						{
							res = new RefOper(scan.Read());
						}

						SymType t = node.getType();
						((DotOper)res).SetParent(node);
						Token id = null;
						try
						{
							CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
							id = scan.Read();
							IdentExpr ident = null;

							if (t is SymTypeStruct && ((SymTypeStruct)t).fields.ContainsIdentifier(id.strval))
							{
								ident = new IdentExpr(id, ((SymTypeStruct)t).fields.GetIdentifier(id.strval));
							}
							else
							{
								ident = new IdentExpr(id);
							}

							((DotOper)res).SetChild(ident);
							((DotOper)res).Check();
						}
						catch (Symbol.Exception e)
						{
							((DotOper)res).SetChild(new IdentExpr(id == null? scan.Peek(): id, new SymSuperVar()));
							ToHandlerException(e);
						}
						break;

					case Token.Type.LPAREN:
						res = new CallOper(node, scan.GetLine(), scan.GetPos());
						scan.Read();

						if (scan.Peek().type == Token.Type.RPAREN)
						{
							scan.Read();
							break;
						}

						ExprList args = (ExprList)ParseExpression(true, true);
						foreach(var arg in args.list)
						{
							((CallOper)res).AddArgument(arg);
						}
						try
						{
							res.Check();
						}
						catch (Symbol.Exception e)
						{
							ToHandlerException(e);
						}

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);

						break;

					case Token.Type.LBRACKET:
						res = new SqBrkOper(scan.Read());
						((SqBrkOper)res).SetParent(node);
						((SqBrkOper)res).SetChild(ParseExpression());
						try
						{
							res.Check();
						}
						catch(Symbol.Exception e)
						{
							ToHandlerException(e);
						}

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
					int l = scan.GetLine(), p = scan.GetPos();
					scan.Read();
					SynExpr res = null;

					if (IsType(scan.Peek()))
					{
						res = new CastExpr(ParseType(), l, p);
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
					if (IsType(scan.Peek()))
					{
						((SizeofOper)node).SetOperand(ParseType());
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
				default:
					return -1;
			}
		}

		private SynExpr ParseConstExpr(bool is_list = true)
		{
			this.parse_const_expr = true;
			SynExpr res = null;
			res = ParseExpression(true, is_list);
			this.parse_const_expr = false;

			return res;
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
					node = ParseCompound();
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

		private StmtBLOCK ParseCompound(SymTypeFunc func = null)
		{
			if (scan.Peek().type != Token.Type.LBRACE)
			{
				return null;
			}

			scan.Read();
			List<SynObj> stmts = new List<SynObj>();
			bool begin_block = true;
			tables.NewTable();

			if (func != null)
			{
				foreach(var arg in func.args)
				{
					if (arg.GetName() != Symbol.UNNAMED)
					{
						tables.AddVar(arg);
					}
				}
			}

			while (scan.Peek().type != Token.Type.RBRACE && scan.Peek().type != Token.Type.EOF)
			{
				try
				{
					if (IsType(scan.Peek()) && begin_block)
					{
						ParseDeclaration();
						continue;
					}

					begin_block = false;
				
					stmts.Add(ParseStmt());
				}
				catch (SynObj.Exception e)
				{
					this.logger.Add(new Parser.Exception(e.Message, scan.GetLine(), scan.GetPos()));
					PassExpr();
				}
				catch (Symbol.Exception e)
				{
					this.logger.Add(new Parser.Exception(e.Message, e.line, e.pos));
					if (!e.Data.Contains("delayed"))
					{
						PassExpr();
					}
				}
				catch (Scaner.Exception e)
				{
					scan.Pass();
					this.logger.Add(e);
					PassExpr();
				}
			}
			CheckToken(scan.Peek(), Token.Type.RBRACE, true);
			StmtBLOCK res = new StmtBLOCK(stmts, tables.GetCurrent());
			tables.Up();
			return res;
		}

		#endregion
		
		#region parse declaration

		enum TypeDeclarator { PARAM, FIELD, VAR };

		private void ParseDeclaration(bool in_block = true, SymTable _table = null)
		{
			SymType type = null;
			SymVar var = null;
			if (scan.Peek().type == Token.Type.KW_TYPEDEF)
			{
				scan.Read();
				type = ParseType(false, false);
				var = ParseDeclarator(type);
				type = new SymTypeAlias(var);
				tables.AddType(type);
				CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
				return;
			}

			type = ParseType(false);

			bool is_function = false;
			bool is_struct = in_block && _table != null;
			bool first_loop = true;

			if (scan.Peek().type != Token.Type.SEMICOLON)
			{
				while (scan.Peek().type == Token.Type.COMMA || first_loop)
				{
					if (first_loop)
					{
						first_loop = false;
					}
					else
					{
						scan.Read();
					}

					var = ParseDeclarator(type, _table == null? TypeDeclarator.VAR: TypeDeclarator.FIELD, in_block);

					if (var.type is SymTypeFunc && !((SymTypeFunc)var.type).IsEmptyBody()) // значит это функция
					{
						is_function = true;
					}

					if (scan.Peek().type == Token.Type.OP_ASSIGN && !is_function && !is_struct)
					{
						scan.Read();
						try
						{
							var.SetInitValue(ParseInit());
						}
						catch (Compiler.Exception e)
						{
							ToHandlerException(e);
						}
					}

					if (is_struct)
					{
						_table.AddVar(var);
					}
					else
					{
						tables.AddVar(var);
					}


					if (is_function)
					{
						if (scan.Peek().type == Token.Type.SEMICOLON)
						{
							scan.Read();
						}
							
						return;
					}
				}
			}

			CheckToken(scan.Peek(), Token.Type.SEMICOLON, true);
		}

		private bool IsType(Token t)
		{
			switch (t.type)
			{
				case Token.Type.KW_STRUCT:
				case Token.Type.KW_ENUM:
					return true;
				default:
					return tables.ContainsType(t.strval);
			}
			
		}

		private SymType ParseStruct()
		{
			CheckToken(scan.Peek(), Token.Type.KW_STRUCT, true);

			SymTypeStruct type = new SymTypeStruct();

			if (scan.Peek().type != Token.Type.IDENTIFICATOR && scan.Peek().type != Token.Type.LBRACE)
			{
				CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
			}

			if (scan.Peek().type == Token.Type.IDENTIFICATOR)
			{
				type.SetName(scan.Read().strval);
			}

			if (scan.Peek().type == Token.Type.LBRACE)
			{
				scan.Read();
				SymTable items = new SymTable();
				while (scan.Peek().type != Token.Type.RBRACE && scan.Peek().type != Token.Type.EOF)
				{
					try
					{
						ParseDeclaration(true, items);
					}
					catch (Compiler.Exception e)
					{
						ToHandlerException(e);
					}
				}

				CheckToken(scan.Peek(), Token.Type.RBRACE, true);
				type.SetItems(items);
			}

			return type;
		}

		private SymType ParseEnum()
		{
			CheckToken(scan.Peek(), Token.Type.KW_ENUM, true);

			SymTypeEnum type = new SymTypeEnum();

			if (scan.Peek().type != Token.Type.IDENTIFICATOR && scan.Peek().type != Token.Type.LBRACE)
			{
				CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
			}

			if (scan.Peek().type == Token.Type.IDENTIFICATOR)
			{
				type.SetName(scan.Read().strval);
			}

			if (scan.Peek().type == Token.Type.LBRACE)
			{
				scan.Read();
				if (scan.Peek().type != Token.Type.RBRACE)
				{
					SymVar var = null;
					do
					{
						CheckToken(scan.Peek(), Token.Type.IDENTIFICATOR);
						var = new SymVarConst(scan.Read());
						var.SetType(tables.GetType("int"));

						if (scan.Peek().type == Token.Type.OP_ASSIGN)
						{
							scan.Read();
							var.SetInitValue(new SynInit(ParseConstExpr(false)));
						}
						
						type.AddEnumerator(var);
						try
						{
							tables.AddConst(var);
						}
						catch (Symbol.Exception e)
						{
							ToHandlerException(e);
						}

						if (scan.Peek().type == Token.Type.COMMA)
						{
							scan.Read();
						}
						else
						{
							break;
						}
					} while (scan.Peek().type != Token.Type.EOF);
				}
				CheckToken(scan.Peek(), Token.Type.RBRACE, true);
			}

			return type;
		}

		private SymType ParseType(bool parse_storage_class = true, bool add_to_table = true)
		{
			SymType type = null;
			bool possibly_override = false;

			switch (scan.Peek().type)
			{
				case Token.Type.KW_VOID:
					type = new SymTypeVoid(scan.Read());
					break;

				case Token.Type.KW_INT:
					type = new SymTypeInt(scan.Read());
					break;

				case Token.Type.KW_DOUBLE:
					type = new SymTypeDouble(scan.Read());
					break;

				case Token.Type.KW_CHAR:
					type = new SymTypeChar(scan.Read());
					break;

				case Token.Type.KW_STRUCT:
					type = ParseStruct();
					possibly_override = true;
					break;

				case Token.Type.KW_ENUM:
					type = ParseEnum();
					possibly_override = true;
					break;

				case Token.Type.IDENTIFICATOR:
					try
					{
						type = tables.GetType(scan.Peek().strval);
					}
					catch (SymTable.Exception e)
					{
						throw new Symbol.Exception(e.Message, scan.GetPos(), scan.GetLine());
					}
					scan.Read();
					break;
			}

			if (add_to_table && possibly_override)
			{
				try
				{
					tables.AddType(type);
				}
				catch (Symbol.Exception e)
				{
					ToHandlerException(e);
				}
			}
			return type;
		}

		private SymVar ParseDeclarator(SymType type, TypeDeclarator t_decl = TypeDeclarator.VAR, bool in_block = true)
		{
			Three<SymType, SymVar, SymType> res = ParseInternalDeclarator(type, t_decl, in_block);

			if (res == null)
			{
				throw new Symbol.Exception("требуется идентификатор", scan.GetPos(), scan.GetLine());
			}

			res.middle.SetType(res.first);
			return res.middle;
		}

		private Pair<SymType, SymType> ParsePointer(Pair<SymType, SymType> l_type_part)
		{
			while (scan.Peek().type == Token.Type.OP_STAR)
			{
				l_type_part.first = new SymTypePointer(l_type_part.first);
				if (l_type_part.last == null)
				{
					l_type_part.last = l_type_part.first;
				}
				scan.Read();
			}

			if (l_type_part.last == null)
			{
				l_type_part.last = l_type_part.first;
			}

			return l_type_part;
		}

		private Three<SymType, SymVar, SymType> ParseInternalDeclarator(SymType type = null,
			TypeDeclarator t_decl = TypeDeclarator.VAR, bool in_block = true)
		{
			Pair<SymType, SymType> right_part = new Pair<SymType,SymType>(), left_part = new Pair<SymType,SymType>();
			bool possibly_function = false;
			
			if (type != null)
			{
				left_part.first = type;
			}

			left_part = ParsePointer(left_part);

			SymVar var = null;

			switch (scan.Peek().type)
			{
				case Token.Type.IDENTIFICATOR:
					if (t_decl == TypeDeclarator.PARAM)
					{
						var = new SymVarParam(scan.Read());
					}
					else
					{
						if (tables.isGlobalTable() || t_decl == TypeDeclarator.FIELD)
						{
							var = new SymVarGlobal(scan.Read());
						}
						else
						{
							var = new SymVarLocal(scan.Read());
						}
						possibly_function = true;
					}
					break;

				case Token.Type.LPAREN:
					scan.Read();
					Three<SymType, SymVar, SymType> res = ParseInternalDeclarator(null, t_decl, in_block);
					if (res == null)
					{
						throw new Symbol.Exception("требуется идентификатор", scan.GetPos(), scan.GetLine());
					}

					right_part.first = res.first;
					right_part.last = res.last;
					var = res.middle;
					CheckToken(scan.Peek(), Token.Type.RPAREN, true);
					break;

				default:
					if (t_decl != TypeDeclarator.PARAM)
					{
						return null;
					}
					var = new SymVarParam(scan.GetLine(), scan.GetPos());
					break;
			}

			if (left_part.first == left_part.last && left_part.last is SymTypeVoid 
				&& scan.Peek().type != Token.Type.LPAREN && t_decl != TypeDeclarator.PARAM)
			{
				throw new Symbol.Exception("недопустимо использование типа \"void\"",
					((SymTypeVoid)left_part.first).pos, ((SymTypeVoid)left_part.first).line);
			}


			bool next = true;

			while (next)
			{
				switch (scan.Peek().type)
				{
					case Token.Type.LPAREN:
						if (possibly_function && in_block)
						{
							CheckToken(scan.Peek(), Token.Type.SEMICOLON);
						}

						scan.Read();

						SymTypeFunc func = new SymTypeFunc();

						if (right_part.last != null)
						{
							((SymRefType)right_part.last).SetType(func);
						}
						right_part.last = func;

						if (right_part.first == null)
						{
							right_part.first = right_part.last;
						}

						if (scan.Peek().type != Token.Type.RPAREN)
						{
							try
							{
								((SymTypeFunc)right_part.last).SetParam(ParseParameterDeclaration());
							}
							catch (Symbol.Exception e)
							{
								logger.Add(e);
							}

							while (scan.Peek().type == Token.Type.COMMA)
							{
								scan.Read();
								try
								{
									((SymTypeFunc)right_part.last).SetParam(ParseParameterDeclaration());
								}
								catch (Symbol.Exception e)
								{
									logger.Add(e);
								}
							}
						}

						CheckToken(scan.Peek(), Token.Type.RPAREN, true);
						if (possibly_function)
						{
							if (scan.Peek().type == Token.Type.LBRACE)
							{
								((SymTypeFunc)right_part.last).SetType(left_part.first);
								var.SetType(right_part.first);
								tables.AddVar(var);
								((SymTypeFunc)right_part.last).SetBody(ParseCompound(func));
								((SymTypeFunc)right_part.last).SetName(var.GetName());
								next = false;
							}
						}
						break;

					case Token.Type.LBRACKET:
						scan.Read();

						SymTypeArray arr = new SymTypeArray();
						if (right_part.last != null)
						{
							((SymRefType)right_part.last).SetType(arr);
						}
						right_part.last = arr;

						if (right_part.first == null)
						{
							right_part.first = right_part.last;
						}

						if (scan.Peek().type != Token.Type.RBRACKET)
						{
							((SymTypeArray)right_part.last).SetSize(ParseConstExpr(false));
						}

						CheckToken(scan.Peek(), Token.Type.RBRACKET, true);
						break;

					default:
						next = false;
						break;
				}
				possibly_function = false;
			}

			if (right_part.last != null)
			{
				((SymRefType)right_part.last).SetType(left_part.first);
			}
			else
			{
				right_part.first = left_part.first;
			}

			return new Three<SymType, SymVar, SymType>(right_part.first, var, left_part.last);
		}

		private SymVarParam ParseParameterDeclaration()
		{
			SymType type = ParseType();
			return ((SymVarParam)ParseDeclarator(type, TypeDeclarator.PARAM));
		}

		private SynInit ParseInit()
		{
			if (scan.Peek().type == Token.Type.LBRACE)
			{
				return ParseInitList();
			}

			SynExpr val = null;
			val = ParseConstExpr(false);

			return val == null ? null : new SynInit(val);
		}

		private SynInit ParseInitList()
		{
			if (scan.Peek().type != Token.Type.LBRACE)
			{
				return null;
			}

			SynInitList res = new SynInitList(scan.GetLine(), scan.GetPos());
			scan.Read();

			res.AddInit(ParseInit());
			while (scan.Peek().type == Token.Type.COMMA)
			{
				scan.Read();
				res.AddInit(ParseInit());
			}
			CheckToken(scan.Peek(), Token.Type.RBRACE, true);

			return new SynInit(res);
		}
		
		#endregion
	}
}