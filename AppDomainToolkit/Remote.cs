using System;
using System.Reflection;


namespace AppDomainToolkit
{
	/// <summary>
	/// Allows for isolated creation of an object of type T imported into another
	/// application domain.
	/// </summary>
	/// <typeparam name="T">
	/// The type of object to import. Must be a deriviative of MarshalByRefObject.
	/// </typeparam>
	public sealed class Remote<T>
			: IIsDisposable
		where T : MarshalByRefObject
	{
		/// <summary>
		/// Creates a new Remote object.
		/// </summary>
		/// <param name="wrappedDomain">
		/// The target AppDomain, wrapped for disposable goodness.
		/// </param>
		/// <param name="constructorArgs">
		/// A list of constructor arguments to pass to the remote object.
		/// </param>
		/// <returns>
		/// A remote proxy to an object of type T living in the target wrapped application domain.
		/// </returns>
		internal static Remote<T> CreateProxy(DisposableAppDomain wrappedDomain, params object[] constructorArgs) {
			if (wrappedDomain == null)
				throw new ArgumentNullException(nameof(wrappedDomain));
			Type type = typeof(T);
			T proxy = (T) wrappedDomain.Domain.CreateInstanceAndUnwrap(
					type.Assembly.FullName,
					type.FullName,
					false,
					BindingFlags.CreateInstance,
					null,
					constructorArgs,
					null,
					null);
			return new Remote<T>(wrappedDomain, proxy);
		}

		/// <summary>
		/// Creates a new remote.
		/// </summary>
		/// <param name="domain">
		/// The domain for the remote. Default value is null, in which case a new application domain  that
		/// mirrors the current one will be automatically created.
		/// </param>
		/// <param name="constructorArgs">
		/// A list of constructor arguments to pass to the remote object.
		/// </param>
		/// <returns>
		/// A remote proxy to an object of type T living in the target application domain.
		/// </returns>
		public static Remote<T> CreateProxy(AppDomain domain, params object[] constructorArgs) {
			if (domain == null)
				throw new ArgumentNullException(nameof(domain));
			return Remote<T>.CreateProxy(new DisposableAppDomain(domain), constructorArgs);
		}


		private readonly DisposableAppDomain wrappedDomain;
		private T remoteObject;


		/// <summary>
		/// Initializes a new instance of the Remote class.
		/// </summary>
		/// <param name="domain">
		/// The disposable app domain where the remote object lives.
		/// </param>
		/// <param name="remoteObject">
		/// The remote object.
		/// </param>
		private Remote(DisposableAppDomain domain, T remoteObject) {
			wrappedDomain = domain;
			this.remoteObject = remoteObject;
			IsDisposed = false;
		}


		/// <summary>
		/// Gets the wrapped value.
		/// </summary>
		public T RemoteObject {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				return remoteObject;
			}
		}

		/// <summary>
		/// Gets the application domain where the wrapped value lives.
		/// </summary>
		public AppDomain Domain {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				return wrappedDomain.Domain;
			}
		}

		/// <inheritdoc />
		public bool IsDisposed { get; private set; }

		/// <inheritdoc />
		public void Dispose() {
			GC.SuppressFinalize(this);
			onDispose(true);
		}

		/// <summary>
		/// Should be called when the object is being disposed.
		/// </summary>
		/// <param name="disposing">
		/// Was Dispose() called or did we get here from the finalizer?
		/// </param>
		private void onDispose(bool disposing) {
			if (disposing) {
				if (!IsDisposed) {
					if (!wrappedDomain.IsDisposed)
						wrappedDomain.Dispose();
					remoteObject = null;
				}
			}
			IsDisposed = true;
		}

		/// <summary>
		/// Finalizes an instance of the Remote class.
		/// </summary>
		~Remote() {
			onDispose(false);
		}
	}
}
