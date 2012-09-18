using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	namespace Symbols
	{
		class Exception : Compiler.Exception
		{
			public Exception(string s, int index, int line) : base("Семантическая ошибка", s, index, line) { }
			public Exception(Syntax.Expression expr, string message)
				: base("Семантическая ошибка", message, expr.GetIndex(), expr.GetLine()) { }
		}


		abstract class Symbol
		{
			protected string name = "";
			protected int index = -1 , line = -1;

			public Symbol() { }

			public Symbol(string name, int index, int line)
			{
				this.SetName(name);
				this.line = line;
				this.index = index;
			}

			public Symbol(Token t)
			{
				this.index = t.pos;
				this.line = t.line;
				this.SetName(t.GetStrVal());
			}

			public string GetName()
			{
				return this.name.Length > 0 ? this.name : "unnamed";
			}

			public void SetName(string name)
			{
				this.name = name;
			}

			public void SetPosition(int index, int line)
			{
				this.line = line;
				this.index = index;
			}

			public int GetLine()
			{
				return this.line;
			}

			public int GetIndex()
			{
				return this.index;
			}

			virtual public void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + this.name);
			}

			public override string ToString()
			{
				return this.name.Length > 0 ? this.name : "unnamed";
			}

			abstract public bool Equals(Symbol t);
		}


		class Var : Symbol
		{
			protected Type type = null;
			protected Syntax.Expression value = null;

			public Var() : base() { }

			public Var(Token t) : base(t) { }

			public Var(string name, int index, int line) : base(name, index, line) { }

			public void SetType(Type type) 
			{
				type.ComputeSize();
				this.type = type;
			}

			new public Type GetType()
			{
				return this.type is TYPEDEF ? ((TYPEDEF)this.type).GetRefType() : this.type;
			}

			public void SetInitializer(Syntax.Expression init)
			{
				this.value = init;
			}

			public override void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + "var ");
				base.Print(stream, 0);
				stream.Write(" type ");
				this.GetType().Print(stream, 0);
				if (this.value != null)
				{
					stream.Write(stream.NewLine);
					this.value.Print(stream, indent);
				}
			}

			public override bool Equals(Symbol t)
			{
				Symbol tt = ((Var)t).GetType();
				return t is Var && t.GetName() == this.GetName() && this.type.Equals(tt);
			}
		}


		class SuperVar : Var
		{
			public SuperVar() : base() { this.type = new SuperType(); }

			public SuperVar(Token t) : base(t) { this.type = new SuperType(); }

			public SuperVar(string name, int index, int line) : base(name, index, line) { this.type = new SuperType(); }

			public override void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + "super ");
				base.Print(stream, 0);
			}

			public override bool Equals(Symbol t)
			{
				return t is Var;
			}
		}


		class ConstVar : Var
		{
			public ConstVar() : base() { }

			public ConstVar(Token t) : base(t) { }

			public ConstVar(string name, int index, int line) : base(name, index, line) { }

			public override void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + "const ");
				base.Print(stream, 0);
			}

			public override bool Equals(Symbol t)
			{
				return t is ConstVar && base.Equals(t);
			}
		}


		class GlobalVar : Var
		{
			public GlobalVar() : base() { }

			public GlobalVar(Token t) : base(t) { }

			public GlobalVar(string name, int index, int line) : base(name, index, line) { }
		}


		class LocalVar : Var
		{
			public LocalVar() : base() { }

			public LocalVar(Token t) : base(t) { }

			public LocalVar(string name, int index, int line) : base(name, index, line) { }
		}


		class ParamVar : Var
		{
			public ParamVar() : base() { }

			public ParamVar(Token t) : base(t) { }

			public ParamVar(string name, int index, int line) : base(name, index, line) { }

			public override bool Equals(Symbol t)
			{
				return t is ParamVar && this.type.Equals(((Var)t).GetType());
			}
		}


		abstract class Type : Symbol
		{
			protected int __size_t;
			protected virtual int size_t { get { return this.__size_t; } set { this.__size_t = value; } }

			public Type() : base() { }

			public Type(Token t) : base(t) { }

			public Type(string name, int index, int line) : base(name, index, line) { }

			virtual public bool IsArifmetic()
			{
				return false;
			}

			virtual public bool IsInteger()
			{
				return false;
			}

			public int GetSizeType()
			{
				return this.size_t;
			}

			public override void Print(StreamWriter stream, int indent)
			{
				base.Print(stream, indent);
			}

			public override bool Equals(Symbol t)
			{
				return t is Type && t.GetName() == this.GetName();
			}

			public virtual void ComputeSize() { }
		}


		class SuperType : Type 
		{
			public SuperType()
			{
				this.name = "super";
			}

			public override bool IsArifmetic()
			{
				return true;
			}

			public override bool IsInteger()
			{
				return true;
			}

			public override bool Equals(Symbol t)
			{
				return t is Type;
			}
		}


		abstract class TypeScalar : Type
		{
			public TypeScalar() : base() { }

			public TypeScalar(Token t) : base(t) { }

			public TypeScalar(string name, int index, int line) : base(name, index, line) { }

			override public bool IsArifmetic() { return true; }
		}


		class VOID : TypeScalar 
		{
			public VOID() : base() { this.name = "void"; }

			public VOID(Token t) : base(t) { }

			public VOID(string name, int index, int line) : base(name, index, line) { }

			override public bool IsArifmetic() { return false; }
		}


		class INT : TypeScalar
		{
			protected override int size_t { get { return 4;} }

			public INT() : base() { this.name = "int"; }

			public INT(Token t) : base(t) { }

			public INT(string name, int index, int line) : base(name, index, line) { }

			public override bool IsInteger()
			{
				return true;
			}
		}


		class CHAR : TypeScalar
		{
			protected override int size_t { get { return 1; } }

			public CHAR() : base() { this.name = "char"; }

			public CHAR(Token t) : base(t) { }

			public CHAR(string name, int index, int line) : base(name, index, line) { }

			public override bool IsInteger()
			{
				return true;
			}
		}


		class DOUBLE : TypeScalar
		{
			protected override int size_t { get { return 8; } }

			public DOUBLE() : base() { this.name = "double"; }

			public DOUBLE(Token t) : base(t) { }

			public DOUBLE(string name, int index, int line) : base(name, index, line) { }
		}


		class RECORD : Type
		{
			protected Table table = null;

			public RECORD() : base() { }

			public RECORD(Token t) : base(t) { }

			public RECORD(string name, int index, int line) : base(name, index, line) { }

			public void SetFields(Table table)
			{
				this.table = table;
				if (table == null || table.GetSize() == 0)
				{
					throw new Exception("необходим хотя бы один элемент", this.GetIndex(), this.GetLine());
				}
			}

			public Table GetTable()
			{
				return this.table;
			}

			public override void Print(StreamWriter stream, int indent)
			{
				int base_indent = stream.GetCharPos();
				base.Print(stream, indent);
				stream.Write(" record");
				stream.Write(stream.NewLine);
				this.table.Print(stream, base_indent + indent + 3);
				stream.Write(new String(' ', base_indent + indent) + " endrecord");
			}

			public override bool Equals(Symbol t)
			{
				return t is RECORD && base.Equals(t) && this.table.Equals(((RECORD)t).table);
			}

			public override string ToString()
			{
				return "struct " + this.GetName();
			}

			public override void ComputeSize()
			{
				if (this.table == null)
				{
					return;
				}

				int size = 0;
				foreach (Var v in this.table.symbols.Values.Where(el => el is Var))
				{
					v.GetType().ComputeSize();
					size += v.GetType().GetSizeType();
				}
				this.size_t = size;
			}
		}


		class ENUM : Type
		{
			public ENUM() : base() { }

			public ENUM(Token t) : base(t) 
			{
				this.name = "enum";
			}

			public ENUM(string name, int index, int line) : base(name, index, line) { }
		}


		abstract class RefType : Type
		{
			protected Type type = null;

			public RefType() : base() { }

			public RefType(Token t) : base(t) { }

			public RefType(string name, int index, int line) : base(name, index, line) { }

			public RefType(Type t)
			{
				this.SetType(t);
			}

			public RefType(Token t, Type ty)
				: base(t)
			{
				this.SetType(ty);
			}

			virtual public void SetType(Type type)
			{
				this.type = type;
			}

			public override string ToString()
			{
				return this.GetRefType().ToString() + " *";
			}

			public Type GetRefType()
			{
				return this.type is TYPEDEF ? ((TYPEDEF)this.type).GetRefType() : this.type;
			}
		}


		class TYPEDEF : RefType
		{
			public TYPEDEF() : base() { }

			public TYPEDEF(Token t) : base(t) { }

			public TYPEDEF(Type t) : base(t) 
			{
				this.size_t = t.GetSizeType();
			}

			public override void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + "typedef <" + this.name + "> <");
				this.GetRefType().Print(stream, 0);
			}

			public override bool Equals(Symbol t)
			{
				return t.Equals(this.GetRefType());
			}

			public override string ToString()
			{
				return this.GetRefType().ToString();
			}

			public override void ComputeSize()
			{
				this.GetRefType().ComputeSize();
				this.size_t = this.GetRefType().GetSizeType();
			}
		}


		class POINTER : RefType
		{
			protected override int size_t { get { return 4; } }

			public POINTER() : base() { }

			public POINTER(Token t) : base(t) 
			{
				this.name = "pointer";
			}

			public POINTER(Type t) : base(t) { }

			public POINTER(Token t, Type ty) : base(t) 
			{
				this.SetType(ty);
				this.name = "pointer";
			}

			public POINTER(string name, int index, int line) : base(name, index, line) { }

			public override bool Equals(Symbol t)
			{
				return t is POINTER && this.type.Equals(((RefType)t).GetRefType());
			}

			public override void Print(StreamWriter stream, int indent)
			{
				base.Print(stream, indent);
				stream.Write(" to ");
				this.GetRefType().Print(stream, 0);
			}

			public override string ToString()
			{
				return "*(" + this.GetRefType().ToString() + ")";
			}
		}


		class ARRAY : POINTER
		{
			protected override int size_t { get { return this.__size_t; } }

			protected const int DIMENSIONLESS = -1;
			protected int size = DIMENSIONLESS;

			public ARRAY() : base() { }

			public ARRAY(Token t)
				: base(t)
			{
				this.name = "array";
			}

			public ARRAY(string name, int index, int line) : base(name, index, line) { }

			public ARRAY(Type t)
				: base(t)
			{
				this.name = "array";
			}

			public void SetSize(Syntax.Expression size)
			{
				Syntax.Object.CheckObject(size);
				try
				{
					this.size = size.ComputeConstIntValue();
				}
				catch (Syntax.CanNotCalculated e)
				{
					throw e;
				}
				this.ComputeSizeType();
			}

			public override void SetType(Type type)
			{
				this.type = type;
				this.ComputeSizeType();
			}

			private void ComputeSizeType()
			{
				if (this.size == ARRAY.DIMENSIONLESS || this.type == null)
				{
					return;
				}
			}

			public override void Print(StreamWriter stream, int indent)
			{
				stream.Write(new String(' ', indent) + "array");
				stream.Write(this.size == ARRAY.DIMENSIONLESS ? " dimensionless" : " size " + this.size);
				stream.Write(" of ");
				this.GetRefType().Print(stream, 0);
			}

			public override string ToString()
			{
				return this.GetRefType().ToString() + "[" + this.size + "]";
			}

			public override void ComputeSize()
			{
				this.GetRefType().ComputeSize();
				this.size_t = this.GetRefType().GetSizeType() * this.size;
			}
		}


		class Func : RefType
		{
			protected override int size_t { get { return 4; } }

			protected List<ParamVar> args = new List<ParamVar>();
			protected Syntax.Statement body = null;
			protected Table table = null;

			public Func() : base() { }

			public Func(Token t) : base(t) { }

			public Func(Type t) : base(t) { }

			public Func(string name, int index, int line) : base(name, index, line) { }


			public bool Merge(Func f)
			{
				if (f.Equals(this) && !f.IsEmptyBody() && this.IsEmptyBody())
				{
					this.SetBody(f.body);
					this.SetTable(f.table);
					return true;
				}
				return false;
			}


			public override bool Equals(Symbol t)
			{
				if (t is Func && base.Equals(t))
				{
					Func f = (Func)t;
					if (f.args.Count != this.args.Count)
					{
						return false;
					}

					for (int i = 0; i < f.args.Count; ++i)
					{
						if (!args.ElementAt(i).Equals(f.args.ElementAt(i)))
						{
							return false;
						}
					}
					return true;
				}
				return false;
			}


			public void SetArguments(List<ParamVar> args)
			{
				this.args = args;
			}

			public List<ParamVar> GetArguments()
			{
				return this.args;
			}

			public void AddArgument(ParamVar arg)
			{
				this.args.Add(arg);
			}

			public void SetBody(Syntax.Statement body) 
			{
				this.body = body;
				this.body.Modified();
			}

			public void SetTable(Table table)
			{
				this.table = table;
			}

			public Table GetTable()
			{
				return this.table;
			}

			public bool IsEmptyBody()
			{
				return this.body == null;
			}

			public override void Print(StreamWriter stream, int indent)
			{
				int base_indent = stream.GetCharPos();
				stream.Write("function " + this.name + "(");
				foreach (Var param in this.args)
				{
					param.Print(stream, 0);
					stream.Write(", ");
				}
				stream.Write("){");
				if (this.body != null)
				{
					stream.Write(stream.NewLine);
					this.table.Print(stream, base_indent + indent + 3);
					this.body.Print(stream, base_indent + indent + 3);
				}
				stream.Write(stream.NewLine);
				stream.Write("} returned ");
				this.GetRefType().Print(stream, 0);
			}

			public override string ToString()
			{
				string res = this.GetRefType() == null ? "" : this.GetRefType().ToString();
				res += "(*)";

				foreach (Var arg in this.args.Where(x => x.GetType() != null))
				{
					res += arg.GetType().ToString() + ", ";
				}

				if (this.args.Where(x => x.GetType() != null).Count() > 0)
				{
					res = res.Substring(0, res.Length - 2);
				}

				return res + ")";
			}
		}
	}
}
