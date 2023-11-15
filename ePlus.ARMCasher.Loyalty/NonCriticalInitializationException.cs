using System;

namespace ePlus.ARMCasher.Loyalty
{
	internal class NonCriticalInitializationException : Exception
	{
		public NonCriticalInitializationException()
		{
		}

		public NonCriticalInitializationException(string message) : base(message)
		{
		}
	}
}