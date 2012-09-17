using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler{
	class Program
   {
		public static string PATH_TO_MASM = System.IO.Path.GetFullPath("C:/Compiler/masm");
		public static string PATH_TO_MASM_LIB = System.IO.Path.GetFullPath("C:/masm32/lib");
		public static string PATH_TO_MASM_LINCLUDE = System.IO.Path.GetFullPath("C:/masm32/include");

      static void Main(string[] args)
      {
			StreamReader istr = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
			StreamWriter ostr = new StreamWriter(System.Console.OpenStandardOutput(), Console.OutputEncoding);

			if (args.Count() == 2 || args.Count() == 3)
			{
				istr = new StreamReader(args[1]);
				if (args.Count() == 3)
				{
					ostr = new StreamWriter(args[2]);
				}

				switch (args[0])
				{
					case "-c":
						Compilation(istr, ostr);
						break;
				}
			}
			Help();
			istr.Close();
			ostr.Close();
      }

		static void Help()
		{
			Console.Write("");
		}

		static void Compilation(StreamReader istream, StreamWriter ostream)
		{
			Scaner scaner = new Scaner(istream);
			Parser parser = new Parser(scaner);

			try
			{
				parser.Parse();
				parser.PrintTree(ostream);
				ostream.WriteLine(parser.logger.ToString());
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
		}
   }
}