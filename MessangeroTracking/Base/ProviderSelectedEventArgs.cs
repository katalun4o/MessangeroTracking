using System;

namespace MessangeroTracking
{
	public class ProviderSelectedEventArgs : EventArgs
	{
		public bool IsDelivered {get;set;}
		public Provider Provider{ get; set;}

		public ProviderSelectedEventArgs ()
		{
			IsDelivered = false;
		}

		public ProviderSelectedEventArgs (Provider prov, bool isDelivered)
		{
			IsDelivered = isDelivered;
			this.Provider = prov;
		}
	}
}

