using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
	namespace Symbols
	{

		
		class Table
		{
			public Table parent = null;
			public List<Table> childrens = new List<Table>();
			private int number = 0;
			public Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
			public Dictionary<string, Symbol> tags = new Dictionary<string, Symbol>();

			public Table() { }

			public Table(Table parent, int number)
			{
				this.number = number;
				this.parent = parent;
			}

			public void AddSymbols(Table table)
			{
				this.symbols = this.symbols.Concat(table.symbols).ToDictionary(v =>v.Key, v=> v.Value);
				this.tags = this.tags.Concat(table.tags).ToDictionary(v => v.Key, v => v.Value);
			}

			private void CheckName(Symbol symbol, params Dictionary<string, Symbol>[] dicts)
			{
				foreach (var dict in dicts)
				{
					if (dict.ContainsKey(symbol.GetName()))
					{
						throw new Symbols.Exception(string.Format(
							"переопределение {0}", symbol.GetName()
						), symbol.GetIndex(), symbol.GetLine());
					}
				}
			}


			public IEnumerable<Var> GetVariables()
			{
				return this.symbols.Values.Where(var => var is Var).ToList().Cast<Var>();
			}

			public void AddSymbol(Symbol symbol)
			{
				if (symbol is Var && ((Var)symbol).GetType() is Func 
					&& this.ContainsVariable(symbol.GetName()) 
					&& symbol.Equals(this.GetVariable(symbol.GetName())))
				{
					Func f = (Func)this.GetVariable(symbol.GetName()).GetType();

					if (f.Merge((Func)((Var)symbol).GetType()))
					{
						return;
					}
				}

				CheckName(symbol, this.symbols);
				this.symbols.Add(symbol.GetName(), symbol);
			}

			public void RemoveSymbol(Symbol symbol)
			{
				try
				{
					this.symbols.Remove(symbol.GetName());
				}
				catch (Exception) { }
			}

			public void AddTag(Symbol tag)
			{
				CheckName(tag, this.tags);
				this.tags.Add(tag.GetName(), tag);
			}

			public bool ContainsSymbol(string name, System.Type symbol_type)
			{
				return this.symbols.ContainsKey(name) && symbol_type.IsAssignableFrom(this.symbols[name].GetType());
			}

			public Symbol GetSymbol(Token t, System.Type symbol_type)
			{
				if (!this.ContainsSymbol(t.GetStrVal(), symbol_type))
				{
					string error = "";
					if (symbol_type == typeof(Type))
					{
						error = string.Format("тип \"{0}\" не определен", t.GetStrVal());
					}
					else if (symbol_type == typeof(Var))
					{
						error = string.Format("переменная \"{0}\" не определена", t.GetStrVal());
					}

					throw new Symbols.Exception(error, t.GetIndex(), t.GetLine());
				}
				return this.symbols[t.GetStrVal()];
			}

			public Symbol GetSymbol(string name, System.Type symbol_type)
			{
				return this.GetSymbol(new Token(Token.Type.IDENTIFICATOR, name), symbol_type);
			}

			public Symbol GetTag(Token t)
			{
				if (!this.ContainsTag(t.GetStrVal()))
				{
					throw new Symbols.Exception(string.Format("тэг \"{0}\" не определен", t.GetStrVal()), t.GetIndex(), t.GetLine());
				}
				return this.tags[t.GetStrVal()];
			}

			public Symbol GetTag(string name)
			{
				return this.GetTag(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public bool ContainsVariable(string name)
			{
				return this.ContainsSymbol(name, typeof(Var));
			}

			public bool ContainsType(string name)
			{
				return this.ContainsSymbol(name, typeof(Type));
			}

			public bool ContainsTag(string name)
			{
				return this.tags.ContainsKey(name);
			}

			public Var GetVariable(Token t)
			{
				return (Var)this.GetSymbol(t, typeof(Var));
			}

			public Var GetVariable(string name)
			{
				return this.GetVariable(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public Type GetType(Token t)
			{
				return (Type)this.GetSymbol(t, typeof(Type));
			}

			public Type GetType(string name)
			{
				return this.GetType(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public long GetSize()
			{
				return this.symbols.Count + this.tags.Count();
			}

			public long GetCountSymbol()
			{
				return this.symbols.Count;
			}

			public long GetCountTag()
			{
				return this.tags.Count();
			}

			public bool Equels(Table t)
			{
				foreach (Symbol s in t.symbols.Values)
				{
					if (!this.symbols.ContainsKey(s.GetName()) || !this.symbols[s.GetName()].Equals(s))
					{
						return false;
					}
				}

				foreach (Symbol s in t.tags.Values)
				{
					if (!this.tags.ContainsKey(s.GetName()) || !this.tags[s.GetName()].Equals(s))
					{
						return false;
					}
				}

				return true;
			}

			public void Print(StreamWriter stream, int indent = 0)
			{
				string s_indent = new String(' ', indent);
				stream.WriteLine(s_indent + "<<<table #" + this.number + ">>>");

				stream.WriteLine(s_indent + "<tags>");
				foreach (var pair in this.tags)
				{
					pair.Value.Print(stream, indent);
					stream.Write(stream.NewLine);
				}

				stream.WriteLine(s_indent + "<types>");
				foreach (var pair in this.symbols.Where(kvpair => kvpair.Value is Type))
				{
					pair.Value.Print(stream, indent);
					stream.Write(stream.NewLine);
				}
				
				stream.WriteLine(s_indent + "<vars>");
				foreach (var pair in this.symbols.Where(kvpair => kvpair.Value is Var))
				{
					pair.Value.Print(stream, indent);
					stream.Write(stream.NewLine);
				}

				this.childrens.ForEach(table => table.Print(stream, indent + 1));
				
			}
		}

		partial class StackTable
		{
			private Table root = null, current = null;
			private int count_tables = 0;

			public Table GetRoot()
			{
				return this.root;
			}

			public StackTable() 
			{
				this.NewTable();
			}

			public void NewTable()
			{
				int index = current == null ? 0 : current.childrens.Count + 1;
				Table table = new Table(this.current, ++this.count_tables);
				if (this.root == null)
				{
					this.current = this.root = table;
				}
				else
				{
					this.current.childrens.Add(table);
					this.current = table;
				}
			}

			public void StepUp()
			{
				this.current = this.current.parent;
			}

			public Table GetCurrentTable()
			{
				return this.current;
			}

			public Table PopTable()
			{
				Table res = this.current;
				this.current = res.parent;
				try
				{
					this.current.childrens.RemoveAt(this.current.childrens.FindIndex(table => table == res));
				}
				catch (System.ArgumentOutOfRangeException){ }
				return res;
			}

			private bool ContainsSymbol(string name, System.Type symbol_type)
			{
				Table itr = this.current;
				while (itr != null)
				{
					if (itr.ContainsSymbol(name, symbol_type))
					{
						return true;
					}
					itr = itr.parent;
				}
				return false;
			}

			private bool ContainsTag(string name)
			{
				Table itr = this.current;
				while (itr != null)
				{
					if (itr.ContainsTag(name))
					{
						return true;
					}
					itr = itr.parent;
				}
				return false;
			}

			private Symbol GetSymbol(Token t, System.Type symbol_type)
			{
				Table itr = this.current;
				while (itr != null)
				{
					if (itr.ContainsSymbol(t.GetStrVal(), symbol_type))
					{
						return itr.GetSymbol(t, symbol_type);
					}
					itr = itr.parent;
				}
				return this.root.GetSymbol(t, symbol_type);
			}

			public Symbol GetTag(Token t)
			{
				Table itr = this.current;
				while (itr != null)
				{
					if (itr.ContainsTag(t.GetStrVal()))
					{
						return itr.GetTag(t);
					}
					itr = itr.parent;
				}
				return this.root.GetTag(t);
			}

			public Symbol GetTag(string name)
			{
				return this.GetTag(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public void AddSymbol(Symbol symbol)
			{
				this.current.AddSymbol(symbol);
			}

			public void AddTag(Symbol tag)
			{
				this.current.AddTag(tag);
			}

			public bool ContainsType(string name)
			{
				return this.ContainsSymbol(name, typeof(Type));
			}

			public bool ContainsVariable(string name)
			{
				return this.ContainsSymbol(name, typeof(Var));
			}

			public Var GetVariable(Token t)
			{
				return (Var)this.GetSymbol(t, typeof(Var));
			}

			public Var GetVariable(string name)
			{
				return this.GetVariable(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public Type GetType(Token t)
			{
				return (Type)this.GetSymbol(t, typeof(Type));
			}

			public Type GetType(string name)
			{
				return (Type)this.GetType(new Token(Token.Type.IDENTIFICATOR, name));
			}

			public bool IsGlobal()
			{
				return this.root == this.current;
			}

			public void Print(StreamWriter stream)
			{
				this.root.Print(stream);
			}
		}
	}
}
