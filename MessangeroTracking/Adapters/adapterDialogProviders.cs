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
	public class adapterDialogProviders : ArrayAdapter<Provider>
	{
		Activity context;

		public adapterDialogProviders(Activity context, List<Provider> list)
			: base(context, Resource.Layout.row_dialog_provider, list)
		{
			this.context = context;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//var item = this.TransHedList[position];
			var item = this.GetItem (position);
			View view = convertView;
			if (view == null)
				view = context.LayoutInflater.Inflate (Resource.Layout.row_provider, null);

			TextView tvProviderName = view.FindViewById<TextView> (Resource.Id.tvProviderName);
			tvProviderName.Text = item.Name;

			ImageView ivProvider = view.FindViewById<ImageView> (Resource.Id.ivProvider);
			ivProvider.SetImageURI (Android.Net.Uri.Parse (item.GetImagePath ()));

			return view;
		}
	}
}

