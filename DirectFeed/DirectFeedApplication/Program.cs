// Copyright © 2007 Triamec Motion AG

using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Triamec.Tam.Samples.UI {
	static class Program {
		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try {
#pragma warning disable CA2000 // Dispose objects before losing scope: Application.Run disposes form
				Application.Run(new DirectFeedForm(args.Length > 0 ? args[0] : null));
#pragma warning restore CA2000 // Dispose objects before losing scope
			} catch (TargetInvocationException ex) {

				var originalEx = ex.InnerException;
				originalEx = ex.InnerException.InnerException ?? originalEx;

				// may bubble up from DirectFeed.Execute (when a move profile was passed with args)
				// Unwrap two target invocation exceptions, interpret data dictionary.
				var builder = new StringBuilder(originalEx.FullMessage());
				foreach (var item in originalEx.Data.Values) {
					builder.AppendLine();
					var dataEx = item as Exception;
					builder.Append(dataEx == null ? item.ToString() : dataEx.FullMessage());
				}
				MessageBox.Show(builder.ToString(), "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1, 0);
			}
		}
		#endregion Main
	}
}
