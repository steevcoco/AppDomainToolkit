using System;
using System.Threading.Tasks;


namespace AppDomainToolkit
{
	/// <summary>
	/// Marshalable TaskCompletionSource
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MarshalableTaskCompletionSource<T> : MarshalByRefObject
	{
		private readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

		public Task<T> Task
			=> tcs.Task;

		public void SetResult(T result)
			=> tcs.SetResult(result);

		public void SetException(Exception[] exception)
			=> tcs.SetException(exception);

		public void SetCanceled()
			=> tcs.SetCanceled();
	}
}
