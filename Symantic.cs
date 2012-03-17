namespace Compiler
{
	class Symantic
	{
		private static SymType PerformType(SymType type)
		{
			if (type is SymTypeArray)
			{
				SymType t = ((SymTypeArray)type).type;
				type = new SymTypePointer(t);
			}

			return type;
		}

		public static bool CheckBinaryOper(SymType ltype, SymType rtype, Token.Type toper)
		{
			if(ltype == null || rtype == null)
			{
				return false;
			}

			if (ltype is SymSuperType || rtype is SymSuperType)
			{
				return true;
			}

			ltype = PerformType(ltype);
			rtype = PerformType(rtype);

			switch (toper)
			{
				case Token.Type.OP_PLUS:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypePointer || rtype is SymTypeDouble)
						{
							return true;
						}
					}

					if (ltype is SymTypeDouble)
					{
						if (rtype is SymTypeDouble || rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum)
						{
							return true;
						}
					}

					if (ltype is SymTypePointer)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum)
						{
							return true;
						}
					}
					break;

				case Token.Type.OP_SUB:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeDouble || ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypeDouble)
						{
							return true;
						}
					}

					if (ltype is SymTypePointer)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || (rtype is SymTypePointer 
							&& ((SymTypePointer)ltype).type.Equals(((SymTypePointer)rtype).type)))
						{
							return true;
						}
					}
					break;
				case Token.Type.OP_STAR:
				case Token.Type.OP_DIV:
				case Token.Type.OP_MOD:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeEnum || ltype is SymTypeDouble)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeDouble || rtype is SymTypeEnum)
						{
							return true;
						}
					}
					break;
				case Token.Type.OP_L_SHIFT:
				case Token.Type.OP_R_SHIFT:
				case Token.Type.OP_BIT_OR:
				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_XOR:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum)
						{
							return true;
						}
					}
					break;

				case Token.Type.OP_LESS:
				case Token.Type.OP_MORE:
				case Token.Type.OP_LESS_OR_EQUAL:
				case Token.Type.OP_MORE_OR_EQUAL:
				case Token.Type.OP_EQUAL:
				case Token.Type.OP_NOT_EQUAL:
					if (ltype is SymType || ltype is SymTypeChar || ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypePointer || rtype is SymTypeDouble || rtype is SymTypeFunc)
						{
							return true;
						}
					}

					if (ltype is SymTypeDouble)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypeDouble)
						{
							return true;
						}
					}

					if (ltype is SymTypePointer)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypePointer || rtype is SymTypeFunc)
						{
							return true;
						}
					}
					break;

				case Token.Type.OP_OR:
				case Token.Type.OP_AND:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeDouble || ltype is SymTypePointer || ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeDouble || rtype is SymTypePointer || rtype is SymTypeEnum || rtype is SymTypeFunc)
						{
							return true;
						}
					}
					break;

				case Token.Type.OP_ASSIGN:
					if (ltype is SymTypeInt || ltype is SymTypeChar || ltype is SymTypeDouble)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeDouble || rtype is SymTypeEnum || rtype is SymTypeFunc || rtype is SymTypePointer)
						{
							return true;
						}
					}

					if (ltype is SymTypePointer)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypePointer || rtype is SymTypeFunc)
						{
							return true;
						}
					}

					if (ltype is SymTypeEnum)
					{
						if (rtype is SymTypeInt || rtype is SymTypeChar || rtype is SymTypeEnum || rtype is SymTypeDouble)
						{
							return true;
						}
					}
					break;

				case Token.Type.OP_PLUS_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_PLUS) && 
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_PLUS), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_SUB_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_SUB) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_SUB), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_MUL_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_STAR) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_STAR), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_DIV_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_DIV) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_DIV), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_R_SHIFT_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_R_SHIFT) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_R_SHIFT), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_L_SHIFT_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_L_SHIFT) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_L_SHIFT), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_BIT_AND_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_BIT_AND) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_BIT_AND), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_BIT_OR_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_BIT_OR) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_BIT_OR), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;

				case Token.Type.OP_XOR_ASSIGN:
					if (Symantic.CheckBinaryOper(ltype, rtype, Token.Type.OP_XOR) &&
						Symantic.CheckBinaryOper(ltype, Symantic.GetTypeBinaryOper(ltype, rtype, Token.Type.OP_XOR), Token.Type.OP_ASSIGN))
					{
						return true;
					}
					break;
			}
			return false;
		}

		public static bool CheckUnaryOper(SymType type, Token.Type toper)
		{
			if (type is SymSuperType)
			{
				return true;
			}

			type = PerformType(type);

			switch (toper)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
					if (type is SymTypeInt || type is SymTypeChar || type is SymTypeDouble || type is SymTypePointer)
					{
						return true;
					}
					break;

				case Token.Type.OP_NOT:
					if (type is SymTypeInt || type is SymTypeChar || type is SymTypeDouble || type is SymTypePointer || type is SymTypeEnum || type is SymTypeFunc)
					{
						return true;
					}
					break;

				case Token.Type.OP_BIT_AND:
					return true;

				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
					if (type is SymTypeInt || type is SymTypeChar || type is SymTypeDouble || type is SymTypeEnum)
					{
						return true;
					}
					break;

				case Token.Type.OP_STAR:
					if (type is SymTypePointer || type is SymTypeFunc)
					{
						return true;
					}
					break;

				case Token.Type.OP_TILDE:
					if (type is SymTypeInt || type is SymTypeChar || type is SymTypeEnum)
					{
						return true;
					}
					break;

				case Token.Type.OP_DOT:
					if (type is SymTypeStruct)
					{
						return true;
					}
					break;

				case Token.Type.OP_REF:
					if (type is SymTypePointer && ((SymTypePointer)type).type is SymTypeStruct)
					{
						return true;
					}
					break;

			}

			return false;
		}

		public static SymType GetTypeBinaryOper(SymType ltype, SymType rtype, Token.Type toper)
		{
			if (ltype is SymSuperType || rtype is SymSuperType)
			{
				return new SymSuperType();
			}

			ltype = PerformType(ltype);
			rtype = PerformType(rtype);

			switch (toper)
			{
				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
					if (ltype is SymTypeDouble || rtype is SymTypeDouble)
					{
						return new SymTypeDouble();
					}

					if (ltype is SymTypePointer)
					{
						return ltype;
					}
					return new SymTypeInt();

				case Token.Type.OP_STAR:
				case Token.Type.OP_DIV:
				case Token.Type.OP_MOD:
					if (ltype is SymTypeDouble || rtype is SymTypeDouble)
					{
						return new SymTypeDouble();
					}
					return ltype;

				case Token.Type.OP_L_SHIFT:
				case Token.Type.OP_R_SHIFT:
				case Token.Type.OP_ASSIGN:
				case Token.Type.OP_PLUS_ASSIGN:
				case Token.Type.OP_SUB_ASSIGN:
				case Token.Type.OP_MUL_ASSIGN:
				case Token.Type.OP_DIV_ASSIGN:
				case Token.Type.OP_R_SHIFT_ASSIGN:
				case Token.Type.OP_L_SHIFT_ASSIGN:
				case Token.Type.OP_BIT_AND_ASSIGN:
				case Token.Type.OP_BIT_OR_ASSIGN:
				case Token.Type.OP_XOR_ASSIGN:
					return ltype;

				case Token.Type.OP_BIT_OR:
				case Token.Type.OP_BIT_AND:
				case Token.Type.OP_XOR:
				case Token.Type.OP_LESS:
				case Token.Type.OP_MORE:
				case Token.Type.OP_LESS_OR_EQUAL:
				case Token.Type.OP_MORE_OR_EQUAL:
				case Token.Type.OP_EQUAL:
				case Token.Type.OP_NOT_EQUAL:
				case Token.Type.OP_OR:
				case Token.Type.OP_AND:
					return new SymTypeInt();
			}
			return null;
		}

		public static SymType GetTypeUnaryOper(SymType type, Token.Type toper)
		{
			if (type is SymSuperType)
			{
				return new SymSuperType();
			}

			type = PerformType(type);

			switch (toper)
			{
				case Token.Type.OP_INC:
				case Token.Type.OP_DEC:
					return type;

				case Token.Type.OP_NOT:
					return new SymTypeInt();

				case Token.Type.OP_BIT_AND:
					return new SymTypePointer(type);

				case Token.Type.OP_PLUS:
				case Token.Type.OP_SUB:
					return type;

				case Token.Type.OP_STAR:
					return ((SymTypePointer)type).type;

				case Token.Type.OP_TILDE:
					return type;
			}

			return null;
		}
	}
}
