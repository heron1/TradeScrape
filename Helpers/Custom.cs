using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Helpers
{
	public static class Custom
	{
		/// <summary>
		///   Usage: TimeProfiler(() => func(args);
		/// </summary>
		/// <param name="func(args)">Desired method to time along with its arguments</param>
		/// <returns>(long) time in milliseconds</returns>
		public static long TimeProfiler<T>(Func<T> func)
		{
			var stopwatch = Stopwatch.StartNew();
			func();
			stopwatch.Stop();
			var ms = stopwatch.ElapsedMilliseconds;
			return ms;
		}

		/// <summary>
		///   NOT FINISHED. Directly print the elements of some IEnumerable collection to the console
		/// </summary>
		/// <param name="obj">The IEnumerable implemented collection</param>
		public static void PrintCollection(IEnumerable obj)
		{
			//TODO: Allow generic type or object that can determine its most specified type for enumeration
			foreach (var i in obj)
				Console.WriteLine(i);
		}

		

		//TODO: Implement deep copy method? It can use an interchangeable abstracted library but this method remains the same 

		/// <summary>
		///   Shortcut alias for printing to the desired location. Edit depending upon project.
		/// </summary>
		/// <param name="s"></param>
		public static void print(object s)
		{
			Console.WriteLine(s.ToString());
		}

		/// <summary>
		/// Logs an exception to a log.txt file in the program directory
		/// </summary>
		/// <param name="e"></param>
		/// <param name="rethrowException"></param>
		public static void LogException(Exception e)
		{
			//Retrieves the directory of the program executable
			string filePath = GetFilenameApplicationPath("log.txt");

			//Appends Exception information to the log file. If the log file doesn't exist, creates it
			using (StreamWriter logFile = File.AppendText(filePath))
			{
				string preString = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				logFile.WriteLine(preString);
				logFile.WriteLine("================");
				logFile.WriteLine("Exception: " + e);
				logFile.WriteLine("Inner Exception: " + e.InnerException);
				logFile.WriteLine("======= END =========");
			}
		}

		public static string GetFilenameApplicationPath(string filename)
		{
			string projectPath = AppDomain.CurrentDomain.BaseDirectory;
			return Path.Combine(projectPath, filename);
		}

		public static void SwitchToAppDataFolder(string folderName)
		{
			string appdataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
			
			if (!Directory.Exists(appdataFolder))
				Directory.CreateDirectory(appdataFolder);
			
			Directory.SetCurrentDirectory(appdataFolder);
		}
		
	}
}