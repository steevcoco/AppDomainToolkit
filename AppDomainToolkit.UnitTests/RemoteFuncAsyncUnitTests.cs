using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	public class RemoteFuncAsyncUnitTests
	{
		[Serializable]
		private class Test
		{
			public int Value1 { get; set; }

			public short Value2 { get; set; }

			public double? Value3 { get; set; }

			public Composite Value4 { get; set; }

			public string Value5 { get; set; }
		}


		[Serializable]
		private class Composite
		{
			public short Value { get; set; }
		}


		[Fact]
		public async Task Delay_Long() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						async value => {
							await Task.Delay(1000);
							return new Test {Value1 = value};
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}

		[Fact]
		public async Task Delay_Short() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						async value => {
							await Task.Delay(100);
							return new Test {Value1 = value};
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}

		[Fact]
		public async Task EmptyFunction() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				int actual = await RemoteFuncAsync.InvokeAsync(context.Domain, async () => { return 1; });
				Assert.Equal(1, actual);
			}
		}

		[Fact]
		public async Task Exception() => await Assert.ThrowsAsync(
				typeof(AggregateException),
				async () => {
					using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
						Test actual = null;

						CancellationTokenSource tcs = new CancellationTokenSource();
						actual = await RemoteFuncAsync.InvokeAsync(
								context.Domain,
								10,
								(short) 11,
								new double?(12.0),
								new Composite {Value = 13},
								"Last",
								async (value1, value2, value3, value4, value5) => {
									if (value5 == "Last")
										throw new Exception("");
									return new Test {Value1 = value1, Value2 = value2, Value3 = value3, Value4 = value4, Value5 = value5};
								});

						Assert.NotNull(actual);
						Assert.Equal(10, actual.Value1);
						Assert.Equal(11, actual.Value2);
						Assert.Equal(12, actual.Value3);
						Assert.NotNull(actual.Value4);
						Assert.Equal("Last", actual.Value5);
						Assert.Equal(13, actual.Value4.Value);
					}
				});

		[Fact]
		public void InstanceFiveTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteFuncAsync<int, int, int, int, int> action = new RemoteFuncAsync<int, int, int, int, int>();
					action.Invoke(1, 2, 3, 4, null, null);
				});

		[Fact]
		public void InstanceFourTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteFuncAsync<int, int, int, int> action = new RemoteFuncAsync<int, int, int, int>();
					action.Invoke(1, 2, 3, null, null);
				});

		[Fact]
		public void InstanceOneType_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteFuncAsync<int> action = new RemoteFuncAsync<int>();
					action.Invoke(null, null);
				});

		[Fact]
		public void InstanceThreeTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteFuncAsync<int, int, int> action = new RemoteFuncAsync<int, int, int>();
					action.Invoke(1, 2, null, null);
				});

		[Fact]
		public void InstanceTwoTypes_NullFunction() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					RemoteFuncAsync<int, int> action = new RemoteFuncAsync<int, int>();
					action.Invoke(1, null, null);
				});

		[Fact]
		public async Task NullDomain()
			=>
					await Assert.ThrowsAsync(
							typeof(ArgumentNullException),
							async () => { await RemoteFuncAsync.InvokeAsync(null, async () => { return 1; }); });

		[Fact]
		public async Task NullFunction() => await Assert.ThrowsAsync(
				typeof(ArgumentNullException),
				async () => {
					using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
						int actual = await RemoteFuncAsync.InvokeAsync<int>(context.Domain, null);
					}
				});

		[Fact]
		public async Task Serializable_FiveArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						(short) 11,
						new double?(12.0),
						new Composite {Value = 13},
						"Last",
						async (value1, value2, value3, value4, value5) =>
							new Test {Value1 = value1, Value2 = value2, Value3 = value3, Value4 = value4, Value5 = value5});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
				Assert.Equal(12, actual.Value3);
				Assert.NotNull(actual.Value4);
				Assert.Equal("Last", actual.Value5);
				Assert.Equal(13, actual.Value4.Value);
			}
		}

		[Fact]
		public async Task Serializable_FourArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						(short) 11,
						new double?(12.0),
						new Composite {Value = 13},
						async (value1, value2, value3, value4) => {
							return new Test {Value1 = value1, Value2 = value2, Value3 = value3, Value4 = value4};
						});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
				Assert.Equal(12, actual.Value3);

				Assert.NotNull(actual.Value4);
				Assert.Equal(13, actual.Value4.Value);
			}
		}

		[Fact]
		public async Task Serializable_NoArguments() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						async () => new Test {Value1 = 10});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}

		[Fact]
		public async Task Serializable_OneArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						async value => new Test {Value1 = value});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}

		[Fact]
		public async Task Serializable_ThreeArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						(short) 11,
						new double?(12),
						async (value1, value2, value3) => new Test {Value1 = value1, Value2 = value2, Value3 = value3});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
				Assert.Equal(12, actual.Value3);
			}
		}

		[Fact]
		public async Task Serializable_TwoArg() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						(short) 11,
						async (value1, value2) => new Test {Value1 = value1, Value2 = value2});

				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
				Assert.Equal(11, actual.Value2);
			}
		}

		[Fact]
		public async Task TaskYields() {
			using (AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> context = AppDomainContext.Create()) {
				await Task.Yield();
				Test actual = await RemoteFuncAsync.InvokeAsync(
						context.Domain,
						10,
						async value => {
							await Task.Yield();
							Test test = new Test {Value1 = value};
							await Task.Yield();
							return test;
						});
				await Task.Yield();
				Assert.NotNull(actual);
				Assert.Equal(10, actual.Value1);
			}
		}
	}
}
