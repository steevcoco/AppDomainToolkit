using System;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	public class DisposableAppDomainUnitTests
	{
		[Fact]
		public void Ctor_CurrentApplicationDomain() {
			DisposableAppDomain target = new DisposableAppDomain(AppDomain.CurrentDomain);

			Assert.NotNull(target);
			Assert.NotNull(target.Domain);
			Assert.False(target.IsDisposed);
		}

		[Fact]
		public void Ctor_NullAppDomain() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					DisposableAppDomain target = new DisposableAppDomain(null);
				});

		[Fact]
		public void Dispose_CurrentAppDomain() {
			// The current app domain should NOT be unloaded, but the object should be disposed.
			DisposableAppDomain target = new DisposableAppDomain(AppDomain.CurrentDomain);
			target.Dispose();

			Assert.True(target.IsDisposed);
		}

		[Fact]
		public void Dispose_DomainProp() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					// The current app domain should NOT be unloaded, but the object should be disposed.
					DisposableAppDomain target = new DisposableAppDomain(AppDomain.CreateDomain("My domain"));
					target.Dispose();

					Assert.True(target.IsDisposed);

					AppDomain domain = target.Domain;
				});

		[Fact]
		public void Dispose_ValidAppDomain() {
			DisposableAppDomain target = new DisposableAppDomain(AppDomain.CreateDomain("My domain"));
			target.Dispose();

			Assert.True(target.IsDisposed);
		}
	}
}
