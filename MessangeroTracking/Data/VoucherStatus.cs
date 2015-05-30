using System;
using System.Collections.Generic;
using System.Data;
using System.Json;
using System.Linq;
using Mono.Data.Sqlite;

namespace MessangeroTracking
{
	public class VoucherStatus
	{
		public decimal VoucherStatusID{ get; set;}
		public decimal VoucherID{ get; set;}
		public string Status{get;set;}
		public string Area{get;set;}
		public string Date{get;set;}
		public string Time {get;set;}
		public bool IsNew{ get; set;}
		public DateTime LastUpdated {get;set;}

		public VoucherStatus ()
		{
			IsNew = true;
		}

		public static List<VoucherStatus> LoadStatuses(JsonArray json )
		{
			var results = (from result in json
				let jResult = result as JsonObject
				select new VoucherStatus(){VoucherStatusID = (decimal)jResult ["StatusID"], 
					VoucherID = (decimal)jResult ["VoucherID"],
					//Status = jResult["Status"] == null ? "" : jResult ["Status"].ToString().TrimStart('\"').TrimEnd('\"'),
					Status = jResult["Description"] == null ? "" : jResult ["Description"].ToString().TrimStart('\"').TrimEnd('\"'),
					Area = jResult["Area"] == null ? "" : jResult ["Area"].ToString().TrimStart('\"').TrimEnd('\"'),
					Date = jResult["Date"] == null ? "" : jResult ["Date"].ToString().TrimStart('\"').TrimEnd('\"'),
					Time = jResult["Time"] == null ? "" : jResult ["Time"].ToString().TrimStart('\"').TrimEnd('\"'),
					LastUpdated = DateTime.Parse(jResult["LastUpdated"])
				}).ToArray ();

			return new List<VoucherStatus>(results);
		}

		public void Load(SqliteDataReader dr)
		{
			if (dr.GetValue (0) == DBNull.Value)
				return;

			VoucherStatusID = dr.GetDecimal(0);
			VoucherID = dr.GetDecimal(1);

			object objStatus = dr.GetValue (2);
			if(objStatus != DBNull.Value)
				Status = (string)objStatus;

			object objArea = dr.GetValue (3);
			if(objArea != DBNull.Value)
				Area = (string)objArea;

			object objDate = dr.GetValue (4);
			if(objDate != DBNull.Value)
				Date = (string)objDate;

			object objTime = dr.GetValue (5);
			if(objTime != DBNull.Value)
				Time = (string)objTime;

			IsNew = false;
		}

		public void Save()
		{
			if (IsNew)
				Insert ();
			else
				Update ();
		}

		private void Insert()
		{
			lock (typeof(VoucherStatus))
			{
			
				using (SqliteConnection cn = DBManager.CreateConnection ())
				{
					using (var cm = cn.CreateCommand ())
					{
						if (PreferencesUtil.IsPremium == false)
						{
							cm.CommandText = @"SELECT MAX(VoucherStatusID) FROM VoucherStatuses";
							object val = cm.ExecuteScalar ();
							if (val == DBNull.Value || val == null)
								VoucherStatusID = 1;
							else
								VoucherStatusID = Convert.ToInt32 (val) + 1;
						}

						cm.CommandText = 
						@"INSERT INTO [VoucherStatuses] 
					([VoucherStatusID], [VoucherID], [Status], [Area],[Date], [Time], [LastUpdated]) 
						VALUES (@VoucherStatusID,@VoucherID,@Status,@Area,@Date, @Time, @LastUpdated)";
						cm.Parameters.AddWithValue ("@VoucherStatusID", VoucherStatusID);
						cm.Parameters.AddWithValue ("@VoucherID", VoucherID);
						cm.Parameters.AddWithValue ("@Status", Status);
						cm.Parameters.AddWithValue ("@Area", Area);
						cm.Parameters.AddWithValue ("@Date", Date);
						cm.Parameters.AddWithValue ("@Time", Time);
						cm.Parameters.AddWithValue ("@LastUpdated", LastUpdated);

						cm.ExecuteNonQuery ();

					}
					cn.Close ();
				}
			}
		}

		public void InsertIfNotExist()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = 
						@"INSERT INTO [VoucherStatuses] 
					([VoucherStatusID], [VoucherID], [Status], [Area],[Date], [Time], [LastUpdated]) 
						SELECT @VoucherStatusID, @VoucherID,@Status,@Area,@Date, @Time, @LastUpdated 
						WHERE NOT EXISTS(SELECT * FROM VoucherStatuses WHERE [VoucherStatusID] = @VoucherStatusID)";
					cm.Parameters.AddWithValue("@VoucherStatusID", VoucherStatusID);
					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.Parameters.AddWithValue("@Status",Status);
					cm.Parameters.AddWithValue("@Area",Area);
					cm.Parameters.AddWithValue("@Date",Date);
					cm.Parameters.AddWithValue("@Time",Time);
					cm.Parameters.AddWithValue("@LastUpdated",LastUpdated);

					cm.ExecuteNonQuery ();
				}
				cn.Close();
			}
		}

		private void Update()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"UPDATE [VoucherStatuses] 
					SET [VoucherID] = @VoucherID ,
						[Status] = @Status, 
						[Area] = @Area, 
						[Date] = @Date, 
						[Time] = @Time,
						[LastUpdated] = @LastUpdated) 
						WHERE VoucherStatusID = @VoucherStatusID";
					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.Parameters.AddWithValue("@Area",Area);
					cm.Parameters.AddWithValue("@Date",Date);
					cm.Parameters.AddWithValue("@Time",Time);
					cm.Parameters.AddWithValue("@LastUpdated",LastUpdated);
					cm.Parameters.AddWithValue("@VoucherStatusID",VoucherStatusID);

					cm.ExecuteNonQuery ();

				}
				cn.Close();
			}
		}


	}
}

