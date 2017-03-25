using System;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	public class RemoteUnitTests
	{
		[Fact]
		public void CreateProxy_CtorArguments() {
			string message = "Foo";
			Remote<TestProxy> target = Remote<TestProxy>.CreateProxy(AppDomain.CurrentDomain, message);

			Assert.NotNull(target.Domain);
			Assert.Equal(AppDomain.CurrentDomain, target.Domain);
			Assert.NotNull(target.RemoteObject);
			Assert.Equal(message, target.RemoteObject.Message);
		}

		[Fact]
		public void CreateProxy_CurrentAppDomain() {
			Remote<TestProxy> target = Remote<TestProxy>.CreateProxy(AppDomain.CurrentDomain);

			Assert.NotNull(target.Domain);
			Assert.Equal(AppDomain.CurrentDomain, target.Domain);
			Assert.NotNull(target.RemoteObject);
			Assert.Equal(TestProxy.MundaneMessage, target.RemoteObject.Message);
		}

		[Fact]
		public void CreateProxy_NullDisposableDomain() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					Remote<TestProxy> target = Remote<TestProxy>.CreateProxy((DisposableAppDomain) null);
				});

		[Fact]
		public void CreateProxy_NullDomain() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					Remote<TestProxy> target = Remote<TestProxy>.CreateProxy((AppDomain) null);
				});

		[Fact]
		public void Dispose_CurrentAppDomain() {
			Remote<TestProxy> target = Remote<TestProxy>.CreateProxy(AppDomain.CurrentDomain);
			target.Dispose();

			Assert.True(target.IsDisposed);
		}

		[Fact]
		public void Dispose_DomainProperty() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					Remote<TestProxy> target = Remote<TestProxy>.CreateProxy(AppDomain.CurrentDomain);
					target.Dispose();

					Assert.True(target.IsDisposed);

					AppDomain domain = target.Domain;
				});

		[Fact]
		public void Dispose_RemoteObjectProperty() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					Remote<TestProxy> target = Remote<TestProxy>.CreateProxy(AppDomain.CurrentDomain);
					target.Dispose();

					Assert.True(target.IsDisposed);

					TestProxy proxy = target.RemoteObject;
				});
	}


	/// <summary>
	/// This private class exists only to facilitate very basic testing of remoting into another application
	/// domain. It doesn't do anything at all except get instantiated.
	/// </summary>
	internal class TestProxy : MarshalByRefObject
	{
		internal static readonly string MundaneMessage = "Hello World";


		public TestProxy() {
			Message = TestProxy.MundaneMessage;
		}

		public TestProxy(string message) {
			Message = message;
		}


		public string Message { get; private set; }
	}
}
