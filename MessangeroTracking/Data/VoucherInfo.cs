using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace MessangeroTracking
{
	public class VoucherInfo : Java.Lang.Object
	{
		public int VoucherID { get; set; }
		public string Name { get; set; }
		public string Number { get; set; }
		public string Description { get; set; }
		public List<VoucherStatus> Statuses{ get; set;}
		public bool IsNew{ get; set;}
		public int ProviderID{ get; set;}
		public int UserID{ get; set;}
		public DateTime LastUpdated{ get; set;}
		public string ProviderName{ get; set;}
		public string ProviderColor{ get; set;}

		public string LastStatus{ get; set;}
		public string LastStatusDate{ get; set;}
		public string LastStatusTime{ get; set;}
		public string LastStatusArea{ get; set;}

		public bool IsDelivered { get; set;}

		private bool _isSelected = false;
		public bool IsSelected
		{ 
			get
			{ 
				return _isSelected; 
			} 
			set
			{ 
				_isSelected = value; 
			} 
		}

		public VoucherInfo ()
		{
			IsNew = true;
			UserID = PreferencesUtil.UserID;
			this.Statuses = new List<VoucherStatus> ();
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

		public void Load(int voucherID)
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"SELECT [Vouchers].[VoucherID], 
[Vouchers].[Name], 
[Vouchers].[Description], 
[Vouchers].[Number], 
[Vouchers].[ProviderID],
[Providers].[Name],
[Providers].[Color],
[VoucherStatuses].[Status] ,
[VoucherStatuses].[Area] ,
[VoucherStatuses].[Date] ,
[VoucherStatuses].[Time] ,
[Vouchers].[IsDelivered]
									 FROM [Vouchers] 
									JOIN Providers ON Providers.ProviderID = Vouchers.ProviderID
LEFT JOIN VoucherStatuses ON VoucherStatuses.VoucherStatusID = (SELECT MAX(VoucherStatusID) FROM VoucherStatuses WHERE VoucherStatuses.VoucherID = [Vouchers].VoucherID)
									WHERE [Vouchers].VoucherID = @VoucherID";
					cm.Parameters.AddWithValue ("@VoucherID", voucherID);

					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						while (dr.Read ()) 
						{
							Load (dr);
						}
						dr.Close ();
					}

					cm.CommandText = @"SELECT [VoucherStatusID], [VoucherID], [Status], [Area],[Date], [Time]
									 FROM [VoucherStatuses] WHERE VoucherID = @VoucherID 
										ORDER BY VoucherStatusID DESC";
					cm.Parameters.Clear ();
					cm.Parameters.AddWithValue ("@VoucherID", voucherID);

					this.Statuses.Clear ();
					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						while (dr.Read ()) 
						{
							VoucherStatus status = new VoucherStatus ();
							status.Load (dr);
							if(status.VoucherStatusID != 0)
								this.Statuses.Add (status);
						}
						dr.Close ();
					}
				}
				cn.Close();
			}
		}

		public void Load(SqliteDataReader dr)
		{
			VoucherID = dr.GetInt32(0);

			object objName = dr.GetValue (1);
			if(objName != DBNull.Value)
				Name = (string)objName;

			object objDescription = dr.GetValue (2);
			if(objDescription != DBNull.Value)
				Description = (string)objDescription;

			object objNumber = dr.GetValue (3);
			if(objNumber != DBNull.Value)
				Number = (string)objNumber;

			object objProviderID = dr.GetValue (4);
			if(objProviderID != DBNull.Value)
				ProviderID = Convert.ToInt32(objProviderID);

			object objProviderName = dr.GetValue (5);
			if(objProviderName != DBNull.Value)
				ProviderName = (string)objProviderName;

			object objProviderColor = dr.GetValue (6);
			if(objProviderColor != DBNull.Value)
				ProviderColor = (string)objProviderColor;

			object objLastStatus = dr.GetValue (7);
			if(objLastStatus != DBNull.Value)
				LastStatus = (string)objLastStatus;

			object objLastStatusArea = dr.GetValue (8);
			if(objLastStatusArea != DBNull.Value)
				LastStatusArea = (string)objLastStatusArea;

			object objLastStatusDate = dr.GetValue (9);
			if(objLastStatusDate != DBNull.Value)
				LastStatusDate = (string)objLastStatusDate;

			object objLastStatusTime = dr.GetValue (10);
			if(objLastStatusTime != DBNull.Value)
				LastStatusTime = (string)objLastStatusTime;

			object objIsDelivered = dr.GetValue (11);
			if (objIsDelivered != DBNull.Value)
				IsDelivered = (bool)objIsDelivered;

			IsNew = false;
		}

		public void Save()
		{
			if (IsNew)
				Insert ();
			else
				Update ();
		}

		public void InsertIfNotExist()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"SELECT * FROM Vouchers WHERE Number = @Number AND ProviderID = @ProviderID AND UserID = @UserID";
					cm.Parameters.AddWithValue("@Number",Number);
					cm.Parameters.AddWithValue("@ProviderID",ProviderID);
					cm.Parameters.AddWithValue("@UserID",PreferencesUtil.UserID);

					bool exists = false;
					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						if (dr.Read ())
							exists = true;
						dr.Close ();
					}

					if (exists == false) 
					{

						cm.CommandText = @"INSERT INTO [Vouchers] ([VoucherID], [Name], [Description], [Number], [ProviderID], [UserID], [LastUpdated], [IsDelivered]) 
											VALUES (@VoucherID, @Name, @Description, @Number, @ProviderID, @UserID, @LastUpdated, @IsDelivered)";
						cm.Parameters.Clear ();
						cm.Parameters.AddWithValue("@VoucherID",VoucherID);
						cm.Parameters.AddWithValue ("@Name", Name);
						cm.Parameters.AddWithValue ("@Description", Description);
						cm.Parameters.AddWithValue ("@Number", Number);
						cm.Parameters.AddWithValue ("@ProviderID", ProviderID);
						cm.Parameters.AddWithValue ("@UserID", UserID);
						cm.Parameters.AddWithValue ("@LastUpdated", LastUpdated);
						cm.Parameters.AddWithValue ("@IsDelivered", IsDelivered);

						cm.ExecuteNonQuery ();
					}

				}
				cn.Close();
			}
		}

		private void Insert()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					try
					{
						if (PreferencesUtil.IsPremium == false)
						{
							cm.CommandText = @"SELECT MAX(VoucherID) FROM Vouchers";
							object val = cm.ExecuteScalar();
							if(val == DBNull.Value || val == null)
								VoucherID = 1;
							else
								VoucherID = Convert.ToInt32(val) + 1;
						}
					
					cm.CommandText = @"INSERT INTO [Vouchers] ([VoucherID], [Name], [Description], [Number], [ProviderID], [UserID], [LastUpdated], [IsDelivered]) 
						VALUES (@VoucherID, @Name, @Description, @Number, @ProviderID, @UserID, @LastUpdated, @IsDelivered)";

					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.Parameters.AddWithValue("@Name",Name);
					cm.Parameters.AddWithValue("@Description",Description);
					cm.Parameters.AddWithValue("@Number",Number);
					cm.Parameters.AddWithValue("@ProviderID",ProviderID);
					cm.Parameters.AddWithValue("@UserID",UserID);
					cm.Parameters.AddWithValue("@LastUpdated",LastUpdated);
					cm.Parameters.AddWithValue ("@IsDelivered", IsDelivered);

					cm.ExecuteNonQuery ();
					}
					catch(Exception ex)
					{
						string m = ex.Message;
					}

				}
				cn.Close();
			}
		}

		public static int GetVouchersCount()
		{
			int count = 0;
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					try
					{	
						cm.CommandText = @"SELECT COUNT(VoucherID) FROM Vouchers WHERE UserID = @UserID";
						cm.Parameters.AddWithValue("@UserID",PreferencesUtil.UserID);
						count = Convert.ToInt32(cm.ExecuteScalar());
					}
					catch(Exception ex)
					{
						string m = ex.Message;
					}

				}
				cn.Close();
			}

			return count;
		}

		private void Update()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"UPDATE [Vouchers] SET
						[Name] = @Name , 
						[Description] = @Description, 
						[Number] = @Number, 
						[ProviderID] = @ProviderID ,
						[LastUpdated] = @LastUpdated,
						[IsDelivered] = @IsDelivered
							WHERE VoucherID = @VoucherID";
					cm.Parameters.AddWithValue("@Name",Name);
					cm.Parameters.AddWithValue("@Description",Description);
					cm.Parameters.AddWithValue("@Number",Number);
					cm.Parameters.AddWithValue("@ProviderID",ProviderID);
					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.Parameters.AddWithValue("@LastUpdated",LastUpdated);
					cm.Parameters.AddWithValue("@IsDelivered",IsDelivered);

					cm.ExecuteNonQuery ();

				}
				cn.Close();
			}
		}

		public void MarkDelivered()
		{
			this.MarkDelivered (true);
		}

		public void MarkNotDelivered()
		{
			this.MarkDelivered (false);
		}

		private void MarkDelivered(bool isDelivered)
		{
			IsDelivered = isDelivered;
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"UPDATE [Vouchers] SET						
						[IsDelivered] = @IsDelivered
							WHERE VoucherID = @VoucherID";
					cm.Parameters.AddWithValue("@IsDelivered",IsDelivered);
					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.ExecuteNonQuery ();
				}
				cn.Close();
			}
		}

		public void Delete()
		{
			DeleteAllStatuses ();
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"DELETE FROM [Vouchers] WHERE VoucherID = @VoucherID";
					cm.Parameters.AddWithValue("@VoucherID",VoucherID);
					cm.ExecuteNonQuery ();
				}
				cn.Close();
			}
		}

		public void DeleteAllStatuses()
		{
			using (SqliteConnection cn = DBManager.CreateConnection ()) 
			{
				using (var cm = cn.CreateCommand ()) 
				{
					cm.CommandText = @"DELETE FROM [VoucherStatuses] WHERE VoucherID = @VoucherID";
					cm.Parameters.AddWithValue ("@VoucherID",VoucherID);
					cm.ExecuteNonQuery ();
				}
				cn.Close();
			}
		}
	}
}

