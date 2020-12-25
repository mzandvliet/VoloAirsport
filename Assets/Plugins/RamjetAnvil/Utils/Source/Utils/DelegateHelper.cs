using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Whathecode.System
{
	/// <summary>
	///   A helper class to do common <see cref = "Delegate" /> operations.
	///   TODO: Add extra contracts to reenforce correct usage.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public static class DelegateHelper
	{
		
		/// <summary>
		///   The name of the Invoke method of a Delegate.
		/// </summary>
		const string InvokeMethod = "Invoke";


		/// <summary>
		///   Get method info for a specified delegate type.
		/// </summary>
		/// <param name = "delegateType">The delegate type to get info for.</param>
		/// <returns>The method info for the given delegate type.</returns>
		public static MethodInfo MethodInfoFromDelegateType( Type delegateType )
		{
			return delegateType.GetMethod( InvokeMethod );
		}


		/// <summary>
		///   Creates a delegate of a specified type that represents the specified static or instance method,
		///   with the specified first argument.
		/// </summary>
		/// <typeparam name = "TDelegate">The type for the delegate.</typeparam>
		/// <param name = "method">The MethodInfo describing the static or instance method the delegate is to represent.</param>
		/// <param name = "instance">When method is an instance method, the instance to call this method on. Null for static methods.</param>
		public static TDelegate CreateDelegate<TDelegate>(MethodInfo method, object instance) where TDelegate : class {
			MethodInfo delegateInfo = MethodInfoFromDelegateType( typeof( TDelegate ) );

			// Create delegate original and converted arguments.
			var delegateParameters = delegateInfo.GetParameters().Select(d => Expression.Parameter(d.ParameterType, "")).ToArray();
			var methodTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

		    if (delegateParameters.Length != methodTypes.Length) {
		        throw new Exception("Cannot convert method " + method + " to " + typeof(TDelegate) + " (incorrect parameter count)");
		    }
		    var expressionArguments = new List<Expression>();
		    for (int i = 0; i < delegateParameters.Length; i++) {
		        expressionArguments.Add(ConvertExpression(delegateParameters[i], methodTypes[i]));    
		    }

			// Create method call.
			Expression methodCall = Expression.Call(
				Expression.Constant(instance),
				method,
				expressionArguments);

		    return Expression.Lambda<TDelegate>(
		        ConvertExpression(methodCall, delegateInfo.ReturnType),
				delegateParameters
				).Compile();

		}

	    public static Expression ConvertExpression(Expression original, Type requestedType) {
	        Expression convertedExpression;
	        if (original.Type != requestedType) {
	            convertedExpression = Expression.Convert(original, requestedType);
	        } else {
	            convertedExpression = original;
	        }
	        return convertedExpression;
	    }
	}
}