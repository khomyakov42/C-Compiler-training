using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	namespace Syntax
	{
		static class Symatic
		{
			const string NOT_ARITHMETICAL = "выражение должно иметь арифметический или перечислимый тип";
			const string NO_OPERATOR = "отсутствует оператор \"{0}\" соответсвующий данным операндам";
			const string NOT_LVALUE = "требуется левостороннее значение";
			const string NOT_INTEGER = "выражение должно иметь целочисленный или перечислимый тип";
			const string NOT_LVALUE_OR_FUNC = "значение должно быть левосторонним значением или обозначением функции";
			const string NOT_POINTER = "операнд должен быть указателем";
			const string NOT_ARITHMETICAL_OR_FUNC = "выражение должно иметь арифметический или перечислимый тип";
			const string NOT_FUNCTION = "выражение должно иметь тип указателя на функцию";
			const string NOT_STRUCT = "выражение должно представлять структуру";
			const string NOT_MEMBER = "\"{0}\" не является членом \"{1}\"";
			const string NOT_COMPLETE_TYPE = "выражение должно представлять собой указатель на полный тип объекта";
			const string WRONG_NUMBER_ARGS = "неправильное количество аргументов";


			public class BinaryConvertResult{
				public Expression left, right;
				public Symbols.Type type;
				public bool lvalue = false;
			}

			public class UnaryConvertResult{
				public Expression operand;
				public Symbols.Type type;
				public bool lvalue = false;
			}

			public enum PostfixOperator { DOT, REF, INDEX }

			private static Pair<Expression, Expression> UsualArithmeticConversions(
				Expression lop, Expression rop)
			{
				Symbols.Type lt = lop.GetType(), rt = rop.GetType();

				if (lt is Symbols.DOUBLE || rt is Symbols.DOUBLE)
				{
					if (rt != lt)
					{
						if (lt is Symbols.DOUBLE) { rop = new Cast(rop, lt); }
						else { lop = new Cast(lop, rt); }
					}
				}
				else if (lt is Symbols.CHAR && rt is Symbols.INT) { lop = new Cast(lop, rt); }
				else if (rt is Symbols.CHAR && lt is Symbols.INT) { rop = new Cast(rop, lt); }

				return new Pair<Expression, Expression>(lop, rop);
			}


			public static BinaryConvertResult ConvertBinaryOperands(Expression lop, Expression rop, Token op)
			{
				Symbols.Type lt = lop.GetType(), rt = rop.GetType();
				BinaryConvertResult res = new BinaryConvertResult();
				Pair<Expression, Expression> uacon = null;
				switch (op.type)
				{
					case Token.Type.OP_STAR:
					case Token.Type.OP_DIV:
						if (!lt.IsArifmetic())
						{
							throw new Symbols.Exception(lop, NOT_ARITHMETICAL);
						}
						if (!rt.IsArifmetic())
						{
							throw new Symbols.Exception(rop, NOT_ARITHMETICAL);
						}

						if (op.type == Token.Type.OP_MOD)
						{
							if (!lt.IsArifmetic())
							{
								lop = new Cast(lop, new Symbols.INT());
							}
							if (!rt.IsArifmetic())
							{
								rop = new Cast(rop, new Symbols.INT());
							}
							res.type = rop.GetType();
							break;
						}
						uacon = UsualArithmeticConversions(lop, rop);
						lop = uacon.first;
						rop = uacon.last;
						res.type = lop.GetType();
						break;
					
					case Token.Type.OP_MOD:
						if (!lt.IsInteger())
						{
							throw new Symbols.Exception(lop, NOT_INTEGER);
						}
						if (!rt.IsInteger())
						{
							throw new Symbols.Exception(rop, NOT_INTEGER);
						}

						if (op.type == Token.Type.OP_MOD)
						{
							if (!(lt is Symbols.INT))
							{
								lop = new Cast(lop, new Symbols.INT());
							}
							if (!(rt is Symbols.INT))
							{
								rop = new Cast(rop, new Symbols.INT());
							}
							res.type = rop.GetType();
							break;
						}
						uacon = UsualArithmeticConversions(lop, rop);
						lop = uacon.first;
						rop = uacon.last;
						res.type = lop.GetType();
						break;

					case Token.Type.OP_PLUS:
					case Token.Type.OP_SUB:
						if (rt is Symbols.POINTER || lt is Symbols.POINTER)
						{
							if (rt is Symbols.POINTER && lt is Symbols.POINTER)
							{
								if (op.type == Token.Type.OP_PLUS)
								{
									throw new Symbols.Exception(lop, NOT_ARITHMETICAL);
								}
								res.type = rt;
								break;
							}


							BinaryOperator binop = new BinaryOperator(new Token(Token.Type.OP_STAR));
							int size = 0;
							if (rt is Symbols.POINTER)
							{
								lop = lt is Symbols.INT ? lop : new Cast(lop, new Symbols.INT());
								binop.SetLeftOperand(lop);
								size = ((Symbols.POINTER)rt).GetRefType().GetSizeType();
								binop.SetRightOperand(new Const(size.ToString(), new Symbols.INT()));
								lop = binop;
								res.type = rt;
							}
							else
							{
								rop = rt is Symbols.INT ? rop : new Cast(rop, new Symbols.INT());
								binop.SetLeftOperand(rop);
								size = ((Symbols.POINTER)lt).GetRefType().GetSizeType();
								binop.SetRightOperand(new Const(size.ToString(), new Symbols.INT()));
								rop = binop;
								res.type = lt;
							}
							break;
						}

						if (lt.IsArifmetic() || rt.IsArifmetic())
						{
							if (!lt.IsArifmetic())
							{
								throw new Symbols.Exception(lop, NOT_ARITHMETICAL);
							}

							if (!rt.IsArifmetic())
							{
								throw new Symbols.Exception(rop, NOT_ARITHMETICAL);
							}

							uacon = UsualArithmeticConversions(lop, rop);
							lop = uacon.first;
							rop = uacon.last;
							res.type = lop.GetType();
							break;
						}

						if (lt is Symbols.Func || 
							(lt is Symbols.POINTER && ((Symbols.POINTER)lt).GetRefType() is Symbols.Func))
						{
							throw new Symbols.Exception(lop, NOT_COMPLETE_TYPE);
						}

						if (rt is Symbols.Func || 
							(rt is Symbols.POINTER && ((Symbols.POINTER)rt).GetRefType() is Symbols.Func))
						{
							throw new Symbols.Exception(rop, NOT_COMPLETE_TYPE);
						}

						
						throw new Symbols.Exception(lop, string.Format(NO_OPERATOR, op.GetStrVal()));

					case Token.Type.OP_L_SHIFT:
					case Token.Type.OP_R_SHIFT:
					case Token.Type.OP_BIT_AND:
					case Token.Type.OP_BIT_OR:
					case Token.Type.OP_XOR:
						if (!lt.IsInteger())
						{
							throw new Symbols.Exception(lop, NOT_ARITHMETICAL);
						}

						if (!rt.IsInteger())
						{
							throw new Symbols.Exception(rop, NOT_ARITHMETICAL);
						}
						uacon = UsualArithmeticConversions(lop, rop);
						lop = uacon.first;
						rop = uacon.last;
						res.type = lop.GetType();
						break;
					case Token.Type.OP_MORE:
					case Token.Type.OP_LESS:
					case Token.Type.OP_LESS_OR_EQUAL:
					case Token.Type.OP_MORE_OR_EQUAL:
					case Token.Type.OP_EQUAL:
					case Token.Type.OP_NOT_EQUAL:
						uacon = UsualArithmeticConversions(lop, rop);
						lop = uacon.first;
						rop = uacon.last;
						res.type = lop.GetType();
						break;

					case Token.Type.OP_AND:
					case Token.Type.OP_OR:
						if (!(lt.IsArifmetic() || lt is Symbols.POINTER))
						{
							throw new Symbols.Exception(lop, string.Format(NO_OPERATOR, op.GetStrVal()));
						}
						if (!(rt.IsArifmetic() || rt is Symbols.POINTER))
						{
							throw new Symbols.Exception(rop, string.Format(NO_OPERATOR, op.GetStrVal()));
						}
						uacon = UsualArithmeticConversions(lop, rop);
						lop = uacon.first;
						rop = uacon.last;
						res.type = lop.GetType();
						break;

					case Token.Type.OP_ASSIGN:
						if (!lop.IsLvalue())
						{
							throw new Symbols.Exception(lop, NOT_LVALUE);
						}


						if (!lt.Equals(rt))
						{
							rop = new Cast(rop, lt);
						}
						res.type = lop.GetType();
						break;

					case Token.Type.COMMA:
						break;

					case Token.Type.OP_MUL_ASSIGN:
					case Token.Type.OP_DIV_ASSIGN:
					case Token.Type.OP_MOD_ASSIGN:
					case Token.Type.OP_PLUS_ASSIGN:
					case Token.Type.OP_SUB_ASSIGN:
					case Token.Type.OP_L_SHIFT_ASSIGN:
					case Token.Type.OP_R_SHIFT_ASSIGN:
					case Token.Type.OP_BIT_AND_ASSIGN:
					case Token.Type.OP_BIT_OR_ASSIGN:
					case Token.Type.OP_XOR_ASSIGN:
						Token.Type tt = Token.Type.OP_STAR;
						switch (op.type)
						{
							case Token.Type.OP_MUL_ASSIGN:
								tt = Token.Type.OP_STAR;
								break;
							case Token.Type.OP_DIV_ASSIGN:
								tt = Token.Type.OP_DIV;
								break;
							case Token.Type.OP_MOD_ASSIGN:
								tt = Token.Type.OP_MOD;
								break;
							case Token.Type.OP_PLUS_ASSIGN:
								tt = Token.Type.OP_PLUS;
								break;
							case Token.Type.OP_SUB_ASSIGN:
								tt = Token.Type.OP_SUB;
								break;
							case Token.Type.OP_L_SHIFT_ASSIGN:
								tt = Token.Type.OP_L_SHIFT;
								break;
							case Token.Type.OP_R_SHIFT_ASSIGN:
								tt = Token.Type.OP_R_SHIFT;
								break;
							case Token.Type.OP_BIT_AND_ASSIGN:
								tt = Token.Type.OP_BIT_AND;
								break;
							case Token.Type.OP_BIT_OR_ASSIGN:
								tt = Token.Type.OP_BIT_OR;
								break;
							case Token.Type.OP_XOR_ASSIGN:
								tt = Token.Type.OP_XOR;
								break;
						}
						BinaryOperator bop = new BinaryOperator(new Token(tt));
						bop.SetLeftOperand(lop);
						bop.SetRightOperand(rop);
						BinaryOperator assign = new BinaryOperator(new Token(Token.Type.OP_ASSIGN));
						assign.SetLeftOperand(lop);
						assign.SetRightOperand(bop);
						res.type = lop.GetType();
						break;

					default:
						throw new System.NotImplementedException();
				}

				res.left = lop;
				res.right = rop;
				return res;
			}

			public static UnaryConvertResult ConvertUnaryOperand(Expression operand, Token op)
			{
				UnaryConvertResult res = new UnaryConvertResult();
				Symbols.Type t = operand.GetType();
				switch (op.type)
				{
					case Token.Type.OP_INC:
					case Token.Type.OP_DEC:
						Token.Type tt = op.type == Token.Type.OP_DEC ?
							Token.Type.OP_SUB_ASSIGN : Token.Type.OP_PLUS_ASSIGN;

						BinaryOperator bop = new BinaryOperator(new Token(tt));
						bop.SetLeftOperand(operand);
						bop.SetRightOperand(new Const("1", new Symbols.INT()));
						res.type = bop.GetType();
						break;

					case Token.Type.OP_BIT_AND:
						if (!(operand.IsLvalue() || operand.GetType() is Symbols.Func))
						{
							throw new Symbols.Exception(operand, NOT_LVALUE_OR_FUNC);
						}
						res.type = new Symbols.POINTER(operand.GetType());
						break;

					case Token.Type.OP_STAR:
						res.lvalue = true;
						if (!(t is Symbols.POINTER || t is Symbols.Func))
						{
							throw new Symbols.Exception(operand, NOT_POINTER);
						}

						if (t is Symbols.Func)
						{
							res.type = t;
						}
						else
						{
							res.type = ((Symbols.RefType)t).GetRefType();
						}
						break;

					case Token.Type.OP_PLUS:
					case Token.Type.OP_SUB:
						if (!t.IsArifmetic())
						{
							throw new Symbols.Exception(operand, NOT_ARITHMETICAL);
						}
						res.type = t;
						break;

					case Token.Type.OP_TILDE:
						if (!t.IsInteger())
						{
							throw new Symbols.Exception(operand, NOT_INTEGER);
						}
						res.type = t;
						break;

					case Token.Type.OP_NOT:
						if (!(t.IsArifmetic() || t is Symbols.Func || t is Symbols.POINTER))
						{
							throw new Symbols.Exception(operand, string.Format(NO_OPERATOR, op.GetStrVal()));
						}
						res.type = new Symbols.INT();
						break;

					case Token.Type.KW_SIZEOF:
						res.type = new Symbols.INT();
						break;

					case Token.Type.OP_DOT:
					default:
						throw new System.NotImplementedException();
				}

				res.operand = operand;
				return res;
			}

			public static void CheckCallFunction(Expression func, List<Expression> args)
			{
				Symbols.Type t = func.GetType();

				if (!(t is Symbols.Func || (t is Symbols.POINTER && ((Symbols.POINTER)t).GetRefType() is Symbols.Func)))
				{
					throw new Symbols.Exception(func, NOT_FUNCTION);
				}

				Symbols.Func f = (Symbols.Func)(t is Symbols.Func ? t : ((Symbols.POINTER)t).GetRefType());
				List<Symbols.ParamVar> f_args = f.GetArguments();

				if (f_args.Count != args.Count)
				{
					throw new Symbols.Exception(func, WRONG_NUMBER_ARGS);
				}

				for (int i = 0; i < f_args.Count; ++i)
				{
					new Cast(args.ElementAt(i), f_args.ElementAt(i).GetType());
				}
			}

			public static BinaryConvertResult PostfixConvertResult(Expression lop, Expression rop,
				PostfixOperator op)
			{
				BinaryConvertResult res = new BinaryConvertResult();

				switch (op)
				{
					case PostfixOperator.REF:
					case PostfixOperator.DOT:
						Symbols.Type t = lop.GetType() is Symbols.POINTER && op == PostfixOperator.REF? 
							((Symbols.POINTER)lop.GetType()).GetRefType() : lop.GetType();

						if (!(t is Symbols.RECORD))
						{
							throw new Symbols.Exception(lop, NOT_STRUCT);
						}

						if (!((Symbols.RECORD)t).GetTable().ContainsVariable(((Identifier)rop).GetName()))
						{
							throw new Symbols.Exception(rop,
								string.Format(NOT_MEMBER, ((Identifier)rop).GetName(), t.GetName())
							);
						}
						res.lvalue = true;
						res.type = (((Symbols.RECORD)t).GetTable().GetVariable(((Identifier)rop).GetName())).GetType();
						break;
					case PostfixOperator.INDEX:
						if (!(lop.GetType() is Symbols.POINTER))
						{
							throw new Symbols.Exception(lop, NOT_POINTER);
						}
						if (!rop.GetType().IsInteger())
						{
							throw new Symbols.Exception(rop, NOT_INTEGER);
						}
						res.lvalue = true;
						res.type = ((Symbols.RefType)lop.GetType()).GetRefType();
						break;
				}

				res.left = lop;
				res.right = rop;
				return res;
			}

			public static void CheckCast(Symbols.Type type, Symbols.Type cast_type)
			{
			}
		}
	}
}
