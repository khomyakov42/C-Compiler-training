using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler
{

	class Token
	{
		public enum Type
		{
			LPAREN , RPAREN, LBRACKET, RBRACKET, LBRACE, RBRACE, COMMA, OP_TILDE, QUESTION, SEMICOLON,
			OP_DIV, OP_DIV_ASSIGN, OP_MOD, OP_MOD_ASSIGN, OP_STAR, OP_MUL_ASSIGN, OP_NOT, OP_NOT_EQUAL, OP_EQUAL, OP_ASSIGN,
			OP_XOR, OP_XOR_ASSIGN, OP_PLUS, OP_PLUS_ASSIGN, OP_INC, OP_SUB, OP_SUB_ASSIGN, OP_DEC, OP_REF, OP_BIT_OR,
			OP_BIT_OR_ASSIGN, OP_OR, OP_BIT_AND, OP_BIT_AND_ASSIGN, OP_AND, OP_LESS, OP_LESS_OR_EQUAL, OP_L_SHIFT,
			OP_L_SHIFT_ASSIGN, OP_MORE, OP_MORE_OR_EQUAL, OP_R_SHIFT, OP_R_SHIFT_ASSIGN, OP_DOT, OP_ELLIPSIS, COLON,
			KW_BREAK, KW_WORD, KW_CASE, KW_CHAR, KW_CONST, KW_CONTINUE, KW_DEFAULT, KW_DO, KW_DOUBLE, KW_ELSE, KW_ENUM, KW_EXTERN,
			KW_FOR, KW_IF, KW_INLINE, KW_INT, KW_REGISTER, KW_RESTRICT, KW_RETURN, KW_SIZEOF, KW_STRUCT, KW_SWITCH, 
			KW_TYPEDEF, KW_UNION, KW_VOID, KW_WHILE, KW_STATIC,

			IDENTIFICATOR, OPERATOR, SEPARATOR, EOF, CONST_INT, CONST_DOUBLE, CONST_CHAR, CONST_STRING, VOID, NONE, KEYWORLD
		};

		public static readonly Hashtable terms, type_to_terms;
		public int pos, line;				
		public Type type;						
		public string strval;				
													
		static Token()
		{
			terms = new Hashtable();
			type_to_terms = new Hashtable();

			terms["("] = Type.LPAREN;
			terms[")"] = Type.RPAREN;
			terms["["] = Type.LBRACKET;
			terms["]"] = Type.RBRACKET;
			terms["{"] = Type.LBRACE;
			terms["}"] = Type.RBRACE;
			terms[","] = Type.COMMA;
			terms["~"] = Type.OP_TILDE;
			terms["?"] = Type.QUESTION;
			terms[";"] = Type.SEMICOLON;
			terms["/"] = Type.OP_DIV;
			terms["/="] = Type.OP_DIV_ASSIGN;
			terms["%"] = Type.OP_MOD;
			terms["%="] = Type.OP_MOD_ASSIGN;
			terms["*"] = Type.OP_STAR;
			terms["*="] = Type.OP_MUL_ASSIGN;
			terms["!"] = Type.OP_NOT;
			terms["!="] = Type.OP_NOT_EQUAL;
			terms["="] = Type.OP_ASSIGN;
			terms["=="] = Type.OP_NOT_EQUAL;
			terms["^"] = Type.OP_XOR;
			terms["^="] = Type.OP_XOR_ASSIGN;
			terms["+"] = Type.OP_PLUS;
			terms["+="] = Type.OP_PLUS_ASSIGN;
			terms["++"] = Type.OP_INC;
			terms["-"] = Type.OP_SUB;
			terms["-="] = Type.OP_SUB_ASSIGN;
			terms["--"] = Type.OP_DEC;
			terms["->"] = Type.OP_REF;
			terms["|"] = Type.OP_BIT_OR;
			terms["|="] = Type.OP_BIT_OR_ASSIGN;
			terms["||"] = Type.OP_OR;
			terms["&"] = Type.OP_BIT_AND;
			terms["&="] = Type.OP_BIT_AND_ASSIGN;
			terms["&&"] = Type.OP_AND;
			terms["<"] = Type.OP_LESS;
			terms["<="] = Type.OP_LESS_OR_EQUAL;
			terms["<<"] = Type.OP_L_SHIFT;
			terms["<<="] = Type.OP_L_SHIFT_ASSIGN;
			terms[">"] = Type.OP_MORE;
			terms[">="] = Type.OP_MORE_OR_EQUAL;
			terms[">>"] = Type.OP_R_SHIFT;
			terms[">>="] = Type.OP_R_SHIFT_ASSIGN;
			terms["."] = Type.OP_DOT;
			terms["..."] = Type.OP_ELLIPSIS;
			terms[":"] = Type.COLON;
			terms["break"] = Type.KW_BREAK;
			terms["char"] = Type.KW_CHAR;
			terms["const"] = Type.KW_CONST;
			terms["continue"] = Type.KW_CONTINUE;
			terms["do"] = Type.KW_DO;
			terms["double"] = Type.KW_DOUBLE;
			terms["else"] = Type.KW_ELSE;
			terms["enum"] = Type.KW_ENUM;
			terms["extern"] = Type.KW_EXTERN;
			terms["for"] = Type.KW_FOR;
			terms["if"] = Type.KW_IF;
			terms["int"] = Type.KW_INT;
			terms["register"] = Type.KW_REGISTER;
			terms["return"] = Type.KW_RETURN;
			terms["sizeof"] = Type.KW_SIZEOF;
			terms["static"] = Type.KW_STATIC;
			terms["struct"] = Type.KW_STRUCT;
			terms["typedef"] = Type.KW_TYPEDEF;
			terms["void"] = Type.KW_VOID;
			terms["while"] = Type.KW_WHILE;

			foreach (DictionaryEntry de in terms)
			{
				type_to_terms[de.Value] = de.Key;
			}
			type_to_terms[Type.IDENTIFICATOR] = "идентификатор";
		}

		public Token(int pos, int line, Type type, string val)
		{
			this.line = line;
			this.pos = pos;
			this.type = type != Type.OPERATOR && type != Type.SEPARATOR ? type: (Type)Token.terms[val];
			this.strval = val;
		}

		public Token() { pos = 1; line = 1; type = Type.NONE; strval = ""; }

		public override string ToString()
		{
			return this.type + " " + this.line + " " + this.pos + " " + this.strval;
		}

		public string GetStrVal()
		{
			return strval;
		}
	}

   class Scaner
   {
		public class Exception : System.Exception
		{
			public Exception(string message, int line, int pos) : 
				base("Лексическая ошибка в строке " + line + " позиции " + pos + ": " + message) { }
		}

		class Buffer: StreamReader
		{
			public const int EOF = -1;
			private int pos = 1, line = 1;

			public Buffer(Stream stream) : base(stream) {}

			public override int Read()
			{
				int ch = base.Read();

				if (ch == '\n')
				{
					line++; pos = 1;
				}
				else
					pos++;

				return ch;
			}

			public int GetLine()
			{
				return line;
			}

			public int getPos()
			{
				return pos;
			}
		}

		Buffer buf;
		int line = 1, pos = 1;
		string val;
		Token.Type type;

		Token next_token = null;
		Scaner.Exception error = null;

      public Scaner(System.IO.Stream istream) { 
			buf = new Buffer(istream);
			try
			{
				next_token = Next();
			}
			catch (Scaner.Exception e)
			{
				error = e;
			}
		}

		private int ReadInValue()
		{
			int ch = buf.Peek();
			val += (char)buf.Read();
			return ch;
		}

		private void throw_exception(string s)
		{
			int pos = buf.getPos(), line = buf.GetLine();

			//while (!IsWhite(buf.Peek()) && buf.Peek() != -1 && buf.Peek() != ';') { buf.Read(); };

			throw new Exception(s, line, pos);
		}

#region helper functions

		private bool IsDecimal(int ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private bool IsOctal(int ch)
		{
			return ch >= '0' && ch <= '7';
		}

		private bool IsHex(int ch)
		{
			return IsDecimal(ch) || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f';
		}

		private bool IsWhite(int ch)
		{
			return char.IsWhiteSpace((char)ch);
		}

		private bool IsAlpha(int ch)
		{
			return ch == '_' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z';
		}

		private bool IsDigit(int ch, int platform)
		{
			switch (platform)
			{
				case 8:
					return IsOctal(ch);
				case 10:
					return IsDecimal(ch);
				case 16:
					return IsHex(ch);
				default:
					return false;
			}
		}

		private bool IsEnter(string s, int ch)
		{
			return s.IndexOf((char)ch) > -1;
		}

		private bool IsSeparator(int ch)
		{
			return "{};,()".IndexOf((char)ch) > -1;
		}

		private bool IsOperator(int ch)
		{
			return "+-=/%*&|^~?!.:<>[]".IndexOf((char)ch) > -1;
		}

		private bool IsPermissed(int ch)
		{
			return IsAlpha(ch) || IsDecimal(ch) || IsWhite(ch) || IsSeparator(ch) || IsOperator(ch);
		}

#endregion

#region convert functions
		private int StringToInt(string s, int platform)
		{
			try
			{
				return Convert.ToInt32(s, platform);
			}
			catch (OverflowException)
			{
				throw_exception("невозможно преобразовать \"" + s + "\" в число int");
				return 0;
			}
		}

		private double StringToDouble(string s)
		{
			try
			{
				return Convert.ToDouble(s.Replace('.', ','));
			}
			catch (OverflowException)
			{
				throw_exception("невозможно преобразовать \"" + s + "\" в число double");
				return 0;
			}
		}

#endregion

#region parsing functions

		private int GetInteger(int pl = 0, bool part_float = false)
		{
			int platform = 10;
			int first_ch = buf.Peek();
			string res = "";

			if (first_ch == '0')
			{
				
				//ReadInValue();
				res += (char)buf.Read();

				if ((buf.Peek() == 'x' || buf.Peek() == 'X'))
				{
					//ReadInValue();
					res += (char)buf.Read();
					platform = 16;
				}
				else if(IsOctal(buf.Peek()) && !part_float)
				{
					platform = 8;
				}
			}

			if (pl != 0 && pl != platform)
			{
				throw_exception("недопустимая запись для основания \"" + pl + "\"");// неверная система счисления
			}

			if (first_ch != '0' || platform != 10)
			{
				if (!IsDigit(buf.Peek(), platform))
				{
					//не обходимо число
					throw_exception("недопустимый символ \"" + (char)buf.Peek() + "\" для основания \"" + platform + "\"");
				}
			}

			while (IsDigit(buf.Peek(), platform)) { res += (char)buf.Read(); }

			val += StringToInt(res, platform);

			return platform;
		}

		private int GetFloat(bool is_exist_integer = false, int pl = 0){
			int platform = 0;

			if (buf.Peek() == '.')
			{
				ReadInValue();

				if (IsDecimal(buf.Peek()))
					platform = GetInteger(pl, true);
				else if(!is_exist_integer)
				{
					type = Token.Type.OPERATOR;

					if (buf.Peek() == '.')
					{
						ReadInValue();
						if (buf.Peek() != '.')
						{
							throw_exception("необходима \".\"");
						}
						ReadInValue();			
					} 
		
					return 0;
				}
			}
			else if (!is_exist_integer)
			{
				throw_exception("необходима \".\"");//необходима .
			}

			if (IsEnter("EepP", buf.Peek()))
			{
				if ((platform == 8 || platform == 16) && (!IsEnter("Pp", buf.Peek())))
				{
					throw_exception("необходимо \"P\" для основания \"" + platform + "\"");// ожидалось p 
				}
				else if (platform == 10 && (!IsEnter("Ee", buf.Peek())))
				{
					throw_exception("необходимо \"E\" для основания \"10\"");// ожидалось E
				}

				val += 'E';
				buf.Read();

				if (IsEnter("+-", buf.Peek()))
				{
					ReadInValue();
				}

				if (!IsDecimal(buf.Peek()))
				{
					throw_exception("требуется экспоненциальное значение");//полсе Е должна быть цифра
				}

				GetInteger(platform, true);
			}

			if (!is_exist_integer)
				val = StringToDouble(val).ToString();

			if (IsAlpha(buf.Peek()))
				throw_exception("неверная запись числа");

			return platform;
		}

		private void GetScalar()
		{
			int platform = GetInteger();

			if (IsEnter(".eEpP", buf.Peek()))
			{
				GetFloat(true, platform);
				val = StringToDouble(val).ToString();
				type = Token.Type.CONST_DOUBLE;
			}
			else
			{
				type = Token.Type.CONST_INT;
			}

			if (IsAlpha(buf.Peek()))
			{
				throw_exception("недопустимый символ \"" + (char)buf.Peek() + "\" в записи числа.");
			}
			else if ((platform == 8 && IsDecimal(buf.Peek())))
			{
				throw_exception("недопустимая цифра \"" + (char)buf.Peek() + "\" для основания \"8\"");
			}
		}

		private void GetIdentificator()
		{

			while (IsAlpha(buf.Peek()) || IsDecimal(buf.Peek())) { ReadInValue(); }

			if (!IsPermissed(buf.Peek()))
			{
				throw_exception("недопустимый символ \"" + (char)buf.Peek() + "\"");
			}

			type = Token.Type.IDENTIFICATOR;
		}

		private void PassComment(bool multiline = false)
		{
			int ch;
			if (multiline)
			{
				do
				{
					ch = buf.Read();
				} while (!(ch == '*' && buf.Peek() == '/' || ch == Buffer.EOF));

				buf.Read();
			}
			else
			{
				do
				{
					ch = buf.Read();
				} while (!(ch == '\n' || ch == Buffer.EOF));
			}
		}

		private void GetOperator()
		{
			int ch = ReadInValue();

			if (":?[]~".IndexOf((char)ch) > -1)
			{
				return;
			}
			else if (ch == '/' && (buf.Peek() == '/' || buf.Peek() == '*'))
			{
				PassComment(buf.Read() == '*');
				Read();
			}
			else if (IsOperator(buf.Peek()))
			{
				if ((buf.Peek() == '=' && "+-/%*<>|&^!".IndexOf((char)ch) > -1)  // += -= /= и тд
					|| (ch == '-' && buf.Peek() == '>') // ->
					|| (buf.Peek() == ch && "+-<>=|&.".IndexOf((char)buf.Peek()) > -1)) // ++ -- >> и тд
				{
					ch = ReadInValue();

					if (ch == '.' && ch != buf.Peek())
					{
						throw_exception("ожидался оператор \"...\"");
					}


					if (((ch == '<' || ch == '>') && buf.Peek() == '=') || (ch == '.' && buf.Peek() == '.'))
					{
						ReadInValue();
					}
				}

			}

		}

		private int ReadChar()
		{
			string result = "";

			if (buf.Peek() == '\\')
			{
				buf.Read();
				int ch = buf.Read();
				result += (char)ch;

				switch (ch)
				{
					case '\'': return '\'';
					case '\"': return '\"';
					case '\\': return '\\';
					case 'a': return '\a';
					case 'b': return '\b';
					case 'f': return '\f';
					case 'n': return '\n';
					case 'r': return '\r';
					case 't': return '\t';
					case 'v': return '\v';
					case 'x':
						result = '0' + result;

						if (!IsHex(buf.Peek()))
						{
							throw_exception("недопустимый символ \"" + (char)buf.Peek() + "\" для основания \"16\"");
						}

						while (IsHex(buf.Peek())) { result += (char)buf.Read(); };

						return StringToInt(result, 16);
					default:
						int i = 0;
						while (IsOctal(buf.Peek()) && i < 3) { result += (char)buf.Read(); i++; };

						if(i == 0)
							throw_exception("недопустимый символ \"" + (char)buf.Peek() + "\" для основания \"8\"");

						return StringToInt(result, 8);
				}
			}
			else
				return buf.Read();
		}

		private void GetChar()
		{
			buf.Read(); //пропускаем первую ковычку
			val += (char)ReadChar();
			if (buf.Peek() != '\'')
			{
				throw_exception("отсутствует \" \' \"");
			}

			buf.Read();//пропускаем закрывающуюся ковычку
		}

		private void GetString()
		{
			buf.Read();

			while (buf.Peek() != '\n' && buf.Peek() != '\"' && buf.Peek() != -1)
			{
				val += (char)ReadChar();
			}

			if (buf.Peek() == '\n')
			{
				throw_exception("отсутствует \"\"\"");
			}

			buf.Read();
		}

#endregion

		private Token Next()
		{
			type = Token.Type.NONE;
			val = "";

			while (IsWhite(buf.Peek())) { buf.Read(); }

			line = buf.GetLine();
			pos = buf.getPos();

			int ch = buf.Peek();

			if (ch == Buffer.EOF)
			{
				type = Token.Type.EOF;
			}
			else if (IsDecimal(ch))
			{
				GetScalar();
			}
			else if (IsAlpha(ch))
			{
				GetIdentificator();

				Object keyword = Token.terms[val];
				type = keyword == null? Token.Type.IDENTIFICATOR: (Token.Type)keyword;
			}
			else if (ch == '.')
			{
				type = Token.Type.CONST_DOUBLE;
				GetFloat();
			}
			else if (IsSeparator(ch))
			{
				type = Token.Type.SEPARATOR;
				ReadInValue();
			}
			else if (IsOperator(ch))
			{
				type = Token.Type.OPERATOR;
				GetOperator();
			}
			else if (ch == '\'')
			{
				type = Token.Type.CONST_CHAR;
				GetChar();
			}
			else if (ch == '\"')
			{
				type = Token.Type.CONST_STRING;
				GetString();
			}
			else
			{
				throw_exception("недопустимый символ \"" + (char)ch + "\"");
			}

			return new Token(pos, line, type, val);;
		}

		private void CheckError(bool is_read = false)
		{
			if (error != null)
			{
				Scaner.Exception res = error;
				error = null;
				throw res;
			}
		}

		public Token Read() 
		{
			CheckError();
			Token res = next_token;

			try
			{
				next_token = Next();
			}
			catch (Scaner.Exception e)
			{
				error = e;
			}

			return res;
		}

		public void Pass()
		{
			while (!IsWhite(buf.Peek()) && buf.Peek() != -1 && buf.Peek() != ';') { buf.Read(); };
			try
			{
				next_token = Next();
			}
			catch (Scaner.Exception e)
			{
				error = e;
			}
		}

		public Token Peek()
		{
			CheckError();
			return next_token;
		}

		public int GetLine()
		{
			return line;
		}

		public int GetPos()
		{
			return pos;
		}
	}
}
