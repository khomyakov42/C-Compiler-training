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
				Compilation("../../test.txt");
			}
			else
				switch (args[0])
				{
					case "-c":
						Compilation(args[1]);
						break;
					default:
						Console.WriteLine(header);
						return;
				}
      }

		static void Compilation(string filename){
			FileStream file = new FileStream(@filename, FileMode.Open, FileAccess.Read);
			Scaner scaner = new Scaner(file);
			Parser parser = new Parser(scaner);
			try
			{
				parser.Parse();
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
		}
   }
}