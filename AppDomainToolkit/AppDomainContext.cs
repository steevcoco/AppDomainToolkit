using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace AppDomainToolkit
{
	/// <summary>
	/// Loads assemblies into the contained application domain. Just a no-hassle wrapper
	/// for creating default instances of
	/// <see cref="AppDomainContext{TAssemblyTargetLoader,TAssemblyResolver}" />
	/// </summary>
	public static class AppDomainContext
	{
		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <returns>
		/// A new AppDomainContext.
		/// </returns>
		public static IAppDomainContext Create()
			=> AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>.Create();

		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <param name="setupInfo">
		/// The setup info.
		/// </param>
		/// <returns>
		/// A new AppDomainContext.
		/// </returns>
		public static IAppDomainContext Create(AppDomainSetup setupInfo)
			=> AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>.Create(setupInfo);

		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <param name="domain">The domain to wrap in the context</param>
		/// <returns></returns>
		public static IAppDomainContext Wrap(AppDomain domain)
			=> AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>.Wrap(domain);
	}


	/// <summary>
	/// Loads assemblies into the contained application domain.
	/// </summary>
	public sealed class AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver> : IAppDomainContext
		where TAssemblyTargetLoader : MarshalByRefObject, IAssemblyTargetLoader, new()
		where TAssemblyResolver : MarshalByRefObject, IAssemblyResolver, new()
	{
		private static AppDomain createDomain(AppDomainSetup setup, string name)
			=> AppDomain.CreateDomain(name, null, setup);


		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <returns>
		/// A new AppDomainContext.
		/// </returns>
		public static AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver> Create() {
			Guid guid = Guid.NewGuid();
			//string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string rootDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			AppDomainSetup setupInfo = new AppDomainSetup {
				ApplicationName = "Temp-Domain-" + guid,
				ApplicationBase = rootDir,
				PrivateBinPath = rootDir
			};
			return new AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver>(setupInfo) { UniqueId = guid };
		}

		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <param name="setupInfo">
		/// The setup info.
		/// </param>
		/// <returns>
		/// A new AppDomainContext.
		/// </returns>
		public static AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver> Create(AppDomainSetup setupInfo) {
			if (setupInfo == null)
				throw new ArgumentNullException(nameof(setupInfo));
			Guid guid = Guid.NewGuid();
			setupInfo.ApplicationName
				= string.IsNullOrEmpty(setupInfo.ApplicationName)
					? "Temp-Domain-" + guid
					: setupInfo.ApplicationName;
			return new AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver>(setupInfo) { UniqueId = guid };
		}

		/// <summary>
		/// Creates a new instance of the AppDomainContext class.
		/// </summary>
		/// <param name="domain">The appdomain to wrap.</param>
		/// <returns>A new AppDomainContext.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver> Wrap(AppDomain domain) {
			if (domain == null)
				throw new ArgumentNullException(nameof(domain));
			return new AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver>(domain);
		}


		private readonly Remote<TAssemblyTargetLoader> loaderProxy;
		private readonly Remote<TAssemblyResolver> resolverProxy;


		private readonly DisposableAppDomain wrappedDomain;


		/// <summary>
		/// Initializes a new instance of the AppDomainAssemblyLoader class. The assembly environment will create
		/// a new application domain with the location of the currently executing assembly as the application base. It
		/// will also add that root directory to the assembly resolver's path in order to properly load a remotable
		/// AssemblyLoader object into context. From here, add whatever assembly probe paths you wish in order to
		/// resolve remote proxies, or extend this class if you desire more specific behavior.
		/// </summary>
		/// <param name="setupInfo">
		/// The setup information.
		/// </param>
		private AppDomainContext(AppDomainSetup setupInfo)
			: this(setupInfo, AppDomainContext<TAssemblyTargetLoader, TAssemblyResolver>.createDomain) {}

		/// <summary>
		/// Initializes a new instance of the AppDomainContext class. The new AppDomainContext will wrap the given domain
		/// </summary>
		/// <param name="domain">Not null.</param>
		private AppDomainContext(AppDomain domain)
			: this(domain.SetupInformation, (setup, friendlyName) => domain) {}

		private AppDomainContext(AppDomainSetup setupInfo, Func<AppDomainSetup, string, AppDomain> createDomain) {
			UniqueId = Guid.NewGuid();
			AssemblyImporter = new TAssemblyResolver {
				ApplicationBase = setupInfo.ApplicationBase,
				PrivateBinPath = setupInfo.PrivateBinPath
			};

			// Add some root directories to resolve some required assemblies
			// Create the new domain and wrap it for disposal.
			wrappedDomain = new DisposableAppDomain(createDomain(setupInfo, UniqueId.ToString()));

			AppDomain.CurrentDomain.AssemblyResolve += AssemblyImporter.Resolve;

			// Create remotes
			loaderProxy = Remote<TAssemblyTargetLoader>.CreateProxy(wrappedDomain);
			resolverProxy = Remote<TAssemblyResolver>.CreateProxy(wrappedDomain);

			// Assign the resolver in the other domain (just to be safe)
			RemoteAction.Invoke(
					wrappedDomain.Domain,
					resolverProxy.RemoteObject,
					resolver => { AppDomain.CurrentDomain.AssemblyResolve += resolver.Resolve; });

			// Assign proper paths to the remote resolver
			resolverProxy.RemoteObject.ApplicationBase = setupInfo.ApplicationBase;
			resolverProxy.RemoteObject.PrivateBinPath = setupInfo.PrivateBinPath;

			IsDisposed = false;
		}


		/// <inheritdoc />
		public Guid UniqueId { get; private set; }

		/// <inheritdoc />
		public AppDomain Domain {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				return wrappedDomain.Domain;
			}
		}

		/// <inheritdoc />
		public IAssemblyResolver RemoteResolver {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				return resolverProxy.RemoteObject;
			}
		}

		/// <inheritdoc />
		public IAssemblyResolver AssemblyImporter { get; private set; }

		/// <inheritdoc />
		/// <remarks>
		/// This property hits the remote AppDomain each time you ask for it, so don't call this in a
		/// tight loop unless you like slow code.
		/// </remarks>
		public IEnumerable<IAssemblyTarget> LoadedAssemblies {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				IAssemblyTarget[] rValue = loaderProxy.RemoteObject.GetAssemblies();
				return rValue;
			}
		}


		/// <inheritdoc />
		public IAssemblyTarget FindByCodeBase(Uri codebaseUri) {
			if (codebaseUri == null)
				throw new ArgumentNullException(nameof(codebaseUri));
			return LoadedAssemblies.FirstOrDefault(x => x.CodeBase.Equals(codebaseUri));
		}

		/// <inheritdoc />
		public IAssemblyTarget FindByLocation(string location) {
			if (string.IsNullOrEmpty(location))
				throw new ArgumentException("Location cannot be null or empty");
			return LoadedAssemblies.FirstOrDefault(x => x.Location.Equals(location));
		}

		/// <inheritdoc />
		public IAssemblyTarget FindByFullName(string fullname) {
			if (string.IsNullOrEmpty(fullname))
				throw new ArgumentException("Full name cannot be null or empty!");
			return LoadedAssemblies.FirstOrDefault(x => x.AssemblyName.FullName.Equals(fullname));
		}

		/// <inheritdoc />
		public IAssemblyTarget LoadTarget(LoadMethod loadMethod, IAssemblyTarget target)
			=> LoadAssembly(loadMethod, target.CodeBase.LocalPath);

		/// <inheritdoc />
		public IEnumerable<IAssemblyTarget> LoadTargetWithReferences(LoadMethod loadMethod, IAssemblyTarget target)
			=> LoadAssemblyWithReferences(loadMethod, target.CodeBase.LocalPath);

		/// <inheritdoc />
		/// <remarks>
		/// In order to ensure that the assembly is loaded the way the caller expects, the LoadMethod property of
		/// the remote domain assembly resolver will be temporarily set to the value of <paramref name="loadMethod" />.
		/// It will be reset to the original value after the load is complete.
		/// </remarks>
		public IAssemblyTarget LoadAssembly(LoadMethod loadMethod, string path, string pdbPath = null) {
			LoadMethod previousLoadMethod = resolverProxy.RemoteObject.LoadMethod;
			resolverProxy.RemoteObject.LoadMethod = loadMethod;
			IAssemblyTarget target = loaderProxy.RemoteObject.LoadAssembly(loadMethod, path, pdbPath);
			resolverProxy.RemoteObject.LoadMethod = previousLoadMethod;
			return target;
		}

		/// <inheritdoc />
		/// <remarks>
		/// In order to ensure that the assembly is loaded the way the caller expects, the LoadMethod property of
		/// the remote domain assembly resolver will be temporarily set to the value of <paramref name="loadMethod" />.
		/// It will be reset to the original value after the load is complete.
		/// </remarks>
		public IEnumerable<IAssemblyTarget> LoadAssemblyWithReferences(LoadMethod loadMethod, string path) {
			LoadMethod previousLoadMethod = resolverProxy.RemoteObject.LoadMethod;
			resolverProxy.RemoteObject.LoadMethod = loadMethod;
			IList<IAssemblyTarget> targets = loaderProxy.RemoteObject.LoadAssemblyWithReferences(loadMethod, path);
			resolverProxy.RemoteObject.LoadMethod = previousLoadMethod;
			return targets;
		}


		/// <inheritdoc />
		public bool IsDisposed { get; private set; }


		/// <inheritdoc />
		public void Dispose() {
			GC.SuppressFinalize(this);
			onDispose(true);
		}

		private void onDispose(bool disposing) {
			if (disposing) {
				if (!IsDisposed) {
					if (!wrappedDomain.IsDisposed)
						wrappedDomain.Dispose();
					if (!loaderProxy.IsDisposed)
						loaderProxy.Dispose();
					AssemblyImporter = null;
				}
			}
			IsDisposed = true;
		}

		/// <summary>
		/// Finalizes an instance of the AppDomainContext class.
		/// </summary>
		~AppDomainContext() {
			onDispose(false);
		}
	}
}
