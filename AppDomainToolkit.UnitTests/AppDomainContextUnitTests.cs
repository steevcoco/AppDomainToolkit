using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;


namespace AppDomainToolkit.UnitTests
{
	//[DeploymentItem(@"test-assembly-files\", @"test-assembly-files\")]
	public class AppDomainContextUnitTests
	{
		private static readonly string testAssemblyDir = @"test-assembly-files\";
		private static readonly string noRefsAssemblyName = @"TestWithNoReferences";

		private static readonly string noRefsAssemblyFileName = string.Format(
				"{0}{1}",
				AppDomainContextUnitTests.noRefsAssemblyName,
				@".dll");

		private static readonly string noRefsAssemblyPath = Path.Combine(
				AppDomainContextUnitTests.testAssemblyDir,
				AppDomainContextUnitTests.noRefsAssemblyFileName);

		private static readonly string internalRefsAssemblyDir = Path.Combine(
				AppDomainContextUnitTests.testAssemblyDir,
				"test-with-internal-references");

		private static readonly string internalRefsAssemblyName = @"TestWithInternalReferences";

		private static readonly string internalRefsAssemblyFileName = string.Format(
				"{0}{1}",
				AppDomainContextUnitTests.internalRefsAssemblyName,
				@".dll");

		private static readonly string internalRefsAssemblyPath =
				Path.Combine(
						AppDomainContextUnitTests.internalRefsAssemblyDir,
						AppDomainContextUnitTests.internalRefsAssemblyFileName);

		private static readonly string assemblyAName = "AssemblyA";
		private static readonly string assemblyAFileName = string.Format("{0}.dll", AppDomainContextUnitTests.assemblyAName);
		private static readonly string assemblyBName = "AssemblyB";
		private static readonly string assemblyBFileName = string.Format("{0}.dll", AppDomainContextUnitTests.assemblyBName);

		[Fact]
		public void Create_NoApplicationNameSupplied() {
			string workingDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			AppDomainSetup setupInfo = new AppDomainSetup {
				ApplicationBase = workingDir,
				PrivateBinPath = workingDir
			};

			IAppDomainContext target = AppDomainContext.Create(setupInfo);

			Assert.NotNull(target);
			Assert.NotNull(target.Domain);
			Assert.NotNull(target.UniqueId);
			Assert.NotNull(target.RemoteResolver);
			Assert.NotEqual(AppDomain.CurrentDomain, target.Domain);

			// Verify the app domain's setup info
			Assert.False(string.IsNullOrEmpty(target.Domain.SetupInformation.ApplicationName));
			Assert.Equal(setupInfo.ApplicationBase, setupInfo.ApplicationBase);
			Assert.Equal(setupInfo.PrivateBinPath, target.Domain.SetupInformation.PrivateBinPath);
		}

		[Fact]
		public void Create_NoApplicationNameSupplied_WrappedDomain() {
			IAppDomainContext target = AppDomainContext.Wrap(AppDomain.CurrentDomain);

			Assert.NotNull(target);
			Assert.NotNull(target.Domain);
			Assert.NotNull(target.UniqueId);
			Assert.NotNull(target.RemoteResolver);
			Assert.Equal(AppDomain.CurrentDomain, target.Domain);

			// Verify the app domain's setup info
			Assert.False(string.IsNullOrEmpty(target.Domain.SetupInformation.ApplicationName));
		}

		[Fact]
		public void Create_NoArgs_ValidContext() {
			IAppDomainContext target = AppDomainContext.Create();

			Assert.NotNull(target);
			Assert.NotNull(target.Domain);
			Assert.NotNull(target.UniqueId);
			Assert.NotNull(target.RemoteResolver);
			Assert.NotEqual(AppDomain.CurrentDomain, target.Domain);
		}

		[Fact]
		public void Create_NullAppDomainSetupInfo() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					IAppDomainContext context = AppDomainContext.Create(null);
				});

		[Fact]
		public void Create_ValidSetupInfo() {
			string workingDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			AppDomainSetup setupInfo = new AppDomainSetup {
				ApplicationName = "My app",
				ApplicationBase = workingDir,
				PrivateBinPath = workingDir
			};

			IAppDomainContext target = AppDomainContext.Create(setupInfo);

			Assert.NotNull(target);
			Assert.NotNull(target.Domain);
			Assert.NotNull(target.UniqueId);
			Assert.NotNull(target.RemoteResolver);
			Assert.NotEqual(AppDomain.CurrentDomain, target.Domain);

			// Verify the app domain's setup info
			Assert.Equal(setupInfo.ApplicationName, target.Domain.SetupInformation.ApplicationName, true);
			Assert.Equal(setupInfo.ApplicationBase, setupInfo.ApplicationBase);
			Assert.Equal(setupInfo.PrivateBinPath, target.Domain.SetupInformation.PrivateBinPath);
		}

		[Fact]
		public void Dispose_DomainProperty() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					IAppDomainContext target = AppDomainContext.Create();
					target.Dispose();

					Assert.True(target.IsDisposed);
					AppDomain domain = target.Domain;
				});

		[Fact]
		public void Dispose_LoadedAssembliesProperty() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					IAppDomainContext target = AppDomainContext.Create();
					target.Dispose();

					Assert.True(target.IsDisposed);
					IEnumerable<IAssemblyTarget> assemblies = target.LoadedAssemblies;
				});

		[Fact]
		public void Dispose_RemoteResolverPropery() => Assert.Throws(
				typeof(ObjectDisposedException),
				() => {
					IAppDomainContext target = AppDomainContext.Create();
					target.Dispose();

					Assert.True(target.IsDisposed);
					IAssemblyResolver resolver = target.RemoteResolver;
				});

		[Fact]
		public void Dispose_WithUsingClause() {
			IAppDomainContext target;
			using (target = AppDomainContext.Create()) {
				Assert.NotNull(target);
				Assert.NotNull(target.Domain);
				Assert.NotNull(target.UniqueId);
				Assert.NotNull(target.RemoteResolver);
				Assert.NotEqual(AppDomain.CurrentDomain, target.Domain);
				Assert.False(target.IsDisposed);
			}

			Assert.True(target.IsDisposed);
		}

		[Fact]
		public void FindByCodeBase_NoRefAssembly_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Uri codebaseUri = new Uri(targetPath);
				context.LoadAssembly(LoadMethod.LoadFrom, targetPath);
				Assert.NotNull(context.FindByCodeBase(codebaseUri));
			}
		}

		[Fact]
		public void FindByCodeBase_NullArgument() => Assert.Throws(
				typeof(ArgumentNullException),
				() => {
					using (IAppDomainContext context = AppDomainContext.Create()) {
						context.FindByCodeBase(null);
					}
				});

		[Fact]
		public void FindByFullName_NoRefAssembly_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				IAssemblyTarget target = context.LoadAssembly(LoadMethod.LoadFrom, targetPath);
				Assert.NotNull(context.FindByFullName(target.AssemblyName.FullName));
			}
		}

		[Fact]
		public void FindByFullName_NullArgument() => Assert.Throws(
				typeof(ArgumentException),
				() => {
					using (IAppDomainContext context = AppDomainContext.Create()) {
						context.FindByFullName(null);
					}
				});

		[Fact]
		public void FindByLocation_NoRefAssembly_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				IAssemblyTarget target = context.LoadAssembly(LoadMethod.LoadFrom, targetPath);
				Assert.NotNull(context.FindByLocation(target.Location));
			}
		}

		[Fact]
		public void FindByLocation_NullArgument() => Assert.Throws(
				typeof(ArgumentException),
				() => {
					using (IAppDomainContext context = AppDomainContext.Create()) {
						context.FindByLocation(null);
					}
				});

		[Fact]
		public void LoadAssembly_NoRefAssembly_LoadBitsNoPdbSpecified() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Uri codebaseUri = new Uri(targetPath);
				IAssemblyTarget target = context.LoadAssembly(LoadMethod.LoadBits, targetPath);
				Assert.True(context.LoadedAssemblies.Any(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName)));
			}
		}

		[Fact]
		public void LoadAssembly_NoRefAssembly_LoadBitsPdbSpecified() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Uri codebaseUri = new Uri(targetPath);
				IAssemblyTarget target = context.LoadAssembly(
						LoadMethod.LoadBits,
						targetPath,
						Path.ChangeExtension(targetPath, "pdb"));
				Assert.True(context.LoadedAssemblies.Any(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName)));
			}
		}

		[Fact]
		public void LoadAssembly_NoRefAssembly_LoadBitsWrongPdbPathSpecified() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				string pdbPath =
						Path.ChangeExtension(
								Path.Combine(
										AppDomainContextUnitTests.testAssemblyDir,
										Guid.NewGuid().ToString(),
										AppDomainContextUnitTests.noRefsAssemblyFileName),
								"pdb");
				Uri codebaseUri = new Uri(targetPath);
				IAssemblyTarget target = context.LoadAssembly(LoadMethod.LoadBits, targetPath, Path.GetFullPath(pdbPath));
				Assert.True(context.LoadedAssemblies.Any(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName)));
			}
		}

		[Fact]
		public void LoadAssembly_NoRefAssembly_LoadFile() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Uri codebaseUri = new Uri(targetPath);
				context.LoadAssembly(LoadMethod.LoadFile, targetPath);
				Assert.True(context.LoadedAssemblies.Any(x => x.CodeBase.Equals(codebaseUri.ToString())));
			}
		}

		[Fact]
		public void LoadAssembly_NoRefAssembly_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Uri codebaseUri = new Uri(targetPath);
				context.LoadAssembly(LoadMethod.LoadFrom, targetPath);
				Assert.True(context.LoadedAssemblies.Any(x => x.CodeBase.Equals(codebaseUri.ToString())));
			}
		}

		[Fact]
		public void LoadAssemblyWithReferences_InternalReferences_LoadBitsNoPdbSpecified() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				IEnumerable<IAssemblyTarget> targets = context.LoadAssemblyWithReferences(LoadMethod.LoadBits, targetPath);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}

		[Fact]
		public void LoadAssemblyWithReferences_InternalReferences_LoadFile() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				IEnumerable<IAssemblyTarget> targets = context.LoadAssemblyWithReferences(LoadMethod.LoadFile, targetPath);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.Location.Equals(targetPath)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}

		[Fact]
		public void LoadAssemblyWithReferences_InternalReferences_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				IEnumerable<IAssemblyTarget> targets = context.LoadAssemblyWithReferences(LoadMethod.LoadFrom, targetPath);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.Location.Equals(targetPath)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}

		[Fact]
		public void LoadTarget_NoRefAssembly_LoadBits() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				context.LoadTarget(LoadMethod.LoadBits, target);
				IAssemblyTarget actual = context.LoadedAssemblies.FirstOrDefault(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName));

				Assert.NotNull(actual);
				Assert.Equal(target.AssemblyName.FullName, actual.AssemblyName.FullName);
				Assert.Equal(string.Empty, actual.Location);
				Assert.Equal(target.CodeBase, target.CodeBase);
			}
		}

		[Fact]
		public void LoadTarget_NoRefAssembly_LoadFile() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				context.LoadTarget(LoadMethod.LoadFile, target);
				IAssemblyTarget actual = context.LoadedAssemblies.FirstOrDefault(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName));

				Assert.NotNull(actual);
				Assert.Equal(target.AssemblyName.FullName, actual.AssemblyName.FullName);
				Assert.Equal(target.Location, actual.Location);
				Assert.Equal(target.CodeBase, target.CodeBase);
			}
		}

		[Fact]
		public void LoadTarget_NoRefAssembly_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.noRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				context.LoadTarget(LoadMethod.LoadFrom, target);
				IAssemblyTarget actual = context.LoadedAssemblies.FirstOrDefault(x => x.AssemblyName.FullName.Equals(target.AssemblyName.FullName));

				Assert.NotNull(actual);
				Assert.Equal(target.AssemblyName.FullName, actual.AssemblyName.FullName);
				Assert.Equal(target.Location, actual.Location);
				Assert.Equal(target.CodeBase, target.CodeBase);
			}
		}

		[Fact]
		public void LoadTargetWithReferences_InternalReferences_LoadBitsNoPdbSpecified() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				IEnumerable<IAssemblyTarget> targets = context.LoadTargetWithReferences(LoadMethod.LoadBits, target);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}

		[Fact]
		public void LoadTargetWithReferences_InternalReferences_LoadFile() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				IEnumerable<IAssemblyTarget> targets = context.LoadTargetWithReferences(LoadMethod.LoadFile, target);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.Location.Equals(targetPath)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}

		[Fact]
		public void LoadTargetWithReferences_InternalReferences_LoadFrom() {
			using (IAppDomainContext context = AppDomainContext.Create()) {
				int prevNumAssemblies = context.LoadedAssemblies.Count();

				// Add the correct resolver path for the test dir.
				context.RemoteResolver.AddProbePath(Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyDir));
				string targetPath = Path.GetFullPath(AppDomainContextUnitTests.internalRefsAssemblyPath);
				Assembly assembly = Assembly.LoadFile(targetPath);
				IAssemblyTarget target = AssemblyTarget.FromAssembly(assembly);

				IEnumerable<IAssemblyTarget> targets = context.LoadTargetWithReferences(LoadMethod.LoadFrom, target);

				Assert.True(context.LoadedAssemblies.Count() > prevNumAssemblies);
				Assert.True(targets.Any(x => x.Location.Equals(targetPath)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.internalRefsAssemblyName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyAName)));
				Assert.True(targets.Any(x => x.AssemblyName.FullName.Contains(AppDomainContextUnitTests.assemblyBName)));
			}
		}
	}
}
