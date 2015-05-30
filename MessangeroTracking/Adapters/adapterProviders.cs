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
	public class adapterProviders : ArrayAdapter<Provider>
	{
		Activity context;

		public adapterProviders(Activity context, List<Provider> list)
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
				view = context.LayoutInflater.Inflate (Resource.Layout.row_provider, null);

			TextView tvProviderName = view.FindViewById<TextView> (Resource.Id.tvProviderName);
			ImageView ivProvider = view.FindViewById<ImageView> (Resource.Id.ivProvider);

			ivProvider.SetImageURI (Android.Net.Uri.Parse (item.GetImagePath ()));
			ivProvider.LayoutParameters.Width = 60;
			ivProvider.LayoutParameters.Height = 30;

			tvProviderName.Gravity = GravityFlags.Right;
			((RelativeLayout.LayoutParams)tvProviderName.LayoutParameters).RightMargin = 5;

			tvProviderName.Text = item.Name;

			return view;
		}
	}
}

