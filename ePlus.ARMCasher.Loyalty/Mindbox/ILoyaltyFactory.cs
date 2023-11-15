using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.Loyalty;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	internal interface ILoyaltyFactory
	{
		LoyaltyCard CreateLoyaltyCard();

		CardReader GetCardReader(IWin32Window parent);
	}
}