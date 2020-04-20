using MonkeyCache.Realm;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.Tests
{
	public partial class HttpCacheTests
	{
		private void SetupBarrel()
		{
			Barrel.ApplicationId = "com.refractored.monkeyrealm";
			barrel = Barrel.Current;
		}
	}
}