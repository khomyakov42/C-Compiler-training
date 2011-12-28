using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler{
	class Program
   {
		const string header = "Compiler by Homyakov Sergey, FEFU School of Natural Sciences, b8303a, 2011";

      static void Main(string[] args)
      {
			if (args.Count() == 0)
			{
				Console.WriteLine(header);
				Compilation("../../test.txt", "../../test.out.txt");
			}
			else
				switch (args[0])
				{
					case "-c":
						//Compilation(args[1]);
						break;
					default:
						Console.WriteLine(header);
						return;
				}
      }

		static void Compilation(string input_path, string write_path){
			FileStream file = new FileStream(@input_path, FileMode.Open, FileAccess.Read);
			StreamWriter fout = new StreamWriter(@write_path, false);
			Scaner scaner = new Scaner(file);
			Parser parser = new Parser(scaner);
			CodeGen generator = new CodeGen(fout, parser);
			try
			{
				//parser.Parse();
				generator.Generate();
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
		}
   }
}