using System;


namespace AppDomainToolkit
{
	/// <summary>
	/// This is a thin wrapper around the .NET AppDomain class that enables safe disposal. Use
	/// these objects where you would normally grab an AppDomain object. Note that if the
	/// current application domain is passed to this class, a call to Dispose will do nothing. We will
	/// never unload the current application domain.
	/// </summary>
	internal sealed class DisposableAppDomain : IIsDisposable
	{
		private AppDomain domain;


		/// <summary>
		/// Initializes a new instance of the DisposableAppDomain class.
		/// </summary>
		/// <param name="domain">
		/// The domain to wrap.
		/// </param>
		public DisposableAppDomain(AppDomain domain) {
			this.domain = domain ?? throw new ArgumentNullException(nameof(domain));
			IsDisposed = false;
		}

		/// <summary>
		/// Gets the wrapped application domain.
		/// </summary>
		public AppDomain Domain {
			get {
				if (IsDisposed)
					throw new ObjectDisposedException("The AppDomain has been unloaded or disposed!");
				return domain;
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
					// Do *not* unload the current app domain.
					if (!((Domain == null) || Domain.Equals(AppDomain.CurrentDomain)))
						AppDomain.Unload(Domain);
					domain = null;
				}
			}
			IsDisposed = true;
		}

		/// <summary>
		/// Finalizes an instance of the DisposableAppDomain class.
		/// </summary>
		~DisposableAppDomain() {
			onDispose(false);
		}
	}
}
