﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace AppDomainToolkit
{
	/// <summary>
	/// This class exists to prevent DLL hell. Assemblies must be loaded into specific application domains
	/// without crossing those boundaries. We cannot simply remote an AssemblyLoader into a remote
	/// domain and load assemblies to use in the current domain. Instead, we introduct a tiny, serializable
	/// implementation of the AssemblyTarget class that handles comunication between the foreign app
	/// domain and the default one. This class is simply a wrapper around an assembly loader that translates
	/// Assembly to AssemblyTarget instances before shipping them back to the parent domain.
	/// </summary>
	public class AssemblyTargetLoader
			: MarshalByRefObject,
					IAssemblyTargetLoader
	{
		private readonly IAssemblyLoader loader;


		/// <summary>
		/// Initializes a new instance of the RemotableAssemblyLoader class. This parameterless ctor is
		/// required for remoting.
		/// </summary>
		public AssemblyTargetLoader()
			=> loader = new AssemblyLoader();


		/// <inheritdoc />
		public IAssemblyTarget LoadAssembly(LoadMethod loadMethod, string assemblyPath, string pdbPath = null) {
			IAssemblyTarget target;
			Assembly assembly = loader.LoadAssembly(loadMethod, assemblyPath, pdbPath);
			if (loadMethod == LoadMethod.LoadBits) {
				// Assemlies loaded by bits will have the codebase set to the assembly that loaded it.
				// Set it to the correct path here.
				Uri codebaseUri = new Uri(assemblyPath);
				target = AssemblyTarget.FromPath(codebaseUri, assembly.Location, assembly.GetName());
			} else
				target = AssemblyTarget.FromAssembly(assembly);
			return target;
		}

		/// <inheritdoc />
		public IList<IAssemblyTarget> LoadAssemblyWithReferences(LoadMethod loadMethod, string assemblyPath)
			=> loader.LoadAssemblyWithReferences(loadMethod, assemblyPath).Select(AssemblyTarget.FromAssembly).ToList();

		/// <inheritdoc />
		public IAssemblyTarget[] GetAssemblies() {
			Assembly[] assemblies = loader.GetAssemblies();
			return assemblies.Select(AssemblyTarget.FromAssembly).ToArray();
		}
	}
}
