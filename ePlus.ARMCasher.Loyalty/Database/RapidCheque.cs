using System;

namespace ePlus.ARMCasher.Loyalty.Database
{
	internal class RapidCheque
	{
		private Guid _chequeId;

		private string _operation;

		private decimal _summ;

		private DateTime _date;

		private string _clientId;

		private Guid _requestId;

		public DateTime ChequeDate
		{
			get
			{
				return this._date;
			}
			set
			{
				this._date = value;
			}
		}

		public Guid ChequeId
		{
			get
			{
				return this._chequeId;
			}
			set
			{
				this._chequeId = value;
			}
		}

		public string ClientId
		{
			get
			{
				return this._clientId;
			}
			set
			{
				this._clientId = value;
			}
		}

		public long Date
		{
			get
			{
				return this._date.Ticks;
			}
			set
			{
				this._date = new DateTime(value, DateTimeKind.Local);
			}
		}

		public string Operation
		{
			get
			{
				return this._operation;
			}
			set
			{
				this._operation = value;
			}
		}

		public Guid RequestId
		{
			get
			{
				return this._requestId;
			}
			set
			{
				this._requestId = value;
			}
		}

		public decimal Summ
		{
			get
			{
				return this._summ;
			}
			set
			{
				this._summ = value;
			}
		}

		public RapidCheque()
		{
		}

		public enum RapidChequeOperation
		{
			MakeDiscount,
			CancelDiscount
		}
	}
}