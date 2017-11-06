using System;
using System.Threading.Tasks;


namespace AppDomainToolkit
{
	/// <summary>
	/// Marshalable <see cref="TaskCompletionSource{TResult}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MarshalableTaskCompletionSource<T>
			: MarshalByRefObject
	{
		private readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

		/// <summary>
		/// The <see cref="TaskCompletionSource{TResult}.Task"/>
		/// </summary>
		public Task<T> Task
			=> tcs.Task;

		/// <summary>
		/// Invokes <see cref="TaskCompletionSource{TResult}.SetResult"/>
		/// </summary>
		public void SetResult(T result)
			=> tcs.SetResult(result);

		/// <summary>
		/// Invokes <see cref="TaskCompletionSource{TResult}.SetException(System.Collections.Generic.IEnumerable{Exception})"/>
		/// </summary>
		public void SetException(Exception[] exception)
			=> tcs.SetException(exception);

		/// <summary>
		/// Invokes <see cref="TaskCompletionSource{TResult}.SetCanceled"/>
		/// </summary>
		public void SetCanceled()
			=> tcs.SetCanceled();
	}
}
