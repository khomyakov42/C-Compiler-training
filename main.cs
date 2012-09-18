using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Compiler{
	class Program
   {
      static void Main(string[] args)
      {
			StreamReader istr = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
			StreamWriter ostr = new StreamWriter(System.Console.OpenStandardOutput(), Console.OutputEncoding);

			if (args.Count() < 2)
			{
				Help();
				return;
			}

			bool low_opt = false, hight_opt = false;
			int index_in = 2, index_out = 3;

			switch (args[1])
			{
				case "-low":
					low_opt = true;
					break;
				case "-hight":
					hight_opt = true;
					break;
				case "-low-hight":
					low_opt = hight_opt = true;
					break;
				default:
					index_in = 1;
					index_out = 2;
					break;
			}

			istr = new StreamReader(args[index_in]);
			if (index_out < args.Count())
			{
				ostr = new StreamWriter(args[index_out]);
			}

			Scaner scaner = null;
			Parser parser = null;
			Fasm.CodeGen codegen = null;
			switch (args[0])
			{
				case "-l":
					scaner = new Scaner(istr);
					Token t = null;
					while (t == null || t.type != Token.Type.EOF)
					{
						try
						{
							t = scaner.Read();
							Console.WriteLine(t.ToString());
						}
						catch (Scaner.Exception e)
						{
							ostr.WriteLine(e.Message);
						}
					}
					break;
				case "-p":
					parser = new Parser(new Scaner(istr));
					parser.Parse();
					parser.PrintTree(ostr);
					ostr.WriteLine(parser.logger.ToString());
					break;
				case "-c":
					parser = new Parser(new Scaner(istr));
					parser.Parse();
					if (!parser.logger.isEmpty())
					{
						ostr.WriteLine(parser.logger.ToString());
						break;
					}
					codegen = new Fasm.CodeGen(parser.tstack);
					codegen.Generate(ostr);
					break;
				case "-cexe":
					parser = new Parser(new Scaner(istr));
					parser.Parse();
					if (!parser.logger.isEmpty())
					{
						ostr.WriteLine(parser.logger.ToString());
						break;
					}
					codegen = new Fasm.CodeGen(parser.tstack);
					codegen.Generate(ostr);
					ostr.Flush();
					ostr.Close();
					if (index_out < args.Count())
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = "C:/fasm/fasm.exe",
							WindowStyle = ProcessWindowStyle.Hidden,
							Arguments = string.Format("{0} {1}", args[index_out], args[index_out])
						});
					}
					break;
				default:
					Help();
					return;
			}

			istr.Close();
			ostr.Close();
      }

		static void Help()
		{
			Console.WriteLine("-----------------------------------------------------------------------------");
			Console.WriteLine("Compiler supposed to compile C language with some restrictions:");
			Console.WriteLine("1) not supported preprocessor");
			Console.WriteLine("2) not supported type float");
			Console.WriteLine("-----------------------------------------------------------------------------");
			Console.WriteLine();
			Console.WriteLine("-----------------------------------------------------------------------------");
			Console.WriteLine("how to used:");
			Console.WriteLine(">compiler mode [optimize] input_file [output_file]");
			Console.WriteLine("mode:");
			Console.WriteLine("\t-l\tdo lexical  analysis; print lexem");
			Console.WriteLine("\t-p\tparse; print syntax trees, vars, types");
			Console.WriteLine("\t-c\tgenerate asm code");
			Console.WriteLine("\t-cexe\tcompile exe file");
			Console.WriteLine();
			Console.WriteLine("optimize:");
			Console.WriteLine("\t-low\toptimize asm code");
			Console.WriteLine("\t-hight\toptimize syntax trees");
			Console.WriteLine("\t-low-height\t optimize syntax trees and asm code");
			Console.WriteLine("input_file:");
			Console.WriteLine("\t\tfile contains sourse code C language");
			Console.WriteLine("output_file:");
			Console.WriteLine("\t\tif no path specified, the program outputs to stdout");
			Console.WriteLine("-----------------------------------------------------------------------------");

		}
   }
}