using Android.Content;
using System.Collections.Generic;
using System.Linq;

namespace MessangeroTracking
{
	public class PreferencesUtil
	{
		public static bool IsDebug = false;
		private static string APP_SHARED_PREFS = "com.messangero.tracking";
		public static int UserID = 0;
		public static int LanguageID = 0;
		public static string Mail = "";
		public static string Password = "";
		public static string SocID = "";
		public static string SocUserID = "";
		public static string Language = "en";
		public static bool ShowDelivered = false;
		public static bool IsPremium = false;

		public static void ClearUserData()
		{
			PreferencesUtil.UserID = 0;
			PreferencesUtil.Mail = "";
			PreferencesUtil.Password = "";
			PreferencesUtil.SocID = "";
			PreferencesUtil.SocUserID = "";
		}

		public static void SavePreferences(Context context)
		{
			ISharedPreferences appSharedPrefs = context.GetSharedPreferences(
				APP_SHARED_PREFS, FileCreationMode.Private);
			ISharedPreferencesEditor editor = appSharedPrefs.Edit();
			editor.PutInt("UserID", UserID);
			editor.PutInt("LanguageID", LanguageID);
			editor.PutString("Mail", Mail);
			editor.PutString("Password", Password);
			editor.PutString("SocID", SocID);
			editor.PutString("SocUserID", SocUserID);
			editor.PutBoolean("ShowDelivered", ShowDelivered);
			editor.PutBoolean("IsPremium", IsPremium);
			editor.Commit();
		}

		public static void LoadSettings(Context context)
		{
			ISharedPreferences appSharedPrefs = context.GetSharedPreferences(
				APP_SHARED_PREFS, FileCreationMode.Private);

			UserID = appSharedPrefs.GetInt("UserID", 0);
			LanguageID = appSharedPrefs.GetInt("LanguageID", 1);
			Mail = appSharedPrefs.GetString("Mail", "");
			Password = appSharedPrefs.GetString("Password", "");
			SocID = appSharedPrefs.GetString("SocID", "");
			SocUserID = appSharedPrefs.GetString("SocUserID", "");
			ShowDelivered = appSharedPrefs.GetBoolean("ShowDelivered", false);
			IsPremium = appSharedPrefs.GetBoolean("IsPremium", false);
		}

		public static List<LanguageInfo> LanguagesList = new List<LanguageInfo> ()
		{
			new LanguageInfo(){ID = 1, Language = "English", Locale="en"},
			new LanguageInfo(){ID = 2, Language = "Greek", Locale="gr"},
			new LanguageInfo(){ID = 3, Language = "Bulgarian", Locale="bg"}
		};

		public static LanguageInfo GetCurrentLanguage()
		{
			var l =(from i in LanguagesList
				where i.ID == LanguageID
				select i).FirstOrDefault();

			return l;
		}

		public static void UpdateCurrentCulture(Context c)
		{
			var l = GetCurrentLanguage ();

			SetCurrentCulture (c, l.Locale);
		}

		private static void SetCurrentCulture(Context context, string localeS) 
		{ 
			Java.Util.Locale locale = new Java.Util.Locale(localeS);
			Java.Util.Locale.Default = locale;
			Android.Content.Res.Configuration config = new Android.Content.Res.Configuration();
			config.Locale = locale;
			context.Resources.UpdateConfiguration(config,
				context.Resources.DisplayMetrics);
		}
	}
}
