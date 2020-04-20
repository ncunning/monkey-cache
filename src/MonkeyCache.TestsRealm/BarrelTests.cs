using MonkeyCache.Realm;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.Tests
{
	public partial class BarrelTests
	{
		public virtual void SetupBarrel()
		{
			Barrel.ApplicationId = "com.refractored.monkeyrealm";
			barrel = Barrel.Current;
		}
	}

	public partial class CustomDirBarrelTests
	{
		public override void SetupBarrel()
		{
			var dir = BarrelUtils.GetBasePath("com.refractored.monkeyfile.customdir");
			this.barrel = Barrel.Create(dir);
		}
	}

	public partial class BarrelUtilTests
	{
		private void SetupBarrel(ref IBarrel barrel)
		{
			Barrel.ApplicationId = "com.refractored.monkeyrealm";
			barrel = Barrel.Current;
		}
	}
}