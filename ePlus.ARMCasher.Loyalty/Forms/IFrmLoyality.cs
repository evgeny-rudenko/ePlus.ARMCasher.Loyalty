using ePlus.ARMCommon;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public interface IFrmLoyality : IBaseView
	{
		void Bind(Dictionary<LoyaltySettings, string> list);

		event Action<LoyaltySettings> LoyaltyTypeSelected;
	}
}