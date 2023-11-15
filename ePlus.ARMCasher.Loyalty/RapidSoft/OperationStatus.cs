using System;

namespace ePlus.ARMCasher.Loyalty.RapidSoft
{
	internal enum OperationStatus
	{
		Success = 0,
		UnknownError = 1,
		Denied = 2,
		NoLoyalty = 3,
		AnotherLoyalty = 99
	}
}