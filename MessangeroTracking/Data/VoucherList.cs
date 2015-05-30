using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace MessangeroTracking
{
	public class VoucherList: List<VoucherInfo>
	{
		public VoucherList ()
		{
		}

		public void Load()
		{
			Load (-1);
		}

		public void Load(int providerID)
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
LEFT JOIN Providers ON Providers.ProviderID = Vouchers.ProviderID
LEFT JOIN VoucherStatuses ON VoucherStatuses.VoucherStatusID = (SELECT MAX(VoucherStatusID) FROM VoucherStatuses WHERE VoucherStatuses.VoucherID = Vouchers.VoucherID)
WHERE UserID = @UserID
 ";
					cm.Parameters.AddWithValue ("@UserID",PreferencesUtil.UserID);
					if (providerID != -1) 
					{
						cm.CommandText += " AND [Vouchers].[ProviderID] = @ProviderID";
						cm.Parameters.AddWithValue ("@ProviderID",providerID);
					}
					if (PreferencesUtil.ShowDelivered == false)
					{
						cm.CommandText += " AND ([Vouchers].[IsDelivered] IS NULL OR [Vouchers].[IsDelivered] = 0)";
					}

					using (SqliteDataReader dr = cm.ExecuteReader ()) 
					{
						while (dr.Read ()) 
						{
							VoucherInfo vi = new VoucherInfo ();
							vi.Load (dr);
							this.Add (vi);
						}
						dr.Close ();
					}
				}
				cn.Close();
			}
		}
	}
}
