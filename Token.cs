using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	class Token
	{
		public enum Type
		{
			LPAREN, RPAREN, LBRACKET, RBRACKET, LBRACE, RBRACE, COMMA, OP_TILDE, QUESTION, SEMICOLON,
			OP_DIV, OP_DIV_ASSIGN, OP_MOD, OP_MOD_ASSIGN, OP_STAR, OP_MUL_ASSIGN, OP_NOT, OP_NOT_EQUAL, OP_EQUAL, OP_ASSIGN,
			OP_XOR, OP_XOR_ASSIGN, OP_PLUS, OP_PLUS_ASSIGN, OP_INC, OP_SUB, OP_SUB_ASSIGN, OP_DEC, OP_REF, OP_BIT_OR,
			OP_BIT_OR_ASSIGN, OP_OR, OP_BIT_AND, OP_BIT_AND_ASSIGN, OP_AND, OP_LESS, OP_LESS_OR_EQUAL, OP_L_SHIFT,
			OP_L_SHIFT_ASSIGN, OP_MORE, OP_MORE_OR_EQUAL, OP_R_SHIFT, OP_R_SHIFT_ASSIGN, OP_DOT, OP_ELLIPSIS, COLON,
			KW_BREAK, KW_WORD, KW_CASE, KW_CHAR, KW_CONST, KW_CONTINUE, KW_DEFAULT, KW_DO, KW_DOUBLE, KW_ELSE, KW_ENUM, KW_EXTERN,
			KW_FOR, KW_IF, KW_INLINE, KW_INT, KW_REGISTER, KW_RESTRICT, KW_RETURN, KW_SIZEOF, KW_STRUCT, KW_SWITCH,
			KW_TYPEDEF, KW_UNION, KW_VOID, KW_WHILE, KW_STATIC,

			IDENTIFICATOR, OPERATOR, SEPARATOR, EOF, CONST_INT, CONST_DOUBLE, CONST_CHAR, CONST_STRING, VOID, NONE, KEYWORLD
		};

		public static readonly Hashtable Terms, Types;
		public int pos = -1, line = -1;
		public Type type = Type.NONE;
		protected string strval = "";

		public Token() { }

		public Token(Type _type, string _strval = null)
		{
			if (_strval != null)
			{
				strval = _strval;
			} else if (Types.ContainsKey(_type))
			{
				strval = (string)Types[_type];
			}

			type = _type;
		}

		public Token(int pos, int line, Type type, string val)
		{
			this.line = line;
			this.pos = pos;
			this.type = type != Type.OPERATOR && type != Type.SEPARATOR ? type : (Type)Token.Terms[val];
			this.strval = val.Replace("\n", "\\n");
		}

		static Token()
		{
			Terms = new Hashtable();
			Types = new Hashtable();

			Terms["("] = Type.LPAREN;
			Terms[")"] = Type.RPAREN;
			Terms["["] = Type.LBRACKET;
			Terms["]"] = Type.RBRACKET;
			Terms["{"] = Type.LBRACE;
			Terms["}"] = Type.RBRACE;
			Terms[","] = Type.COMMA;
			Terms["~"] = Type.OP_TILDE;
			Terms["?"] = Type.QUESTION;
			Terms[";"] = Type.SEMICOLON;
			Terms["/"] = Type.OP_DIV;
			Terms["/="] = Type.OP_DIV_ASSIGN;
			Terms["%"] = Type.OP_MOD;
			Terms["%="] = Type.OP_MOD_ASSIGN;
			Terms["*"] = Type.OP_STAR;
			Terms["*="] = Type.OP_MUL_ASSIGN;
			Terms["!"] = Type.OP_NOT;
			Terms["!="] = Type.OP_NOT_EQUAL;
			Terms["="] = Type.OP_ASSIGN;
			Terms["=="] = Type.OP_EQUAL;
			Terms["^"] = Type.OP_XOR;
			Terms["^="] = Type.OP_XOR_ASSIGN;
			Terms["+"] = Type.OP_PLUS;
			Terms["+="] = Type.OP_PLUS_ASSIGN;
			Terms["++"] = Type.OP_INC;
			Terms["-"] = Type.OP_SUB;
			Terms["-="] = Type.OP_SUB_ASSIGN;
			Terms["--"] = Type.OP_DEC;
			Terms["->"] = Type.OP_REF;
			Terms["|"] = Type.OP_BIT_OR;
			Terms["|="] = Type.OP_BIT_OR_ASSIGN;
			Terms["||"] = Type.OP_OR;
			Terms["&"] = Type.OP_BIT_AND;
			Terms["&="] = Type.OP_BIT_AND_ASSIGN;
			Terms["&&"] = Type.OP_AND;
			Terms["<"] = Type.OP_LESS;
			Terms["<="] = Type.OP_LESS_OR_EQUAL;
			Terms["<<"] = Type.OP_L_SHIFT;
			Terms["<<="] = Type.OP_L_SHIFT_ASSIGN;
			Terms[">"] = Type.OP_MORE;
			Terms[">="] = Type.OP_MORE_OR_EQUAL;
			Terms[">>"] = Type.OP_R_SHIFT;
			Terms[">>="] = Type.OP_R_SHIFT_ASSIGN;
			Terms["."] = Type.OP_DOT;
			Terms["..."] = Type.OP_ELLIPSIS;
			Terms[":"] = Type.COLON;
			Terms["break"] = Type.KW_BREAK;
			Terms["char"] = Type.KW_CHAR;
			Terms["const"] = Type.KW_CONST;
			Terms["continue"] = Type.KW_CONTINUE;
			Terms["do"] = Type.KW_DO;
			Terms["double"] = Type.KW_DOUBLE;
			Terms["else"] = Type.KW_ELSE;
			Terms["enum"] = Type.KW_ENUM;
			Terms["extern"] = Type.KW_EXTERN;
			Terms["for"] = Type.KW_FOR;
			Terms["if"] = Type.KW_IF;
			Terms["int"] = Type.KW_INT;
			Terms["register"] = Type.KW_REGISTER;
			Terms["return"] = Type.KW_RETURN;
			Terms["sizeof"] = Type.KW_SIZEOF;
			Terms["static"] = Type.KW_STATIC;
			Terms["struct"] = Type.KW_STRUCT;
			Terms["typedef"] = Type.KW_TYPEDEF;
			Terms["void"] = Type.KW_VOID;
			Terms["while"] = Type.KW_WHILE;

			foreach (DictionaryEntry de in Terms)
			{
				Types[de.Value] = de.Key;
			}
			Types[Type.IDENTIFICATOR] = "идентификатор";
		}

		public string GetStrVal()
		{
			return strval;
		}

		public int GetIndex()
		{
			return this.pos;
		}

		public int GetLine()
		{
			return this.line;
		}

		public override string ToString()
		{
			return this.type + " " + this.line + " " + this.pos + " " + this.strval;
		}
	}
}
