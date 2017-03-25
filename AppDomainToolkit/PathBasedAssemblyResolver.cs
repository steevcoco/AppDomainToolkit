using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace AppDomainToolkit
{
	/// <summary>
	/// Handles resolving assemblies in application domains. This class is helpful when attempting to load a
	/// particular assembly into an application domain and the assembly you're looking for doesn't exist in the
	/// main application bin path. This 'feature' of the .NET framework makes assembly loading very, very
	/// irritating, but this little helper class should alleviate much of the pains here. Note that it extends
	/// MarshalByRefObject, so it can be remoted into another application domain. Paths to directories containing
	/// assembly files that you wish to load should be added to an instance of this class, and then the Resolve
	/// method should be assigned to the AssemblyResolve event on the target application domain.
	/// </summary>
	public class PathBasedAssemblyResolver : MarshalByRefObject, IAssemblyResolver
	{
		private readonly IAssemblyLoader loader;
		private readonly HashSet<string> probePaths;

		/// <inheritdoc />
		private string applicationBase;

		/// <inheritdoc />
		private string privateBinPath;


		/// <summary>
		/// Initializes a new instance of the PathBasedAssemblyResolver class. Exists for MarshalByRefObject
		/// remoting into app domains.
		/// </summary>
		public PathBasedAssemblyResolver() : this(null) {}

		/// <summary>
		/// Initializes a new instance of the AssemblyResolver class. A default instance of this class will resolve
		/// assemblies into the LoadFrom context.
		/// </summary>
		/// <param name="loader">
		/// The loader to use when loading assemblies. Default is null, which will create and use an instance
		/// of the RemotableAssemblyLoader class.
		/// </param>
		/// <param name="loadMethod">
		/// The load method to use when loading assemblies. Defaults to LoadMethod.LoadFrom.
		/// </param>
		public PathBasedAssemblyResolver(
				IAssemblyLoader loader = null,
				LoadMethod loadMethod = LoadMethod.LoadFrom) {
			probePaths = new HashSet<string>();
			this.loader = loader ?? new AssemblyLoader();
			LoadMethod = loadMethod;
		}


		/// <inheritdoc />
		public LoadMethod LoadMethod { get; set; }

		/// <inheritdoc />
		public string ApplicationBase {
			get { return applicationBase; }
			set {
				applicationBase = value;
				AddProbePath(value);
			}
		}

		/// <inheritdoc />
		public string PrivateBinPath {
			get { return privateBinPath; }
			set {
				privateBinPath = value;
				AddProbePath(value);
			}
		}


		/// <inheritdoc />
		public void AddProbePath(string path) {
			if (string.IsNullOrEmpty(path))
				return;
			if (path.Contains(";")) {
				string[] paths = path.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
				AddProbePaths(paths);
			} else
				AddProbePaths(path);
		}

		/// <inheritdoc />
		public void AddProbePaths(params string[] paths) {
			foreach (string path in paths) {
				if (string.IsNullOrEmpty(path))
					continue;
				DirectoryInfo dir = new DirectoryInfo(path);
				if (!probePaths.Contains(dir.FullName))
					probePaths.Add(dir.FullName);
			}
		}

		/// <inheritdoc />
		public Assembly Resolve(object sender, ResolveEventArgs args) {
			AssemblyName name = new AssemblyName(args.Name);
			foreach (string path in probePaths) {
				string dllPath = Path.Combine(path, string.Format("{0}.dll", name.Name));
				if (File.Exists(dllPath))
					return loader.LoadAssembly(LoadMethod, dllPath);
				string exePath = Path.ChangeExtension(dllPath, "exe");
				if (File.Exists(exePath))
					return loader.LoadAssembly(LoadMethod, exePath);
			}

			// Not found.
			return null;
		}

		/// <summary>
		/// Removes the given probe path or semicolon separated list of probe paths from the assembly loader.
		/// </summary>
		/// <param name="path">The path to remove.</param>
		public void RemoveProbePath(string path) {
			if (string.IsNullOrEmpty(path))
				return;
			if (path.Contains(";")) {
				string[] paths = path.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
				RemoveProbePaths(paths);
			} else
				RemoveProbePaths(path);
		}

		/// <summary>
		/// Removes the given probe paths from the assembly loader.
		/// </summary>
		/// <param name="paths">The paths to remove.</param>
		public void RemoveProbePaths(params string[] paths) {
			foreach (DirectoryInfo dir in from path in paths
					where !string.IsNullOrEmpty(path)
					select new DirectoryInfo(path)) {
				probePaths.Remove(dir.FullName);
			}
		}
	}
}
