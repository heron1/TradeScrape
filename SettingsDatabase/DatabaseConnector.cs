using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using AppSettings;
using Helpers;

//TODO Completely refactor this, the API should only be using an interface to the database, not a direct instantiation or reliance
//upon its implementation
//The supported platforms etc should also be pulled from a base dependent source file such as a JSON listing the supported platforms,
//this same file should also provide the same information to the Level 1 APIs etc.
namespace SettingsDatabase
{
	public class DatabaseConnector : IAppSettings
	{
		private const int _version = 1; //update version only if breaking changes to database are made
		private readonly List<string> _supportedPlatforms;
		private SQLiteConnection m_dbConnection;

		public DatabaseConnector()
		{
			CreateOrAccessDatabase();
			
			_supportedPlatforms = GetSupportedPlatforms();
			InitializeDatabase();
		}

		private void CreateOrAccessDatabase()
		{
			Custom.SwitchToAppDataFolder(Settings.GetAppDataFolderName());
			m_dbConnection = new SQLiteConnection("Data Source=AppDatabase.sqlite;Version=3;Pooling=False");
			m_dbConnection.Open();
		}

		private void InitializeDatabase()
		{
			if (!EnsureDatabaseIntegrity())
				ReCreateDatabase();
		}


		public List<string> GetSupportedPlatforms()
		{
			return Settings.GetSupportedPlatforms();
		}

		public (bool success, string apiKey, string secretKey, string passphrase) GetCredentials(string platform)
		{
			var success = false;
			var apiKey = "";
			var secretKey = "";
			var passphrase = "";

			var query = $"SELECT * FROM credentials WHERE platform = '{platform}'";
			var reader = ExecuteReader(query);
			while (reader.Read())
			{
				apiKey = reader["apiKey"].ToString();
				secretKey = reader["secretKey"].ToString();
				passphrase = reader["passphrase"].ToString();
			}

			if (reader.StepCount != 1)
				return (false, "", "", "");

			if (apiKey.Length == 0 || secretKey.Length == 0)
				return (false, "", "", "");

			return (true, apiKey, secretKey, passphrase);
		}

		public (bool success, string platform, string apiKey, string secretKey, string passphrase) GetDefaultCredentials()
			//returns true if credentials sucessfullly retrieved, along with the credentials cols
		{
			var platform = "";
			var apiKey = "";
			var secretKey = "";
			var passphrase = "";

			var query = "SELECT default_platform FROM meta";
			var defaultPlatform = "";
			var reader = ExecuteReader(query);
			while (reader.Read()) defaultPlatform = reader["default_platform"].ToString();
			if (reader.StepCount != 1)
				return (false, "", "", "", "");

			if (defaultPlatform.Length == 0) return (false, "", "", "", "");

			query = $"SELECT * FROM credentials WHERE platform = '{defaultPlatform}'";
			reader = ExecuteReader(query);
			while (reader.Read())
			{
				platform = reader["platform"].ToString();
				apiKey = reader["apiKey"].ToString();
				secretKey = reader["secretKey"].ToString();
				passphrase = reader["passphrase"].ToString();
			}

			if (reader.StepCount != 1)
				return (false, "", "", "", "");

			if (platform.Length == 0 || apiKey.Length == 0 || secretKey.Length == 0)
				return (false, "", "", "", "");

			return (true, platform, apiKey, secretKey, passphrase);
		}

		public bool SetCredentials(string platform, string apiKey, string secretKey,
			string passphrase) //returns true if successful
		{
			var query =
				$"UPDATE credentials SET apiKey = '{apiKey}', secretKey = '{secretKey}', passphrase = '{passphrase}' " +
				$"WHERE platform = '{platform}'";
			return ExecuteNonQuery(query) == 1;
		}

		public bool SetDefaultPlatform(string platform) //returns true if successful
		{
			var query = $"UPDATE meta SET default_platform = '{platform}';";
			return ExecuteNonQuery(query) == 1;
		}

		public List<(string platform, string apiKey, string secretKey, string passphrase)> GetAllKeys()
		{
			var platformKeys = new List<(string platform, string apiKey, string secretKey, string passphrase)>();
			var query = "SELECT * FROM credentials";
			var reader = ExecuteReader(query);
			while (reader.Read())
				platformKeys.Add((reader["platform"].ToString(), reader["apiKey"].ToString(), reader["secretKey"].ToString(),
					reader["passphrase"].ToString()));

			return platformKeys;
		}

		public bool SetRemoveKeysOnExit(bool value) //returns true if successful
		{
			string query;
			if (value)
				query = "UPDATE meta SET remove_keys_on_exit = 1;";
			else
				query = "UPDATE meta SET remove_keys_on_exit = 0;";

			if (ExecuteNonQuery(query) == 0)
				return true;
			else
				return false;
		}

		public bool GetRemoveKeysOnExitValue()
		{
			var query = "SELECT remove_keys_on_exit FROM meta;";
			var reader = ExecuteReader(query);
			reader.Read();
			var val = Convert.ToInt32(reader["remove_keys_on_exit"].ToString());

			if (val == 1)
				return true;
			else if (val == 0)
				return false;
			else
				throw new Exception("Database corrupted. Invalid value in remove_keys_on_exit");
		}

		public void ClearAllPlatformKeys()
		{
			var query = "UPDATE credentials SET apiKey = null, secretKey = null;";
			ExecuteNonQuery(query);
		}

		private bool EnsureDatabaseIntegrity() //returns true if integrity test passed, adds missing platforms
		{
			//Check all tables exist
			var reader = ExecuteReader("SELECT name FROM sqlite_master WHERE type='table' AND name='credentials';");
			if (!reader.HasRows)
				return false;

			reader = ExecuteReader("SELECT name FROM sqlite_master WHERE type='table' AND name='meta';");
			if (!reader.HasRows)
				return false;

			//Ensure correct version
			reader = ExecuteReader("SELECT version FROM meta");
			while (reader.Read())
				try
				{
					if (Convert.ToInt32(reader["version"].ToString()) != _version)
						return false;
				}
				catch (FormatException)
				{
					return false;
				}

			if (reader.StepCount != 1)
				return false;

			//Check tables have all required rows
			if (!InsertMissingPlatforms())
				return false;

			return true;
		}

	

		private bool InsertMissingPlatforms() //returns true if operation successful, false otherwise
		{
			var foundPlatforms = new List<string>();

			var reader = ExecuteReader("SELECT * FROM credentials;");
			if (reader.FieldCount != 4)
				return false;
			while (reader.Read()) foundPlatforms.Add(reader["platform"].ToString());
			foreach (var platform in _supportedPlatforms)
				if (!foundPlatforms.Contains(platform))
				{
					var query = $"INSERT INTO credentials VALUES ('{platform}', null, null, null)";
					if (ExecuteNonQuery(query) != 1)
						return false;
				}

			return true;
		}

		private bool ReCreateDatabase() //returns true if database successfully recreated
		{
			ResetDatabase();

			//Create the tables
			var query = @"CREATE TABLE credentials
				(
					platform TEXT
						constraint credentials_pk
							primary key,
					apiKey TEXT,
					secretKey TEXT
				, passphrase TEXT)";
			ExecuteNonQuery(query);

			query = @"CREATE TABLE meta
				(
					default_platform TEXT default null,
					remove_keys_on_exit INTEGER default 0
				, version int)";
			ExecuteNonQuery(query);

			//Insert the platforms
			InsertMissingPlatforms();

			query = $"INSERT INTO meta VALUES (null, 0, {_version});";
			ExecuteNonQuery(query);

			if (!EnsureDatabaseIntegrity())
				return false;

			return true;
		}

		private void ResetDatabase()
		{
			m_dbConnection.Close();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			File.Delete("AppDatabase.sqlite");
			CreateOrAccessDatabase();
		}

		private SQLiteDataReader ExecuteReader(string query)
		{
			var command = new SQLiteCommand(query, m_dbConnection);
			return command.ExecuteReader(); //returns the reader object
		}

		private int ExecuteNonQuery(string query)
		{
			var command = new SQLiteCommand(query, m_dbConnection);
			return command.ExecuteNonQuery(); //returns int rows affected.
		}
		
	}
}