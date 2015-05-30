using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mono.Data.Sqlite;

namespace MessangeroTracking
{
	public class Provider
	{
		public bool IsDefault {get;set;}
		public int ProviderID{get;set;}
		public string Name{get;set;}
		public string Color{get;set;}
		public bool IsActive{get;set;}
		public string Logo{get;set;}

		public Provider()
		{
			IsDefault = false;
		}

		public string GetImagePath()
		{
			String dir = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath,".messangeroTracking");
			if (!System.IO.Directory.Exists(dir))
			{
				System.IO.Directory.CreateDirectory(dir);
			}
			String filePath = System.IO.Path.Combine(dir, ProviderID.ToString()+".jpg");
			return filePath;
		}

		public static List<Provider> GetProviders()
		{
			List<Provider> res = new List<Provider> ();

			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"SELECT [ProviderID], [Name], [Color] FROM  [Providers]";

					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						while (dr.Read ()) 
						{
							Provider p = new Provider ();
							p.Load(dr);
							res.Add(p);
						}
						dr.Close ();
					}
				}
				cn.Close();
			}

			return res;
		}

		private void Load(SqliteDataReader dr)
		{
			ProviderID = dr.GetInt32(0);

			object objName = dr.GetValue (1);
			if(objName != DBNull.Value)
				Name = (string)objName;

			object objColor  = dr.GetValue (2);
			if(objColor != DBNull.Value)
				Color = (string)objColor;
		}

		public void Insert()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"INSERT INTO [Providers] ([ProviderID],[Name], [Color]) 
										VALUES (@ProviderID,@Name,@Color)";

					cm.Parameters.AddWithValue("@ProviderID",ProviderID);
					cm.Parameters.AddWithValue("@Name",Name);
					cm.Parameters.AddWithValue("@Color",Color);

					cm.ExecuteNonQuery ();

				}
				cn.Close();
			}
		}

		public static void DeleteAll()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"DELETE FROM [Providers]";
					cm.ExecuteNonQuery ();
				}
				cn.Close();
			}
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

