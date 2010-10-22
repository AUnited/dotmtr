/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Test.CommandLineParsing;

namespace dotMTR
{
	/// <summary>
	/// TODO
	/// Command line arguments
	/// </summary>
	class CmdLineArgs
	{
		public string traceDest
		{
			get;
			set;
		}

		/// <summary>
		/// Process command line arguments
		/// </summary>
		/// <param name="_args"></param>
		public CmdLineArgs(string[] _args)
		{
			String[] args = _args;
			for (int i = 0; args.Length > i; i++)
			{
				args[i] = args[i].ToUpper();
			}

			CommandLineDictionary cld = CommandLineDictionary.FromArguments(args);

			if (cld.ContainsKey("TRACEDEST")) this.traceDest = cld["TRACEDEST"];
		}


		/// <summary>
		/// Show help message
		/// </summary>
		void ShowHelp()
		{
			string helpMessage = "dotMTR - TODO";

			if (Environment.UserInteractive) MessageBox.Show(helpMessage);
			Console.WriteLine(helpMessage);
		}
	}
}
