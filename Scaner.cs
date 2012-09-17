using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler
{
   class Scaner
   {
		public class Exception : Compiler.Exception
		{
			public Exception(string message, int index, int line) 
				: base("Лексическая ошибка", message, index, line) { }
		}

		private class Buffer: StreamReader
		{
			public const int EOF = -1;
			private int pos = 1, line = 1;

			public Buffer(Stream stream) : base(stream) { }

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

		private Buffer buf;
		private int line = 1, pos = 1;
		private string val;
		private Token.Type type;

		private Token next_token = null;
		private Scaner.Exception error = null;

      public Scaner(System.IO.StreamReader istream) {
			buf = new Buffer(istream.BaseStream);
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
			return char.IsWhiteSpace((char)ch) && ch != -1;
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
			return s.IndexOf((char)ch) > -1 && ch != -1;
		}

		private bool IsSeparator(int ch)
		{
			return "{};,()".IndexOf((char)ch) > -1 && ch != -1;
		}

		private bool IsOperator(int ch)
		{
			return "+-=/%*&|^~?!.:<>[]".IndexOf((char)ch) > -1 && ch != -1;
		}

		private bool IsPermissed(int ch)
		{
			return IsAlpha(ch) || IsDecimal(ch) || IsWhite(ch) || IsSeparator(ch) || IsOperator(ch) || ch == -1;
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

				Object keyword = Token.Terms[val];
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
