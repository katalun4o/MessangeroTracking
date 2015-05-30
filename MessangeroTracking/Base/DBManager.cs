using System;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;

namespace MessangeroTracking
{
	public class DBManager
	{
		const string dbName = "MessangeroTracking.db3";		
		static string dbPath = Path.Combine (
        Environment.GetFolderPath (Environment.SpecialFolder.Personal),
			dbName);
		
		public DBManager ()
		{
		}

		public static void CreateDatabase()
		{
			if (System.IO.File.Exists (dbPath))
				return;
				
			SqliteConnection.CreateFile (dbPath);

			using (SqliteConnection cn = CreateConnection ()) 
			{
				using (var command = cn.CreateCommand ()) 
				{
					command.CommandText = "CREATE TABLE IF NOT EXISTS [Providers] ([ProviderID] INTEGER , [Name] VARCHAR(64), [Color] VARCHAR(16));";
					command.ExecuteNonQuery ();

					command.CommandText = "CREATE TABLE IF NOT EXISTS [Vouchers] ([VoucherID] NUMERIC(18,0) PRIMARY KEY, [Name] VARCHAR(64), [Description] VARCHAR(246), [Number] VARCHAR(64), [ProviderID] INTEGER, [UserID] int, [IsDelivered] BIT,  [LastUpdated] DateTime);";
					command.ExecuteNonQuery ();

					command.CommandText = "CREATE TABLE IF NOT EXISTS [VoucherStatuses] ([VoucherStatusID] NUMERIC(18,0) PRIMARY KEY,[VoucherID] INTEGER, [Status] VARCHAR(512), [Area] VARCHAR(256), [Date] VARCHAR(64), [Time] VARCHAR(64), [LastUpdated] DateTime );";
					command.ExecuteNonQuery ();
				}

				cn.Close ();
			}
	
		}

		public static SqliteConnection CreateConnection()
		{
			var connection = new SqliteConnection ("Data Source=" + dbPath);
			connection.Open();

			return connection;
		}

		public static DateTime? GetLastDate()
		{
			DateTime? dateResult = null;
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"SELECT MIN(maxDate)
FROM (
SELECT MIN([Vouchers].[LastUpdated]) maxDate
FROM [Vouchers]
WHERE [Vouchers].[UserID] = @UserID
UNION 
SELECT MIN([VoucherStatuses].[LastUpdated]) maxDate
FROM [VoucherStatuses]
WHERE [VoucherStatuses].[VoucherID] IN (SELECT VoucherID FROM Vouchers WHERE Vouchers.UserID = @UserID)
) as sub
 ";
					cm.Parameters.AddWithValue ("@UserID",PreferencesUtil.UserID);

					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						if (dr.Read ()) 
						{
							object val = dr.GetValue (0);
							if (val == DBNull.Value)
								dateResult = null;
							else
								dateResult = dr.GetDateTime(0);
							if (dateResult == DateTime.MinValue)
								dateResult = null;
						}
						dr.Close ();
					}
				}
				cn.Close();
			}

			return dateResult;
		}
	}
}

