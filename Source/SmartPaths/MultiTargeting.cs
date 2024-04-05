using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if NET6_0
namespace SmartPaths;

public class ArgumentException
{

    public static void ThrowIfNullOrEmpty([NotNull] string? argument,
                                          [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
        if (string.IsNullOrEmpty(argument)) {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            throw new System.ArgumentException("Argument is empty", paramName);
        }
    }

}

#endif