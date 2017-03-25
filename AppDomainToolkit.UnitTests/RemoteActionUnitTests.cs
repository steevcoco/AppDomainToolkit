using System;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	public class RemoteActionUnitTests
	{
		private class Test : MarshalByRefObject
		{
			public int Value1 { get; set; }

			public short Value2 { get; set; }

			public double? Value3 { get; set; }

			public Composite Value4 { get; set; }
		}


		private class Composite : MarshalByRefObject
		{
			public short Value { get; set; }
		}


		[Fact]
		public void Invoke_InstanceFourTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteAction<int, int, int, int> action = new RemoteAction<int, int, int, int>();
					action.Invoke(1, 2, 3, 4, null);
				});

		[Fact]
		public void Invoke_InstanceNoTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteAction action = new RemoteAction();
					action.Invoke(null);
				});

		[Fact]
		public void Invoke_InstanceOneType_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteAction<int> action = new RemoteAction<int>();
					action.Invoke(1, null);
				});

		[Fact]
		public void Invoke_InstanceThreeTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteAction<int, int, int> action = new RemoteAction<int, int, int>();
					action.Invoke(1, 2, 3, null);
				});

		[Fact]
		public void Invoke_InstanceTwoTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteAction<int, int> action = new RemoteAction<int, int>();
					action.Invoke(1, 2, null);
				});

		[Fact]
		public void Invoke_MarshalObjectByRef_FourArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = new Test();
				RemoteAction.Invoke(
						context.Domain,
						actual,
						(short) 11,
						new double?(12.0),
						new Composite {Value = 13},
						(test, value2, value3, value4) => {
							test.Value1 = 10;
							test.Value2 = value2;
							test.Value3 = value3;
							test.Value4 = value4;
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
				Assert.Equal(12.0, actual.Value3);

				Assert.NotNull(actual.Value4);
				Assert.Equal(13, actual.Value4.Value);
			}
		}

		[Fact]
		public void Invoke_MarshalObjectByRef_NoArguments() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				RemoteAction.Invoke(context.Domain, () => { });
			}
		}

		[Fact]
		public void Invoke_MarshalObjectByRef_OneArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = new Test();
				RemoteAction.Invoke(
						context.Domain,
						actual,
						test => { test.Value1 = 10; });

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}

		[Fact]
		public void Invoke_MarshalObjectByRef_ThreeArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = new Test();
				RemoteAction.Invoke(
						context.Domain,
						actual,
						(short) 11,
						new double?(12.0),
						(test, value2, value3) => {
							test.Value1 = 10;
							test.Value2 = value2;
							test.Value3 = value3;
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
				Assert.Equal(12.0, actual.Value3);
			}
		}

		[Fact]
		public void Invoke_MarshalObjectByRef_TwoArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = new Test();
				RemoteAction.Invoke(
						context.Domain,
						actual,
						(short) 11,
						(test, value2) => {
							test.Value1 = 10;
							test.Value2 = value2;
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
			}
		}

		[Fact]
		public void Invoke_NullDomain()
			=> Assert.Throws(typeof(ArgumentNullException), () => { RemoteAction.Invoke(null, () => { }); });

		[Fact]
		public void Invoke_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
						RemoteAction.Invoke(context.Domain, null);
					}
				});
	}
}
