using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace MessangeroTracking
{
	public class adapterLanguages : ArrayAdapter<LanguageInfo>
	{
		Activity context;

		public adapterLanguages(Activity context, List<LanguageInfo> list)
			: base(context, Resource.Layout.row_provider, list)
		{
			this.context = context;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//var item = this.TransHedList[position];
			var item = this.GetItem (position);
			View view = convertView;
			if (view == null)
				view = context.LayoutInflater.Inflate (Resource.Layout.row_language, null);

			TextView tvLanguage = view.FindViewById<TextView> (Resource.Id.tvLanguage);
			tvLanguage.Text = item.Language;

			return view;
		}
	}
}

