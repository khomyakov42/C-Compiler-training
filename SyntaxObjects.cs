using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	namespace Syntax
	{
		class Exception : Compiler.Exception
		{
			public Exception(string s, int index, int line) : base("Ошибка синтаксиса", s, index, line) { }

			public Exception(Expression expr, string message)
				: base("Ошибка синтаксиса", message, expr.GetIndex(), expr.GetLine()) { }
		}


		class CanNotCalculated : Compiler.Exception
		{
			Expression expr = null;

			public CanNotCalculated(Expression expr, string message = "невозможно вычислить выражение") 
				: base("Ошибка синтаксиса", message, expr.GetIndex(), expr.GetLine())
			{
				this.expr = expr;
			}

			public Expression GetExpression()
			{
				return this.expr;
			}
		}


		abstract class Object
		{
			protected int line = -1, pos = -1;

			public int GetLine() { return line; }

			public int GetIndex() { return this.pos; }

			public static void CheckObject(Object obj, string text = "требуется выражение")
			{
				if (obj == null)
				{
					throw new Syntax.Exception(text, -1, -1);
				}
			}

			public static void CheckToken(Token token, Token.Type type, string text)
			{
				if (token.type != type)
				{
					throw new Syntax.Exception(text, token.GetIndex(), token.GetLine());
				}
			}

			abstract public List<Object> GetChildrens();

			public void Print(StreamWriter stream, int indent = 0)
			{
				Stack<Three<Object, string, bool>> stack = new Stack<Three<Object, string, bool>>();
				stack.Push(new Three<Object, string, bool>(this, new String(' ', indent), true));
				while (stack.Count != 0)
				{
					Three<Object, string, bool> el = stack.Pop();
					string s_indent = el.middle;
					bool last = el.last;
					Object obj = el.first;
					stream.Write(s_indent);
					stream.Write(last ? "└──" : "├──");
					stream.WriteLine('{' + (obj != null ? obj.ToString() : "ERROR") + '}');
					s_indent += last ? "   " : "│  ";

					List<Object> childrens = obj != null ? obj.GetChildrens() : new List<Object>();
					for (int i = childrens.Count - 1; i >= 0; --i)
					{
						Object subobj = childrens.ElementAt(i);
						stack.Push(new Three<Object, string, bool>(subobj, s_indent, i == childrens.Count - 1));
					}
				}
			}
		}


		class Root : Object
		{
			List<Object> statments = new List<Object>();

			public Root() { }

			public void AddChild(Object stmt)
			{
				statments.Add(stmt);
			}

			override public List<Object> GetChildrens()
			{
				return this.statments;
			}

			override public string ToString()
			{
				return "";
			}
		}


#region Expression

		abstract class Expression : Object
		{
			protected Symbols.Type type = new Symbols.SuperType();
			protected bool lvalue = false;

			public Expression() { }

			public Expression(Token t)
			{
				this.pos = t.GetIndex();
				this.line = t.GetLine();
			}

			virtual public int ComputeConstIntValue()
			{
				throw new CanNotCalculated(this, "требуется константное выражение");
			}

			public bool IsLvalue()
			{
				return this.lvalue;
			}

			public void SetType(Symbols.Type type)
			{
				this.type = type;
			}

			new public Symbols.Type GetType()
			{
				return this.type;
			}

			override public List<Object> GetChildrens()
			{
				return new List<Object>();
			}

			virtual public Expression Modified()
			{
				return this;
			}


			virtual public void GenerateCode(ICodeGen gen) { }
		}


		class EmptyExpression : Expression
		{
			public override string ToString()
			{
				return "empty expr";
			}
		}


		abstract class Operator : Expression {
			protected Token op = null;

			public Operator() { }

			public Operator(Token op) : base(op)
			{
				this.op = op;
			}

			public override string ToString()
			{
				return op.GetStrVal();
			}
		}


		class UnaryOperator : Operator {
			protected Expression operand = null;
			protected bool prefix_operator = true;

			public UnaryOperator(Token op, bool prefix_operator = true) : base(op)
			{
				this.prefix_operator = prefix_operator;
			}

			public void SetOperand(Expression operand) 
			{
				Object.CheckObject(operand);
				this.operand = operand;
				Symatic.UnaryConvertResult res = Symatic.ConvertUnaryOperand(this.operand, this.op);
				this.lvalue = res.lvalue;
				this.type = res.type;
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(operand);
				return res;
			}

			override public int ComputeConstIntValue()
			{
				if (this.prefix_operator)
				{
					switch (this.op.type)
					{
						case Token.Type.OP_PLUS:
							return +this.operand.ComputeConstIntValue();
						case Token.Type.OP_MUL_ASSIGN:
							return -this.operand.ComputeConstIntValue();
						case Token.Type.OP_TILDE:
							return ~this.operand.ComputeConstIntValue();
						case Token.Type.OP_NOT:
							return this.operand.ComputeConstIntValue() == 0 ? 1 : 0;
					}
				}
				return base.ComputeConstIntValue();
			}

			public override string ToString()
			{
				return this.prefix_operator ? base.ToString() + '@' : '@' + base.ToString();
			}

			public override Expression Modified()
			{
				this.operand = this.operand.Modified();
				return this;
			}
		}


		class BinaryOperator : Operator
		{
			protected Expression left_operand, right_operand;

			public BinaryOperator() { }

			public BinaryOperator(Token op) : base(op) { }

			public void SetLeftOperand(Expression expr)
			{
				Object.CheckObject(expr);
				this.left_operand = expr;
				this.CheckAndCastOperands();
			}

			public void SetRightOperand(Expression expr)
			{
				Object.CheckObject(expr);
				this.right_operand = expr;
				this.CheckAndCastOperands();
			}

			private void CheckAndCastOperands()
			{
				if (this.left_operand == null || this.right_operand == null)
				{
					return;
				}

				Symatic.BinaryConvertResult res = Symatic
					.ConvertBinaryOperands(this.left_operand, this.right_operand, this.op);
				this.type = res.type;
				this.lvalue = res.lvalue;
			}

			public override int ComputeConstIntValue()
			{
				int l = this.left_operand.ComputeConstIntValue(),
						r = this.right_operand.ComputeConstIntValue();
				switch (this.op.type)
				{
					case Token.Type.OP_STAR:
						return l * r;
					case Token.Type.OP_MOD:
						return l % r;
					case Token.Type.OP_DIV:
						return l / r;
					case Token.Type.OP_PLUS:
						return l + r;
					case Token.Type.OP_SUB:
						return l - r;
					case Token.Type.OP_XOR:
						return l ^ r;
					case Token.Type.OP_BIT_AND:
						return l & r;
					case Token.Type.OP_BIT_OR:
						return l | r;
					case Token.Type.OP_AND:
						return l != 0 && r != 0 ? 1 : 0;
					case Token.Type.OP_OR:
						return l != 0 || r != 0 ? 1 : 0;
					case Token.Type.OP_EQUAL:
						return l == r ? 1 : 0;
					case Token.Type.OP_NOT_EQUAL:
						return l != r ? 1 : 0;
					case Token.Type.OP_MORE:
						return l > r ? 1 : 0;
					case Token.Type.OP_LESS:
						return l < r ? 1 : 0;
					case Token.Type.OP_MORE_OR_EQUAL:
						return l >= r ? 1 : 0;
					case Token.Type.OP_LESS_OR_EQUAL:
						return l <= r ? 1 : 0;
					case Token.Type.OP_L_SHIFT:
						return l << r;
					case Token.Type.OP_R_SHIFT:
						return l >> r;
					default:
						return base.ComputeConstIntValue();
				}
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(left_operand);
				res.Add(right_operand);
				return res;
			}

			public override Expression Modified()
			{
				Symatic.BinaryConvertResult res =
					Symatic.ConvertBinaryOperands(this.left_operand, this.right_operand, this.op);
				this.left_operand = res.left.Modified();
				this.right_operand = res.right.Modified();
				this.op = res.op;
				return this;
			}


			public override void GenerateCode(ICodeGen gen)
			{
				this.left_operand.GenerateCode(gen);
				this.right_operand.GenerateCode(gen);
				switch (this.op.type)
				{
					case Token.Type.OP_STAR: break;
					case Token.Type.OP_ASSIGN: gen.Mov(); break;
					case Token.Type.OP_MOD: gen.Mod(); break;
					case Token.Type.OP_DIV: gen.Div(); break;
					case Token.Type.OP_PLUS: gen.Add(); break;
					case Token.Type.OP_SUB: gen.Sub(); break;
					case Token.Type.OP_XOR: gen.Xor(); break;
					case Token.Type.OP_BIT_AND: break;
					case Token.Type.OP_BIT_OR: break;
					case Token.Type.OP_AND: break;
					case Token.Type.OP_OR: break;
					case Token.Type.OP_EQUAL: break;
					case Token.Type.OP_NOT_EQUAL: break;
					case Token.Type.OP_MORE: break;
					case Token.Type.OP_LESS: break;
					case Token.Type.OP_MORE_OR_EQUAL: break;
					case Token.Type.OP_LESS_OR_EQUAL: break;
					case Token.Type.OP_L_SHIFT: break;
					case Token.Type.OP_R_SHIFT: break;
				}
			}
		}


		class Refference : Operator
		{
			protected Expression expr, refference;

			public Refference() { }

			public Refference(Token t) : base(t) { }

			public void SetExpression(Expression expr)
			{
				Object.CheckObject(expr);
				this.expr = expr;
				CheckAndCastOperands();
			}

			public void SetRefference(Expression refference)
			{
				Object.CheckObject(refference);
				this.refference = refference;
				CheckAndCastOperands();
			}

			private void CheckAndCastOperands()
			{
				if (this.expr == null || this.refference == null)
				{
					return;
				}

				Symatic.PostfixOperator t_op = Symatic.PostfixOperator.DOT;
				switch (this.op.type)
				{
					case Token.Type.OP_REF:
						t_op = Symatic.PostfixOperator.REF;
						break;
					case Token.Type.OP_DOT:
						t_op = Symatic.PostfixOperator.DOT;
						break;
					case Token.Type.LBRACKET:
						t_op = Symatic.PostfixOperator.INDEX;
						break;
					default:
						throw new NotImplementedException();
				}

				Symatic.BinaryConvertResult res = Symatic
					.PostfixConvertResult(this.expr, this.refference, t_op);
				this.type = res.type;
				this.lvalue = res.lvalue;
			}

			public override List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.expr);
				res.Add(this.refference);
				return res;
			}

			public override Expression Modified()
			{
				Expression res = null;
				switch (this.op.type)
				{
					case Token.Type.LBRACKET:
						BinaryOperator bop = new BinaryOperator(new Token(Token.Type.OP_PLUS));
						
						if (this.expr is Refference && ((Refference)this.expr).op.type == Token.Type.LBRACKET)
						{
							UnaryOperator u = new UnaryOperator(new Token(Token.Type.OP_BIT_AND));
							this.expr.SetType(((Symbols.RefType)this.expr.GetType()).GetRefType());
							u.SetOperand(this.expr);
							bop.SetRightOperand(u);
						}
						else
						{
							bop.SetRightOperand(this.expr);
						}

						bop.SetLeftOperand(this.refference);
						res = new UnaryOperator(new Token(Token.Type.OP_STAR));
						((UnaryOperator)res).SetOperand(bop);
						break;
					case Token.Type.OP_REF:
						UnaryOperator rop = new UnaryOperator(new Token(Token.Type.OP_STAR));
						rop.SetOperand(this.expr);
						res = new Refference(new Token(Token.Type.OP_DOT));
						((Refference)res).SetExpression(rop);
						((Refference)res).SetRefference(this.refference);
						break;
				}
				return res == null ? this : res.Modified();
			}
		}


		class Cast : Operator
		{
			protected Expression operand = null;
			protected Symbols.Type type_cast = null;

			public Cast(Expression operand, Symbols.Type type)
			{ 
				this.SetOperand(operand);
				this.SetTypeCast(type);
			}

			public Cast(Token op) : base(op) { }

			public void SetOperand(Expression operand)
			{
				Object.CheckObject(operand);
				this.operand = operand;
				CheckOperands();
			}

			public void SetTypeCast(Symbols.Type type)
			{
				this.type_cast = type;
				this.type = type;
				CheckOperands();
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.operand);
				return res;
			}

			private void CheckOperands()
			{
				if (this.type_cast == null || this.operand == null || this.operand.GetType().Equals(this.type_cast))
				{
					return;
				}

				const string ERROR = "не существует преобразования из {0} в {1}";

				if (this.operand.GetType() is Symbols.RECORD || this.type_cast is Symbols.RECORD ||
					this.operand.GetType() is Symbols.VOID || this.type_cast is Symbols.VOID)
				{
					throw new Symbols.Exception(this.operand,
						string.Format(ERROR, this.operand.GetType().ToString(), this.type_cast.ToString()));
				}
			}

			public override string ToString()
			{
				return "cast(" + this.type.ToString() + ")";
			}

			public override Expression Modified()
			{
				this.operand = this.operand.Modified();
				return this;
			}

			public override void GenerateCode(ICodeGen gen)
			{
				this.operand.GenerateCode(gen);
				if (this.type_cast is Symbols.INT || this.type_cast is Symbols.POINTER)
				{
					gen.ToInt(this.type_cast.GetSizeType());
				}
				else if (this.type_cast is Symbols.CHAR)
				{
					gen.ToInt(this.type_cast.GetSizeType());
				}
				else if (this.type_cast is Symbols.DOUBLE)
				{
					gen.ToFloat(this.type_cast.GetSizeType());
				}
			}
		}


		class Call : Operator
		{
			protected Expression operand = null;
			protected List<Expression> arguments = null;

			public Call() { }

			public Call(Token op) : base(op) { }

			public void SetOperand(Expression operand)
			{
				Object.CheckObject(operand);
				if (!(operand.GetType() is Symbols.Func))
				{
					throw new Exception(operand, "выражение должно представлять функцию");
				}
				this.operand = operand;
				this.type = ((Symbols.Func)operand.GetType()).GetRefType();
				this.Check();
			}

			public void SetArgumentList(List<Expression> args)
			{
				this.arguments = args;
				this.Check();
			}

			private void Check()
			{
				if (this.arguments == null || this.operand == null)
				{
					return;
				}

				Symatic.CheckCallFunction(this.operand, this.arguments);
			}

			public override List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.operand);
				if (this.arguments != null)
				{
					res.AddRange(this.arguments);
				}
				return res;
			}

			public override string ToString()
			{
				return "call";
			}

			public override Expression Modified()
			{
				List<Expression> args = new List<Expression>();
				foreach (Expression arg in this.arguments)
				{
					args.Add(arg.Modified());
				}

				this.arguments = args;
				this.operand = this.operand.Modified();
				return this;
			}

			public override void GenerateCode(ICodeGen gen)
			{
				this.arguments.Reverse();
				foreach (Expression arg in this.arguments)
				{
					arg.GenerateCode(gen);
				}
				this.arguments.Reverse();

				this.operand.GenerateCode(gen);
				gen.Call((Symbols.Func)this.operand.GetType());

				for (int i = 0; i < this.arguments.Count; ++i )
				{
					gen.Pop();
				}
			}
		}
		

		class Ternary : BinaryOperator
		{
			protected Expression condition, branch_true, branch_false;

			public Ternary(Expression cond)
			{
				this.SetCondition(cond);
			}

			public void SetCondition(Expression condition)
			{
				Object.CheckObject(condition);
				this.condition = condition;
			}

			public void SetBranchTrue(Expression branch_true)
			{
				Object.CheckObject(branch_true);
				this.branch_true = branch_true;
				this.CheckAndCast();
			}

			public void SetBranchFalse(Expression branch_false)
			{
				Object.CheckObject(branch_false);
				this.branch_false = branch_false;
				this.CheckAndCast();
			}

			private void CheckAndCast()
			{
				this.type = this.branch_true.GetType();
				if (this.branch_false == null || this.branch_true == null 
					|| this.branch_true.GetType().Equals(this.branch_false.GetType()))
				{
					return;
				}

				this.branch_false = new Cast(this.branch_false, this.branch_true.GetType());
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(condition);
				res.Add(branch_true);
				res.Add(branch_false);
				return res;
			}

			public override int ComputeConstIntValue()
			{
				return this.condition.ComputeConstIntValue() != 0 ? this.branch_true.ComputeConstIntValue() :
					this.branch_false.ComputeConstIntValue();
			}

			public override string ToString()
			{
				return "ternary";
			}

			public override Expression Modified()
			{
				this.condition = this.condition.Modified();
				this.branch_true = this.branch_true.Modified();
				this.branch_false = this.branch_false.Modified();
				return this;
			}
		}


		class Const : Expression
		{
			protected string constant = null;

			public Const(string value, Symbols.Type type)
			{
				this.SetType(type);
				this.SetConstant(value);
			}

			public Const(Token constant, Symbols.Type type) : base(constant)
			{
				this.SetType(type);
				this.SetConstant(constant);
			}

			public void SetConstant(string value)
			{
				if (this.type is Symbols.DOUBLE)
				{
					value = value.Replace(',', '.');
				}
				else if (this.type is Symbols.CHAR)
				{
					value = ((int)Convert.ToChar(value)).ToString();
				}
				this.constant = value;
			}

			public string GetValue()
			{
				return this.constant;
			}

			public void SetConstant(Token constant)
			{
				this.SetConstant(constant.GetStrVal());
				this.pos = constant.GetIndex();
				this.line = constant.GetLine();
			}

			public override int ComputeConstIntValue()
			{
				if (this.type is Symbols.INT)
				{
					return Int32.Parse(this.constant);
				}
				else if (this.type is Symbols.DOUBLE)
				{
					return (int)Double.Parse(this.constant);
				}
				else if (this.type is Symbols.CHAR)
				{
					return (int)Char.Parse(this.constant);
				}
				return base.ComputeConstIntValue();
			}

			public override string ToString()
			{
				return this.constant;
			}

			public override void GenerateCode(ICodeGen gen)
			{
				gen.Push(this);
			}
		}


		class Identifier : Expression
		{
			protected Token identifier;
			protected Symbols.Var variable = null;

			public Identifier(Token identifier)
			{
				this.SetIdentifier(identifier);
				this.lvalue = true;
			}

			public Identifier(Token identifier, Symbols.Var variable)
			{
				this.SetIdentifier(identifier);
				this.SetVariable(variable);
				this.lvalue = true;
			}

			public void SetIdentifier(Token identifier)
			{
				this.identifier = identifier;
				this.line = identifier.line;
				this.pos = identifier.pos;
			}

			public void SetVariable(Symbols.Var variable)
			{
				this.variable = variable;
				this.type = variable.GetType();
			}

			public string GetName()
			{
				return this.identifier.GetStrVal();
			}

			public Symbols.Var GetVariable() { return this.variable; }

			public override string ToString()
			{
				return this.identifier.GetStrVal();
			}

			public override void GenerateCode(ICodeGen gen)
			{
				gen.Push(this);
			}
		}


		class Initializer : Expression {
			protected List<Expression> init_list = new List<Expression>();

			public Initializer() { }

			public void AddInitializer(Expression init) 
			{
				this.init_list.Add(init);
			}

			public override List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				foreach (Object obj in this.init_list)
				{
					res.Add(obj);
				}
				return res;
			}

			public override string ToString()
			{
				return "init";
			}
		}


		class InitializerList : Expression
		{
			public class Designation : Expression
			{
				protected Expression designator = null;

				public Designation() { }

				public override List<Object> GetChildrens()
				{
					return this.designator.GetChildrens();
				}

				public override string ToString()
				{
					return this.designator.ToString();
				}

				public void SetDesignator(Expression designator) 
				{
					this.designator = designator;
				}
			}

			protected List<Pair<Designation, Expression>> init_list = new List<Pair<Designation, Expression>>();

			public InitializerList() { }

			public void AddInitializer(Expression value, Designation designator = null)
			{
				Object.CheckObject(value);
				this.init_list.Add(new Pair<Designation, Expression>(designator, value));
			}

			public override List<Object> GetChildrens()
			{
				return base.GetChildrens();
			} 

			public override string ToString()
			{
				return "init list";
			}
		}

#endregion

#region Statement

		abstract class Statement : Object
		{
			public override List<Object> GetChildrens()
			{
				return new List<Object>();
			}

			virtual public void Modified() { }

			virtual public void GenerateCode(ICodeGen gen) { }
		}


		class IF : Statement
		{
			Expression condition;
			Statement branch_true, branch_false;

			public IF(Expression condition)
			{
				Object.CheckObject(condition);
				this.condition = condition;
			}

			public void SetBranchTrue(Statement br)
			{
				Object.CheckObject(br);
				this.branch_true = br;
			}

			public void SetBranchFalse(Statement br)
			{
				Object.CheckObject(br);
				this.branch_false = br;
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(condition);
				res.Add(branch_true);
				if (branch_false != null)
				{
					res.Add(branch_false);
				}
				return res;
			}

			public override string ToString()
			{
				return "if";
			}

			public override void Modified()
			{
				this.branch_false.Modified();
				this.branch_true.Modified();
				this.condition = this.condition.Modified();
			}
		}


		class StatementExpression : Statement
		{
			protected Expression expression = new EmptyExpression();

			public StatementExpression() { }

			public StatementExpression(Expression expression)
			{
				this.SetExpression(expression);
			}

			public void SetExpression(Expression expression)
			{
				this.expression = expression == null ? new EmptyExpression() : expression;
			}

			public Expression GetExpression()
			{
				return this.expression == null? new EmptyExpression() : this.expression;
			}

			public override List<Object> GetChildrens()
			{
				return this.expression == null? new List<Object>(): this.expression.GetChildrens();
			}

			public override string ToString()
			{
				return this.expression == null ? "empty statement" : this.expression.ToString();
			}

			public override void Modified()
			{
				this.expression = this.expression.Modified();
			}

			public override void GenerateCode(ICodeGen gen)
			{
				if (!(this.expression is EmptyExpression))
				{
					this.expression.GenerateCode(gen);
					if (!(this.expression.GetType() is Symbols.VOID))
					{
						gen.Pop();
					}
				}
			}
		}


		class BLOCK : Statement
		{
			List<Statement> statement_list = new List<Statement>();

			public BLOCK() { }

			public BLOCK(List<Statement> stmts)
			{
				this.SetStatementList(stmts);
			}

			public void SetStatementList(List<Statement> statement_list)
			{
				this.statement_list = statement_list;
			}

			public override List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				this.statement_list.ForEach(el => res.Add(el));
				return res;
			}

			public override string ToString()
			{
				return this.statement_list.Count == 0 ? "empty block" : "block";
			}

			public override void Modified()
			{
				foreach (Statement st in this.statement_list)
				{
					st.Modified();
				}
			}


			public override void GenerateCode(ICodeGen gen)
			{
				foreach (Statement stmt in this.statement_list)
				{
					stmt.GenerateCode(gen);
				}
			}
		}


		class Cycle : Statement {
			protected Statement cycle_statement = null;

			public Cycle() { }

			public Cycle(Statement cycle_statement) 
			{
				this.SetCycleStatement(cycle_statement);
			}

			public void SetCycleStatement(Statement cycle_statement)
			{
				Object.CheckObject(cycle_statement);
				this.cycle_statement = cycle_statement;
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.cycle_statement);
				return res;
			}

			public override string ToString()
			{
				return "cycle";
			}

			public override void Modified()
			{
				this.cycle_statement.Modified();	
			}
		}


		class WHILE : Cycle
		{
			protected Expression condition = null;

			public WHILE() { }

			public WHILE(Expression condition)
			{
				this.SetCondition(condition);
			}

			public void SetCondition(Expression condition)
			{
				Object.CheckObject(condition);
				this.condition = condition;
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.condition);
				res.AddRange(base.GetChildrens());
				return res;
			}

			public override string ToString()
			{
				return "while";
			}

			public override void Modified()
			{
				this.condition = this.condition.Modified();
				base.Modified();
			}
		}


		class DO : WHILE
		{
			public DO() { }

			public DO(Statement cycle_statement)
			{
				this.SetCycleStatement(cycle_statement);
			}

			public override string ToString()
			{
				return "do";
			}
		}


		class FOR : WHILE {
			protected Expression counter, incriment;

			public FOR() { }

			public void SetCounter(Expression counter) {
				Object.CheckObject(counter);
				this.counter = counter;
			}

			public void SetIncriment(Expression incriment)
			{
				Object.CheckObject(incriment);
				this.incriment = incriment;
			}

			override public List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.counter);
				res.AddRange(base.GetChildrens());
				if (this.incriment != null)
				{
					res.Add(this.incriment);
				}
				return res;
			}

			public override string ToString()
			{
				return "for";
			}

			public override void Modified()
			{
				this.counter = this.counter.Modified();
				this.incriment = this.incriment.Modified();
			}
		}


		class RETURN : Statement
		{
			protected Expression value = new EmptyExpression();
			protected Symbols.Type ret_type = new Symbols.SuperType();

			public RETURN(Symbols.Type ret_type) 
			{
				this.ret_type = ret_type;
			}

			public void SetValue(Expression value)
			{
				this.value = value;
				Check();
			}

			private void Check()
			{
				if (this.ret_type is Symbols.VOID && !(this.value is EmptyExpression))
				{
					throw new Symbols.Exception("функция не должна возвращать значение", this.pos, this.line);
				}
				else if (this.value is EmptyExpression && !(this.ret_type is Symbols.VOID))
				{
					throw new Symbols.Exception("функция должна возвращать значение", this.pos, this.line);
				}
				else if (!(value is EmptyExpression))
				{
					new Cast(this.value, this.ret_type);
				}
			}

			public override List<Object> GetChildrens()
			{
				List<Object> res = new List<Object>();
				res.Add(this.value);
				return res;
			}

			public override string ToString()
			{
				return "return";
			}

			public override void Modified()
			{
				this.value.Modified();
			}
		}


		class CONTINUE : Statement 
		{
			public override string ToString()
			{
				return "continue";
			}
		}


		class BREAK : Statement 
		{
			public override string ToString()
			{
				return "break";
			}
		}

#endregion
	}
}