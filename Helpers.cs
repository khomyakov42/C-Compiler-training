using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	class Pair<T1, T2>
	{
		public T1 first;
		public T2 last;

		public Pair(T1 _first, T2 _last)
		{
			first = _first;
			last = _last;
		}

		public Pair()
		{
			first = default(T1);
			last = default(T2);
		}
	}


	class StreamWriter : System.IO.StreamWriter
	{
		private long charPos = 0;
		private long charLine = 0;

		public StreamWriter(System.IO.Stream stream) : base(stream) { }
		
		public StreamWriter(string path) : base(path) { }
		
		public StreamWriter(System.IO.Stream stream, Encoding encoding) : base(stream, encoding) { }
		
		public StreamWriter(string path, bool append) : base(path, append) { }
		
		public StreamWriter(System.IO.Stream stream, Encoding encoding, int bufferSize) 
			: base(stream, encoding, bufferSize) { }

		public StreamWriter(string path, bool append, Encoding encoding)
			: base(path, append, encoding) { }

		public StreamWriter(string path, bool append, Encoding encoding, int bufferSize)
			: base(path, append, encoding, bufferSize) { }

		public override void Write(string value)
		{
			base.Write(value);

			if (value.IndexOf(this.NewLine) != -1)
			{
				this.charPos = 0;
				this.charLine += (value.Length - value.Replace(this.NewLine, "").Length) / this.NewLine.Length;
			}
			else
			{
				this.charPos += value.Length;
			}
		}

		public int GetCharPos()
		{
			return (int)this.charPos;
		}

		public int GetCharLine()
		{
			return (int)this.charLine;
		}
	}


	class Three<T1, T2, T3> : Pair<T1, T3>
	{
		public T2 middle;
		public Three(T1 _first, T2 _middle, T3 _last)
		{
			first = _first;
			middle = _middle;
			last = _last;
		}

		public Three()
		{
			first = default(T1);
			middle = default(T2);
			last = default(T3);
		}
	}


	class Exception : System.Exception
	{
		public override string Message
		{
			get
			{
				return this.title + string.Format(" в строке {0}, позиции {1}: {2}", this.line, this.index, base.Message);
			}
		}
		protected string title = "";
		public int index, line;
		public Exception(string title, string message, int index, int line) : base(message)
		{
			this.title = title;
			this.index = index;
			this.line = line;
		}

		public Exception(string message, int index, int line)
			: base(message)
		{
			this.line = line;
			this.index = index;
		}
	}


	abstract class PrintObject
	{
		public void Print(StreamWriter stream, int indent = 0)
		{
			this.Print(stream, new String(' ', indent), true);
		}


		protected string GenIndent(string indent, bool last = false)
		{
			return indent + (last ? "└──" : "├──");
		}

		virtual protected void Print(StreamWriter stream, string indent = "", bool last = false)
		{
			stream.Write(GenIndent(indent, last) + "{" + this.ToString() + "}");
			stream.AutoFlush = true;
			this.PrintChildrens(stream, indent, last);
		}

		virtual protected void PrintChildrens(StreamWriter stream, string indent = "", bool last = false)
		{
			List<PrintObject> childrens = this.GetChildrens();
			for (int i = childrens.Count - 1; i >= 0; --i)
			{
				stream.Write(stream.NewLine);
				childrens.ElementAt(i).Print(stream, indent + (last ? "   " : "│  "), i == childrens.Count - 1);
			}
		}

		public abstract List<PrintObject> GetChildrens();
	}
}
