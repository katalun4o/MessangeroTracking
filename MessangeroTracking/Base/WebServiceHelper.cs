using System;
using System.Net;
using System.Linq;
using System.Json;
using System.IO;
using System.Drawing;
using Android.Graphics.Drawables;
using System.Collections.Generic;

namespace MessangeroTracking
{
	public class WebServiceHelper
	{
		//public static string webServiceLocaton = "http://127.0.0.1:81/VoucherTrackingService.svc";
		//public static string webServiceLocaton = "http://137.116.229.15/VoucherTrackingService.svc";
		public static string webServiceLocaton = "http://foxtracking.cloudapp.net/Service1.svc";
		//http://foxtracking.cloudapp.net/Service1.svc/GetProviders

		public static string WebServiceURL 
		{
			get
			{
				return webServiceLocaton + "/GetVoucherStatus?n={0}&t={1}&l={2}";
			}
		}

		public static string GetVoucherStatusFromDBURL
		{
			get
			{
				return webServiceLocaton + "/GetVoucherStatusFromDB?n={0}&t={1}&l={2}&u={3}";
			}
		}

		public static string GetProvidersURL 
		{
			get
			{
				return webServiceLocaton + "/GetProviders";
			}
		}

		public static string RegisterUserURL
		{
			get
			{
				return webServiceLocaton + "/RegisterUser?mail={0}&pass={1}";
			}
		}

		public static string RegisterCloudMessageURL
		{
			get
			{
				return webServiceLocaton + "/RegisterCloudMessage?user_id={0}&gcm_id={1}";
			}
		}

		public static string AddVoucherURL
		{
			get
			{
				return webServiceLocaton + "/AddVoucher?user_id={0}&number={1}&type={2}&name={3}&desc={4}&voucher_id={5}&isDelivered={6}";
			}
		}
		///RegisterCloudMessage?user_id={user_id}&gcm_id={gcm_id}

		public static string LoginUserURL
		{
			get
			{
				return webServiceLocaton + "/LoginUser?mail={0}&pass={1}";
			}
		}

		public static string SocialLoginURL
		{
			get
			{
				return webServiceLocaton + "/SocialLogin?mail={0}&soc_id={1}&soc_user_id={2}";
			}
		}

		public static string GetVouchersURL
		{
			get
			{
				return webServiceLocaton + "/GetVouchers?user_id={0}&date={1}";
			}
		}

		public static string GetUserVoucherStatusesURL
		{
			get
			{
				return webServiceLocaton + "/GetUserVoucherStatuses?ids={0}&user_id={1}&date={2}&lang={3}";
			}
		}

		public static string DeleteVoucherURL
		{
			get
			{
				return webServiceLocaton + "/DeleteVoucher?voucher_id={0}";
			}
		}

		public static string ForgotPasswordURL
		{
			get
			{
				return webServiceLocaton + "/ForgotPassword?mail={0}";
			}
		}

		public static string ChangePasswordURL
		{
			get
			{
				return webServiceLocaton + "/ChangePassword?mail={0}&oldpass={1}&newpass={2}";
			}
		}

		//public const string GetProvidersURL = "http://137.116.229.15/VoucherTrackingService.svc/GetProviders";
		//public const string RegisterUserURL = "http://137.116.229.15/VoucherTrackingService.svc/RegisterUser?user_id={0}&gcm_id={1}";
		//public const string LoginUserURL = "http://137.116.229.15/VoucherTrackingService.svc/LoginUser?user={0}&pass={1}";

		public WebServiceHelper ()
		{

		}

		public static void GetVoucherStatusAsync(Android.App.Activity context, VoucherInfo voucher, Action<VoucherStatus[]> action)
		{
			string number = voucher.Number;
			int providerID = voucher.ProviderID;
			string lang = "el";
			string url = string.Format (WebServiceURL, number, providerID, lang);

			try
			{
				var httpReq = CreateWebRequest(context,url);

				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

				httpReq.BeginGetResponse ((ar) => {
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))
					{
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);
						bool hasErrors = (bool)j ["HasErrors"];
						if (hasErrors == false)
						{
							var results = (from result in (JsonArray)j ["VoucherStatuses"]
							               let jResult = result as JsonObject
							               select new VoucherStatus () {Status = jResult ["Status"].ToString ().TrimStart ('\"').TrimEnd ('\"'), 
								Area = jResult ["Area"].ToString ().TrimStart ('\"').TrimEnd ('\"'),
								Date = jResult ["Date"].ToString ().TrimStart ('\"').TrimEnd ('\"'),
								Time = jResult ["Time"].ToString ().TrimStart ('\"').TrimEnd ('\"')
							}).ToArray();


							voucher.DeleteAllStatuses ();
							foreach (VoucherStatus vStatus in results)
							{
								vStatus.VoucherID = voucher.VoucherID;
								vStatus.Save ();
							}

							context.RunOnUiThread (() => {
								action (results);
							});
						} else
						{
							context.RunOnUiThread (() => {
								action (null);
							});
						}

						//RunOnUiThread (() => { } );
						//int a = 0;
					}
				}, httpReq);
			} catch (Exception ex)
			{
				string msg = ex.Message;
				context.RunOnUiThread (() => {
					action (null);
				});
			}
		}

		public static void GetVoucherStatusFromDBAsync(Android.App.Activity context, VoucherInfo voucher, Action<VoucherStatus[]> action)
		{
			string number = voucher.Number;
			int providerID = voucher.ProviderID;
			string lang = "el";
			string url = string.Format (GetVoucherStatusFromDBURL, number, providerID, lang, PreferencesUtil.UserID);

			try
			{
				var httpReq = CreateWebRequest(context,url);

				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

				httpReq.BeginGetResponse ((ar) => {
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))
					{
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);
						bool hasErrors = (bool)j ["HasErrors"];
						if (hasErrors == false)
						{
							var results = (from result in (JsonArray)j ["VoucherStatuses"]
								let jResult = result as JsonObject
								select new VoucherStatus () {Status = jResult ["Status"].ToString ().TrimStart ('\"').TrimEnd ('\"'), 
									Area = jResult ["Area"].ToString ().TrimStart ('\"').TrimEnd ('\"'),
									Date = jResult ["Date"].ToString ().TrimStart ('\"').TrimEnd ('\"'),
									Time = jResult ["Time"].ToString ().TrimStart ('\"').TrimEnd ('\"')
								}).ToArray ();


							voucher.DeleteAllStatuses ();
							foreach (VoucherStatus vStatus in results)
							{
								vStatus.VoucherID = voucher.VoucherID;
								vStatus.Save ();
							}

							context.RunOnUiThread (() => {
								action (results);
							});
						} else
						{
							context.RunOnUiThread (() => {
								action (null);
							});
						}

						//RunOnUiThread (() => { } );
						//int a = 0;
					}
				}, httpReq);
			} catch (Exception ex)
			{
				string msg = ex.Message;
				context.RunOnUiThread (() => {
					action (null);
				});
			}
		}

		public static void GetProvidersAsync(Android.App.Activity context, Action<Provider[]> action)
		{
			string url = GetProvidersURL;

			try
			{
			var httpReq = CreateWebRequest(context,url);

				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {
					var s = response.GetResponseStream ();
					var j = (JsonArray)JsonArray.Load (s);

					var results = (from result in j
						let jResult = result as JsonObject
						select new Provider(){ProviderID = (int)jResult ["ProviderID"], 
							Name = jResult ["Name"].ToString().TrimStart('\"').TrimEnd('\"'),
							Color = jResult ["Color"].ToString().TrimStart('\"').TrimEnd('\"'),
							IsActive = (bool)jResult ["IsActive"],
							Logo = (string)jResult ["Logo"]
						}).ToArray ();

					Provider.DeleteAll();
					foreach(var prov in results)
					{
						prov.Insert();
						if(string.IsNullOrEmpty(prov.Logo) == false)
							SaveImage(prov.ProviderID,prov.Logo);
						//save image to image dir

					}
					context.RunOnUiThread(() => { action(results);});
					//RunOnUiThread (() => { } );
					//int a = 0;
				}
			} , httpReq);
			}
			catch(Exception ex)
			{
				string msg = ex.Message;
				context.RunOnUiThread (() => {
					action (null);
				});
			}
		}

		public static void SaveImage(int providerID, string base64String)
		{
			System.IO.Stream s = new MemoryStream(Convert.FromBase64String(base64String));
			byte[] arr = Convert.FromBase64String(base64String);
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			String dir = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath,".messangeroTracking");
			if (!System.IO.Directory.Exists(dir))
			{
				System.IO.Directory.CreateDirectory(dir);
			}
			String filePath = System.IO.Path.Combine(dir, providerID.ToString()+".jpg");

			System.IO.FileStream fs = new FileStream (filePath, FileMode.Create, FileAccess.ReadWrite);
			fs.Write (arr, 0, arr.Length);
			fs.Close ();
			s.Close ();
						
			//Drawable img = Drawable.CreateFromStream(s, null);
			//return img;
		}

		public static void LoginUser(string mail, string password, Android.App.Activity context, Action action)
		{
			string url = string.Format (LoginUserURL, mail, password);
			try
			{
				var httpReq = CreateWebRequest (context, url);

				if (httpReq == null)
				{
					context.RunOnUiThread (() => {
						action ();
					});
					return;
				}

				httpReq.BeginGetResponse ((ar) => {
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))
					{
						var s = response.GetResponseStream ();
						//var j = (JsonArray)JsonArray.Load (s);
						var j = (JsonObject)JsonObject.Load (s);

						int userid = (int)j ["UserID"];
						string rMail = (string)j ["Mail"];
						string rPass = (string)j ["Password"];

						PreferencesUtil.UserID = userid;
						PreferencesUtil.Mail = rMail;
						PreferencesUtil.Password = rPass;
						PreferencesUtil.SocID = "";
						PreferencesUtil.SocUserID = "";
						PreferencesUtil.IsPremium = (bool)j ["IsPremium"];
						PreferencesUtil.SavePreferences (context);

						context.RunOnUiThread (() => {
							action ();
						});

					}
				}, httpReq);
			} catch (Exception ex)
			{
				context.RunOnUiThread (() => {
					action ();
				});
			}
		}

		private static HttpWebRequest CreateWebRequest(Android.App.Activity context, string url)
		{

			//check if network is available
			var connectivityManager = (Android.Net.ConnectivityManager)context.GetSystemService(Android.Content.Context.ConnectivityService);
			if (!(connectivityManager.ActiveNetworkInfo != null && connectivityManager.ActiveNetworkInfo.IsConnected))
			{
				//no connection

				Android.Widget.Toast.MakeText (context, context.GetString(Resource.String.msgNoConnection), Android.Widget.ToastLength.Short).Show();
				return null;
			}

			var httpReq =  (HttpWebRequest) HttpWebRequest.Create(url);
			httpReq.Timeout = 5 * 60 * 1000;
			return httpReq;
		}

		private static void CheckInternetAvailable()
		{

		}

		public static void SocialLogin(string mail, string soc_id, string soc_user_id, Android.App.Activity context, Action action)
		{
			string url = string.Format(SocialLoginURL, mail, soc_id, soc_user_id);
			try
			{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action();});
					return;
				}

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {
					var s = response.GetResponseStream ();
					var j = (JsonObject)JsonObject.Load (s);

					int userid = (int)j["UserID"];

					PreferencesUtil.UserID = userid;
					PreferencesUtil.Mail = "";
					PreferencesUtil.Password = "";
					PreferencesUtil.SocID = soc_id;
					PreferencesUtil.SocUserID = soc_user_id;
					PreferencesUtil.IsPremium = (bool)j["IsPremium"];
					PreferencesUtil.SavePreferences(context);

					context.RunOnUiThread(() => { action();});
				}
			} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action();});
			}
		}

		public static void RegisterUser(string mail, string pass, Android.App.Activity context, Action<User> action)
		{
			string url = string.Format(RegisterUserURL, mail, pass);
			try
			{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {

					var s = response.GetResponseStream ();
					//var j = (JsonArray)JsonArray.Load (s);
					var j = (JsonObject)JsonObject.Load (s);

					int userid = (int)j["UserID"];
					string rMail = (string)j["Mail"];
					string rPass = (string)j["Password"];
					int errorID = (int)j["ErrorID"];

					User user = new User(){UserID = userid, Email = rMail, ErrorID = errorID };

					//PreferencesUtil.UserID = userid;
					//PreferencesUtil.Mail = rMail;
					//PreferencesUtil.Password = rPass;
					//PreferencesUtil.SavePreferences(context);

					context.RunOnUiThread(() => { action(user);});
				}
			} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action(null);});
			}
		}

		public static void RegisterCloudMessage(int userID, string gcm_id, Android.App.Activity context)
		{
			string url = string.Format(RegisterCloudMessageURL,userID,gcm_id);
			try
			{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
					return;

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {

				}
			} , httpReq);
			}
			catch(Exception ex)
			{
			}
		}

		public static void AddVoucher(decimal voucher_id, string  number, int providerID, string name, string desc, bool isDelivered, Android.App.Activity context, Action<VoucherInfo> action)
		{
			string url = string.Format(AddVoucherURL, PreferencesUtil.UserID, number, providerID, name, desc, voucher_id == 0 ? "": voucher_id.ToString(), isDelivered.ToString());
			try
			{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {
					var s = response.GetResponseStream ();

					VoucherInfo v = null;

					if(s.Length == 0)
					{
						action(v);
						return;
					}

					JsonObject jResult = (JsonObject)JsonObject.Load(s);

					v = new VoucherInfo(){VoucherID = (int)jResult ["VoucherID"], 
						Number = jResult ["Number"].ToString().TrimStart('\"').TrimEnd('\"'),
						Name = jResult["Name"] == null ? "" : jResult ["Name"].ToString().TrimStart('\"').TrimEnd('\"'),
						Description = jResult["Description"] == null ? "" : jResult ["Description"].ToString().TrimStart('\"').TrimEnd('\"'),
						ProviderID = (int)jResult["ProviderID"],
						Statuses =  VoucherStatus.LoadStatuses((JsonArray)jResult["Statuses"])
					};


					context.RunOnUiThread(() => { action(v);});
				};
			} , httpReq);
			}
			catch(Exception ex)
			{
			}
		}

		public static void DeleteVoucher(decimal voucher_id, Android.App.Activity context, Action action)
		{
			string url = string.Format(DeleteVoucherURL, voucher_id == 0 ? "": voucher_id.ToString());
			try{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action();});
					return;
				}

			httpReq.BeginGetResponse ((ar) => {
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {
					//var s = response.GetResponseStream ();
					context.RunOnUiThread(() => { action();});
				};
			} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action();});
			}
		}

		public static void GetVouchers(Android.App.Activity context, Action<VoucherInfo[]> action)
		{
			DateTime? date = DBManager.GetLastDate ();
			string url = "";
			if(date == null)
				url = string.Format(GetVouchersURL,PreferencesUtil.UserID, "");
			else
				url = string.Format(GetVouchersURL,PreferencesUtil.UserID, date.Value.ToString("yyyy-MM-dd"));

			try
			{
			var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

			httpReq.BeginGetResponse ((ar) => 
				{
				var request = (HttpWebRequest)ar.AsyncState;
				using (var response = (HttpWebResponse)request.EndGetResponse (ar))     
				{
					var s = response.GetResponseStream ();
						var j = (JsonArray)JsonArray.Load(s);

					var results = (from result in j
						let jResult = result as JsonObject
						select new VoucherInfo(){VoucherID = (int)jResult ["VoucherID"], 
							Number = jResult ["Number"].ToString().TrimStart('\"').TrimEnd('\"'),
							Name = jResult["Name"] == null ? "" : jResult ["Name"].ToString().TrimStart('\"').TrimEnd('\"'),
							Description = jResult["Description"] == null ? "" : jResult ["Description"].ToString().TrimStart('\"').TrimEnd('\"'),
								ProviderID = (int)jResult["ProviderID"],
								IsDelivered = (bool)jResult["IsDelivered"],
								Statuses =  VoucherStatus.LoadStatuses((JsonArray)jResult["Statuses"]),
								LastUpdated = DateTime.Parse(jResult["LastUpdated"])
						}).ToArray ();


							lock(typeof(WebServiceHelper))
							{
								foreach(var voucher in results)
								{
									voucher.InsertIfNotExist();

									foreach(var status in voucher.Statuses)
									{
										status.InsertIfNotExist();
									}
								}
							}

					context.RunOnUiThread(() => { action(results);});
				}
			} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action(null);});
			}
		}

		public static void GetUserVouchersStatuses(List<decimal> voucherIDs, Android.App.Activity context, Action<VoucherInfo[]> action)
		{
			DateTime? date = DBManager.GetLastDate ();

			string url = "";
			string vIDs = "";
			foreach (var vID in voucherIDs)
				vIDs += vID.ToString () + ",";
			vIDs = vIDs.TrimEnd (',');

			if(date == null)
				url = string.Format(GetUserVoucherStatusesURL, vIDs,PreferencesUtil.UserID, "", PreferencesUtil.Language);
			else
				url = string.Format(GetUserVoucherStatusesURL, vIDs,PreferencesUtil.UserID, date.Value.ToString("yyyy-MM-dd"), PreferencesUtil.Language);

			try
			{

			var httpReq = CreateWebRequest(context,url);

				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(null);});
					return;
				}

			httpReq.BeginGetResponse ((ar) => 
				{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     
					{
						var s = response.GetResponseStream ();
						var j = (JsonArray)JsonArray.Load(s);

						var results = (from result in j
							let jResult = result as JsonObject
							select new VoucherInfo(){VoucherID = (int)jResult ["VoucherID"], 
								Number = jResult ["Number"].ToString().TrimStart('\"').TrimEnd('\"'),
								Name = jResult["Name"] == null ? "" : jResult ["Name"].ToString().TrimStart('\"').TrimEnd('\"'),
								Description = jResult["Description"] == null ? "" : jResult ["Description"].ToString().TrimStart('\"').TrimEnd('\"'),
								ProviderID = (int)jResult["ProviderID"],
								Statuses =  VoucherStatus.LoadStatuses((JsonArray)jResult["Statuses"]),
								LastUpdated = DateTime.Parse(jResult["LastUpdated"])
							}).ToArray ();

						foreach(var voucher in results)
						{
							//voucher.InsertIfNotExist();

							foreach(var status in voucher.Statuses)
							{
								status.InsertIfNotExist();
							}
						}

						context.RunOnUiThread(() => { action(results);});
					}
				} , httpReq);
			}
			catch(WebException ex)
			{
				context.RunOnUiThread(() => { action(null);});
			}
		}

		public static void ForgotPassword(Android.App.Activity context, string mail, Action<int> action)
		{
			string url = string.Format(ForgotPasswordURL, mail);

			try
			{
				var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(1);});
					return;
				}

				httpReq.BeginGetResponse ((ar) => 
					{
						var request = (HttpWebRequest)ar.AsyncState;
						using (var response = (HttpWebResponse)request.EndGetResponse (ar))     
						{
							var s = response.GetResponseStream ();
							var j = (JsonObject)JsonObject.Load(s);
							int errorID = j["ErrorID"];

							context.RunOnUiThread(() => { action(errorID);});
						}
					} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action(1);});
			}
		}

		public static void ChangePassword(Android.App.Activity context, string mail, string oldPassword, string newPassword, Action<int> action)
		{
			string url = string.Format(ChangePasswordURL, mail, oldPassword, newPassword);

			User user = null;
			try
			{
				var httpReq = CreateWebRequest(context,url);
				if(httpReq == null)
				{
					context.RunOnUiThread(() => { action(0);});
					return;
				}

				httpReq.BeginGetResponse ((ar) => 
					{
						var request = (HttpWebRequest)ar.AsyncState;
						using (var response = (HttpWebResponse)request.EndGetResponse (ar))     
						{
							var s = response.GetResponseStream ();
							var j = (JsonObject)JsonObject.Load (s);

							string rPass = (string)j["Password"];
							int errorID = (int)j["ErrorID"];

							if(errorID == 0)
							{
								PreferencesUtil.Password = rPass;
								PreferencesUtil.SavePreferences(context);
							}

							context.RunOnUiThread(() => { action(errorID);});

						}
					} , httpReq);
			}
			catch(Exception ex)
			{
				context.RunOnUiThread(() => { action(1);});
			}
		}

	}
}

