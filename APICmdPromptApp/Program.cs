using System;
using static Helpers.Custom;

namespace APICmdPromptApp
{
	internal class Program
	{
		/* Initial iteration to be the console app. Re-create as a WPF if that interface is reached. Until then, reach level 3 
		within the context of the console application and command line tools. Ensure encapsulation and easy extensibility of functionality
		to a GUI, non-coupling, etc. Higher levels must rely only on an interface, nothing else */

		private static void Main(string[] args)
		{
			try
			{
				AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
				Entry.Start();
			}
			catch (AggregateException e)
			{
				if (e.Message.Contains("No such host is known"))
					print("ERROR (SocketException). Please check your Internet connection.");
				else
					throw;
			}
			catch (Exception e)
			{
				print("Manually caught exception: " + e);
			}
			finally
			{
				Entry.HandleExitKeys();
			}
		}

		private static void OnProcessExit(object sender, EventArgs e)
		{
			Entry.HandleExitKeys();
		}
	}
}