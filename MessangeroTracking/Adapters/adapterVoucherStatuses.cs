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
	public class adapterVoucherStatuses : ArrayAdapter<VoucherStatus>
	{
		Activity context;

		public adapterVoucherStatuses(Activity context, List<VoucherStatus> list)
			: base(context, Resource.Layout.row_voucher_status, list)
		{
			this.context = context;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//var item = this.TransHedList[position];
			var item = this.GetItem (position);
			View view = convertView;
			if (view == null)
				view = context.LayoutInflater.Inflate (Resource.Layout.row_voucher_status, null);

			TextView tvDate = view.FindViewById<TextView> (Resource.Id.tvDate);
			TextView tvTime = view.FindViewById<TextView> (Resource.Id.tvTime);
			TextView tvStatus = view.FindViewById<TextView> (Resource.Id.tvStatus);
			TextView tvArea = view.FindViewById<TextView> (Resource.Id.tvArea);

			tvDate.Text = item.Date;
			tvTime.Text = item.Time;
			tvStatus.Text = item.Status;
			tvArea.Text = item.Area;

			return view;
		}
	}
}

