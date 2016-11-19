using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Error : Exception
{
	public Error(string message) : base(message)
	{
	}
}

public class Result
{
	public string errorMessage;

	public static Result Error(string errorMessage = null)
	{
		var r = new Result();
		r.errorMessage = errorMessage;
		return r;
	}

	public static Result Error<T>(Result<T> result)
	{
		var r = new Result();
		r.errorMessage = result.errorMessage;
		return r;
	}

	public void Throw(string prefixMessage = null)
	{
		if (string.IsNullOrEmpty(prefixMessage)) throw new Error(errorMessage);
		else throw new Error(prefixMessage + ": " + errorMessage);
	}
}

public class Result<T>
{
	public T value;
	public string errorMessage;
	public bool isError;

	public static Result<T> Success(T value)
	{
		var r = new Result<T>();
		r.value = value;
		r.isError = false;
		return r;
	}

	public static Result<T> Error(string errorMessage)
	{
		var r = new Result<T>();
		r.errorMessage = errorMessage;
		r.isError = true;
		return r;
	}

	/// <summary>
	/// If result is error, set the value to.
	/// </summary>
	public Result<T> IfErrorThenSetValueTo(T value)
	{
		if (isError) this.value = value;
		return this;
	}

	public Result<T> IfErrorThenThrowException(string prefixMessage = null)
	{
		if (isError)
		{
			if (string.IsNullOrEmpty(prefixMessage)) throw new Error(errorMessage);
			else throw new Error(prefixMessage + ": " + errorMessage);
		}
		return this;
	}

	public static implicit operator Result<T>(T value)
	{
		return Success(value);
	}

	public static implicit operator T(Result<T> result)
	{
		return result.value;
	}

	public static implicit operator Result<T>(Result error)
	{
		return Error(error.errorMessage);
	}

	public Result PassError()
	{
		if (this.isError == false) throw new Exception("cannot pass error if there is none");
		return Result.Error(this.errorMessage);
	}

	public override string ToString()
	{
		return value.ToString();
	}
}