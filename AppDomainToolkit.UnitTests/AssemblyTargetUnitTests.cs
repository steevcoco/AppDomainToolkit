using System;
using System.IO;
using System.Reflection;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	public class AssemblyTargetUnitTests
	{
		[Fact]
		public void FromAssembly_CurrentAssembly() {
			Assembly assembly = Assembly.GetExecutingAssembly();
			IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

			Assert.NotNull(target);
			Assert.Equal(assembly.CodeBase, target.CodeBase.ToString());
			Assert.Equal(assembly.Location, target.Location);
			Assert.Equal(assembly.FullName, target.AssemblyName.FullName);
		}

		[Fact]
		public void FromAssembly_NullArgument() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					IAssemblyTarget target = AssemblyTarget.FromAssembly(null);
				});

		[Fact]
		public void FromPath_CurrentAssembly() {
			Assembly assembly = Assembly.GetExecutingAssembly();
			IAssemblyTarget target = AssemblyTarget.FromPath(new Uri(assembly.CodeBase), assembly.Location, assembly.GetName());

			Assert.NotNull(target);
			Assert.Equal(assembly.CodeBase, target.CodeBase.ToString());
			Assert.Equal(assembly.Location, target.Location);
			Assert.Equal(assembly.FullName, target.AssemblyName.FullName);
		}

		[Fact]
		public void FromPath_NonExistingCodeBase() => Assert.Throws(
				typeof(FileNotFoundException),
				() => {
					string location = Path.GetFullPath(string.Format("{0}/{1}", Guid.NewGuid(), Path.GetRandomFileName()));
					IAssemblyTarget target = AssemblyTarget.FromPath(new Uri(location));
				});

		[Fact]
		public void FromPath_NonExistingLocationExistingCodeBase() => Assert.Throws(
				typeof(FileNotFoundException),
				() => {
					Assembly assembly = Assembly.GetExecutingAssembly();
					string location = string.Format("{0}/{1}", Guid.NewGuid(), Path.GetRandomFileName());
					IAssemblyTarget target = AssemblyTarget.FromPath(new Uri(assembly.CodeBase), location);
				});

		[Fact]
		public void FromPath_NullCodebase() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					IAssemblyTarget target = AssemblyTarget.FromPath(null);
				});
	}
}
