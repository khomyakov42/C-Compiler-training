using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler{
	class FatalException : Exception
	{
		public FatalException() : base("FATAL ERROR") { }
	}

	class Program
   {
		public static string PATH_TO_MASM = System.IO.Path.GetFullPath("C:/Users/Admin/Documents/Documents/Compiler/Compiler/masm");
		public static string PATH_TO_MASM_LIB = System.IO.Path.GetFullPath("C:/masm32/lib");
		public static string PATH_TO_MASM_LINCLUDE = System.IO.Path.GetFullPath("C:/masm32/include");

      static void Main(string[] args)
      {
			if (args.Length == 0)
			{
				Console.WriteLine("Run -h for usage help");
				return;
			}

			switch (args[0])
			{
				case "-c":
					if (args.Length == 1) { break; }
					string result_path;
					if (args.Length == 3)
					{
						result_path = args[2];
					}
					else
					{
						result_path = System.IO.Path.Combine(
							System.IO.Path.GetDirectoryName(args[1]),
							System.IO.Path.GetFileNameWithoutExtension(args[1]) + ".asm"
						);
					}
					Compilation(args[1], result_path);
			//		CompileEXE(result_path);
					break;
				case "-h":
				default:
					Help();
					break;
			}
      }

		static void CompileEXE(string asmf)
		{
			string filename = System.IO.Path.GetFileNameWithoutExtension(asmf);
			string fullfilename = System.IO.Path.GetFileName(asmf);
			var masm_info = new System.Diagnostics.ProcessStartInfo
			{
				FileName = System.IO.Path.Combine(Program.PATH_TO_MASM, "buildc.bat"),
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				Arguments = System.IO.Path.Combine(Program.PATH_TO_MASM, filename) + " " + Program.PATH_TO_MASM
			};


			if (System.IO.File.Exists(System.IO.Path.Combine(PATH_TO_MASM, fullfilename)))
			{
				System.IO.File.Delete(System.IO.Path.Combine(PATH_TO_MASM, fullfilename));
			}

			System.IO.File.Move(asmf, System.IO.Path.Combine(PATH_TO_MASM, fullfilename));
			System.Diagnostics.Process.Start(masm_info).WaitForExit();
			if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asmf), filename + ".exe")))
			{
				System.IO.File.Delete(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asmf), filename + ".exe"));
			}
			System.IO.File.Move(System.IO.Path.Combine(PATH_TO_MASM, filename + ".exe"), System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asmf), filename + ".exe"));
			System.Diagnostics.Process.Start(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asmf), filename + ".exe"));
		}

		static void Help()
		{
			Console.Write("");
		}

		static void Compilation(string input_file, string output_file){
			FileStream inf = new FileStream(@input_file, FileMode.Open, FileAccess.Read);
			StreamWriter outf = new StreamWriter(@output_file);

			Scaner scaner = new Scaner(inf);
			Parser parser = new Parser(scaner);
			CodeGen generator = new CodeGen(outf, parser);

			try
			{
				generator.Generate();
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}

			outf.Close();
			inf.Close();
		}
   }
}