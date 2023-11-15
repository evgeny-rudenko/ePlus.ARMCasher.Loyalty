using System;

namespace ePlus.ARMCasher.Loyalty.LSPoint
{
	internal class GoodsInfo
	{
		private string _barCode;

		private short _flags;

		private string _name;

		private decimal _quantity;

		private decimal _price;

		public string BarCode
		{
			get
			{
				return this._barCode;
			}
			set
			{
				this._barCode = value;
			}
		}

		public short Flags
		{
			get
			{
				return this._flags;
			}
			set
			{
				this._flags = value;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public decimal Price
		{
			get
			{
				return this._price;
			}
			set
			{
				this._price = value;
			}
		}

		public decimal Quantity
		{
			get
			{
				return this._quantity;
			}
			set
			{
				this._quantity = value;
			}
		}

		public GoodsInfo()
		{
		}
	}
}