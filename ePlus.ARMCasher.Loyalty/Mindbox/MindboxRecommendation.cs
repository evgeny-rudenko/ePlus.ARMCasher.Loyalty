using ePlus.ARMCasher.BusinessObjects;
using ePlus.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	public class MindboxRecommendation : IRecommendation, IObjectHoder<STOCK_DETAIL>
	{
		private HashSet<STOCK_DETAIL> items = new HashSet<STOCK_DETAIL>();

		public string Code
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return string.Empty;
				}
				return this.items.First<STOCK_DETAIL>().CODE;
			}
		}

		public string CodeTo
		{
			get
			{
				return JustDecompileGenerated_get_CodeTo();
			}
			set
			{
				JustDecompileGenerated_set_CodeTo(value);
			}
		}

		private string JustDecompileGenerated_CodeTo_k__BackingField;

		public string JustDecompileGenerated_get_CodeTo()
		{
			return this.JustDecompileGenerated_CodeTo_k__BackingField;
		}

		private void JustDecompileGenerated_set_CodeTo(string value)
		{
			this.JustDecompileGenerated_CodeTo_k__BackingField = value;
		}

		public Guid GoodsGuid
		{
			get
			{
				STOCK_DETAIL obj = this.GetObject();
				if (obj == null)
				{
					return Guid.Empty;
				}
				return obj.ID_LOT_GLOBAL;
			}
		}

		public string GoodsName
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return string.Empty;
				}
				return this.items.First<STOCK_DETAIL>().GOODS_NAME;
			}
		}

		public int Marginality
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return 0;
				}
				int? marginInt = this.items.First<STOCK_DETAIL>().MarginInt;
				if (!marginInt.HasValue)
				{
					return 0;
				}
				return marginInt.GetValueOrDefault();
			}
		}

		public decimal Price
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return new decimal(0);
				}
				return this.items.Max<STOCK_DETAIL>((STOCK_DETAIL i) => i.LOT_PRICE_VAT);
			}
		}

		public decimal Quantity
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return new decimal(0);
				}
				return this.items.Sum<STOCK_DETAIL>((STOCK_DETAIL i) => i.ACCESSIBLE);
			}
		}

		public ePlus.Interfaces.RecommendationType RecommendationType
		{
			get
			{
				return JustDecompileGenerated_get_RecommendationType();
			}
			set
			{
				JustDecompileGenerated_set_RecommendationType(value);
			}
		}

		private ePlus.Interfaces.RecommendationType JustDecompileGenerated_RecommendationType_k__BackingField;

		public ePlus.Interfaces.RecommendationType JustDecompileGenerated_get_RecommendationType()
		{
			return this.JustDecompileGenerated_RecommendationType_k__BackingField;
		}

		private void JustDecompileGenerated_set_RecommendationType(ePlus.Interfaces.RecommendationType value)
		{
			this.JustDecompileGenerated_RecommendationType_k__BackingField = value;
		}

		public string RecommendationTypeString
		{
			get
			{
				return JustDecompileGenerated_get_RecommendationTypeString();
			}
			set
			{
				JustDecompileGenerated_set_RecommendationTypeString(value);
			}
		}

		private string JustDecompileGenerated_RecommendationTypeString_k__BackingField;

		public string JustDecompileGenerated_get_RecommendationTypeString()
		{
			return this.JustDecompileGenerated_RecommendationTypeString_k__BackingField;
		}

		private void JustDecompileGenerated_set_RecommendationTypeString(string value)
		{
			this.JustDecompileGenerated_RecommendationTypeString_k__BackingField = value;
		}

		public bool Show
		{
			get;
			set;
		}

		public string StoragePlace
		{
			get
			{
				if (!this.items.Any<STOCK_DETAIL>())
				{
					return string.Empty;
				}
				return this.items.First<STOCK_DETAIL>().STORE_PLACE_NAME;
			}
		}

		public MindboxRecommendation(ePlus.Interfaces.RecommendationType type, string typeString, string codeTo, IEnumerable<STOCK_DETAIL> itemList, bool show)
		{
			this.RecommendationType = type;
			this.CodeTo = codeTo;
			this.RecommendationTypeString = typeString;
			this.Show = show;
			foreach (STOCK_DETAIL sTOCKDETAIL in itemList)
			{
				this.items.Add(sTOCKDETAIL);
			}
		}

		public override bool Equals(object obj)
		{
			return this.GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return (this.Code ?? string.Empty).GetHashCode();
		}

		public STOCK_DETAIL GetObject()
		{
			return this.items.FirstOrDefault<STOCK_DETAIL>();
		}
	}
}