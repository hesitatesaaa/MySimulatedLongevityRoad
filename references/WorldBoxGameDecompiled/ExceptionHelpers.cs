using System;
using System.Runtime.ExceptionServices;

internal static class ExceptionHelpers
{
	public static Exception PrepareForRethrow(Exception exception)
	{
		ExceptionDispatchInfo.Capture(exception).Throw();
		return exception;
	}
}
