using System;
using System.IO;
using System.Reflection;


namespace AppDomainToolkit
{
	/// <summary>
	/// Simple class representing an assembly target. This class will be serialized accross application domains
	/// instead of remoted. There's no reason to remote it because it's simply a wrapper around a couple of
	/// strings anyway.
	/// </summary>
	[Serializable]
	public sealed class AssemblyTarget : IAssemblyTarget
	{
		/// <summary>
		/// Creates a new AssemblyTarget from the target assembly.
		/// </summary>
		/// <param name="assembly">
		/// The assembly to create the target for.
		/// </param>
		/// <returns>
		/// An AssemblyTarget.
		/// </returns>
		public static IAssemblyTarget FromAssembly(Assembly assembly) {
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			if (assembly.IsDynamic)
				return AssemblyTarget.FromDynamic(assembly.GetName());
			Uri uri = new Uri(assembly.CodeBase);
			return AssemblyTarget.FromPath(uri, assembly.Location, assembly.GetName());
		}

		/// <summary>
		/// Creates a new assembly target for the given location. The only required parameter here is the codebase.
		/// </summary>
		/// <param name="codebase">
		/// The URI to the code base.
		/// </param>
		/// <param name="location">
		/// The location. Must be a valid path and an existing file if supplied--defaults to null.
		/// </param>
		/// <param name="assemblyName">
		/// The AssemblyName. Defaults to null.
		/// </param>
		/// <returns>
		/// An AssemblyTarget.
		/// </returns>
		public static IAssemblyTarget FromPath(Uri codebase, string location = null, AssemblyName assemblyName = null) {
			if (codebase == null)
				throw new ArgumentNullException(nameof(codebase), "Codebase URI cannot be null!");
			if (!File.Exists(codebase.LocalPath))
				throw new FileNotFoundException("The target location must be an existing file!");
			if (!string.IsNullOrEmpty(location) && !File.Exists(location))
				throw new FileNotFoundException("The target location must be an existing file!");
			return new AssemblyTarget {
				CodeBase = codebase,
				Location = location,
				AssemblyName = assemblyName,
				IsDynamic = false
			};
		}

		/// <summary>
		/// Creates a new assembly target for the given dynamic assembly.
		/// </summary>
		/// <param name="assemblyName">
		/// The AssemblyName.
		/// </param>
		/// <returns>
		/// An AssemblyTarget.
		/// </returns>
		public static IAssemblyTarget FromDynamic(AssemblyName assemblyName) {
			if (assemblyName == null)
				throw new ArgumentNullException(nameof(assemblyName), "AssemblyName cannot be null!");
			return new AssemblyTarget {
				CodeBase = null,
				Location = null,
				AssemblyName = assemblyName,
				IsDynamic = true
			};
		}


		/// <summary>
		/// Prevents a default instance of the AssemblyTarget class from being created.
		/// </summary>
		private AssemblyTarget() {}


		/// <inheritdoc />
		public Uri CodeBase { get; private set; }

		/// <inheritdoc />
		public string Location { get; private set; }

		/// <inheritdoc />
		public AssemblyName AssemblyName { get; private set; }

		/// <inheritdoc />
		public bool IsDynamic { get; private set; }
	}
}
