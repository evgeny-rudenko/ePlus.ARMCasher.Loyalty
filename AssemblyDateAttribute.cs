using System;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
[ComVisible(true)]
public sealed class AssemblyDateAttribute : Attribute
{
	private readonly static DateTime dt;

	public DateTime AssemblyDate
	{
		get
		{
			return AssemblyDateAttribute.dt;
		}
	}

	static AssemblyDateAttribute()
	{
		AssemblyDateAttribute.dt = new DateTime(2021, 12, 1);
	}

	public AssemblyDateAttribute()
	{
	}
}