using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Compiler
{
	
	class SymTable
	{
		Dictionary<string, Symbol> table = new Dictionary<string,Symbol>();

		public void Add(SymVar sym)
		{
			table.Add(sym.GetName(), sym);
		}

		public override string ToString()
		{
			string s = "TABLE\n";

			foreach (var x in table)
			{
				s += x.Value.ToString();
			}

			return s + '\n';
		}
	}

	class VarTable : SymTable
	{
	}

	class TypeTable: SymTable
	{
	}

	class Symbol
	{
		protected string name;

		public Symbol(string name)
		{
			this.name = name;
		}

		public Symbol()
		{
			this.name = "";
		}

		public void SetName(string s)
		{
			name = s;
		}

		public string GetName()
		{
			return name;
		}

		public override string ToString()
		{
			return this.name;
		}
	}

#region Variable

	class SymVar : Symbol
	{
		SymType type;
		SynExpr value;

		public SymVar(string s): base(s) { }
		public SymVar(): base() { }

		public void SetType(SymType type)
		{
			this.type = type;
		}

		public void SetInitValue(SynExpr val)
		{
			value = val;
		}

		public SymType GetType()
		{
			return this.type;
		}

		public override string ToString()
		{
			return this.name + "   " + this.type.ToString() + "   ";
		}
	}

	class SymFunc : SymVar
	{
	}

	class SymVarParam : SymVar
	{
	}

	class SymVarConst : SymVar
	{
	}

	class SymVarLocal : SymVar
	{
	}

	class SymVarGlobal : SymVar
	{
	}

#endregion

#region Types

	class SymType : Symbol
	{
	}

	class SymTypeScalar : SymType
	{
	}

	class SymTypeVoid : SymTypeScalar
	{
		public SymTypeVoid()
		{
			this.name = "VOID";
		}
	}

	class SymTypeDouble : SymTypeScalar
	{
		public SymTypeDouble()
		{
			this.name = "DOUBLE";
		}
	}

	class SymTypeChar : SymTypeScalar
	{
		public SymTypeChar()
		{
			this.name = "CHAR";
		}
	}

	class SymTypeInt : SymTypeScalar
	{
		public SymTypeInt()
		{
			this.name = "INT";
		}
	}

	class SymTypeArray : SymType
	{
		SymType type;
		SynExpr size;
		public SymTypeArray(SymType t)
		{
			this.type = t;
		}

		public void SetSize(SynExpr size)
		{
			this.size = size;
		}

		public override string ToString()
		{
			return "ARRAY OF " + type.ToString(); 
		}
	}

	class SymTypeFunc : SymType
	{
		SymType type;
		List<SymVar> args = new List<SymVar>();

		public SymTypeFunc(SymType t)
		{
			this.type = t;
		}

		public void SetParam(SymVar p)
		{
			this.args.Add(p);
		}
	}

	class SymTypeEnum : SymType
	{
		System.Collections.Hashtable enumerators = new System.Collections.Hashtable();

		public void AddEnumerator()
		{
		}
	}

	class SymTypeStruct : SymType
	{
		SymTable fields;

		public void SetItems(SymTable table)
		{
			fields = table;
		}
	}

	class SymTypeAlias : SymType
	{
		SymType type;
		public void SetType(SymType type)
		{
			this.type = type;
		}
	}

	class SymTypePointer : SymType
	{
		SymType type;
		public SymTypePointer(SymType t)
		{
			type = t;
		}

		public override string ToString()
		{
			return "POINTER TO " + type.ToString();
		}
	}

#endregion
}
