using System;

namespace ePlus.ARMCasher.Loyalty.RapidSoft
{
	internal enum CardStatus
	{
		NotFound = 0,
		Active = 1,
		Limited = 20,
		Locked = 30,
		NotActivated = 40,
		Expired = 50
	}
}