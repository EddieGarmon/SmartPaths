using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }

    /// <summary>Specifies that an output is not <see langword="null" /> even if the corresponding type
    ///     allows it. Specifies that an input argument was not <see langword="null" /> when the call
    ///     returns.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    internal sealed class NotNullAttribute : Attribute { }

    /// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {

        /// <summary>Initializes the attribute with the associated parameter name.</summary>
        /// <param name="parameterName">The associated parameter name.  The output will be non-null if the
        ///     argument to the parameter specified is non-null.</param>
        public NotNullIfNotNullAttribute(string parameterName) {
            ParameterName = parameterName;
        }

        /// <summary>Gets the associated parameter name.</summary>
        public string ParameterName { get; }

    }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue" />, the parameter will not be
    ///     null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute : Attribute
    {

        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">The return value condition. If the method returns this value, the
        ///     associated parameter will not be null.</param>
        public NotNullWhenAttribute(bool returnValue) {
            ReturnValue = returnValue;
        }

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; }

    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {

        public CallerArgumentExpressionAttribute(string parameterName) {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }

    }
}

namespace SmartPaths
{
    public class ArgumentNullException : System.ArgumentNullException
    {

        public ArgumentNullException(string paramName)
            : base(paramName) { }

        /// <summary>Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" />
        ///     corresponds.</param>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
            if (argument is null) {
                Throw(paramName);
            }
        }

        [DoesNotReturn]
        private static void Throw(string? paramName) {
            throw new System.ArgumentNullException(paramName);
        }

    }

    public class ArgumentException
    {

        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (string.IsNullOrEmpty(argument)) {
                throw new System.ArgumentException("Argument is empty", paramName);
            }
        }

    }
}
#endif