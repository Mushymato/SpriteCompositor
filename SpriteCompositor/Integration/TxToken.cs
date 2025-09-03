using System.Diagnostics.CodeAnalysis;
using SpriteCompositor.Framework;

namespace SpriteCompositor.Integration;

/// <summary>Method delegates which represent a simplified version of <see cref="IValueProvider"/> that can be implemented by custom mod tokens through the API via <see cref="ConventionValueProvider"/>.</summary>
/// <remarks>Methods should be kept in sync with <see cref="ConventionWrapper"/>.</remarks>
public sealed class TxToken
{
    /****
    ** Metadata
    ****/
    // /// <summary>Get whether the values may change depending on the context.</summary>
    // /// <remarks>Default true.</remarks>
    // public bool IsMutable() => false;

    /// <remarks>Default false.</remarks>
    public bool IsDeterministicForInput() => true;

    /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
    /// <remarks>Default false.</remarks>
    public bool AllowsInput() => true;

    /// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
    /// <remarks>Default false.</remarks>
    public bool RequiresInput() => true;

    // /// <summary>Whether the token may return multiple values for the given input.</summary>
    // /// <param name="input">The input arguments, if any.</param>
    // /// <remarks>Default true.</remarks>
    // public bool CanHaveMultipleValues(string? input = null) => false;

    // /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
    // /// <remarks>Default unrestricted.</remarks>
    // public IEnumerable<string> GetValidInputs() => [];

    // /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
    // /// <param name="input">The input arguments, if any.</param>
    // /// <param name="allowedValues">The possible values for the input.</param>
    // /// <remarks>Default unrestricted.</remarks>
    // public bool HasBoundedValues(string? input, out IEnumerable<string> allowedValues) => false;

    // /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
    // /// <param name="input">The input arguments, if any.</param>
    // /// <param name="min">The minimum value this token may return.</param>
    // /// <param name="max">The maximum value this token may return.</param>
    // /// <remarks>Default false.</remarks>
    // public bool HasBoundedRangeValues(string? input, out int min, out int max);

    // /// <summary>Validate that the provided input arguments are valid.</summary>
    // /// <param name="input">The input arguments, if any.</param>
    // /// <param name="error">The validation error, if any.</param>
    // /// <returns>Returns whether validation succeeded.</returns>
    // /// <remarks>Default true.</remarks>
    // public bool TryValidateInput(string? input, [NotNullWhen(false)] out string? error)
    // {
    //     if (input == null)
    //     {
    //         error = "Must have input";
    //         return false;
    //     }
    //     string[] parts = input.Split('@');
    //     if (parts.Length < 2 || !AssetManager.ValidAssetNames.ContainsKey(parts[0]))
    //     {
    //         error = $"Not registered with {ModEntry.ModId}: '{input}'";
    //         return false;
    //     }
    //     error = null!;
    //     return true;
    // }

    // /// <summary>Validate that the provided values are valid for the given input arguments (regardless of whether they match).</summary>
    // /// <param name="input">The input arguments, if any.</param>
    // /// <param name="values">The values to validate.</param>
    // /// <param name="error">The validation error, if any.</param>
    // /// <returns>Returns whether validation succeeded.</returns>
    // /// <remarks>Default true.</remarks>
    // public delegate bool TryValidateValues(string? input, IEnumerable<string> values, [NotNullWhen(false)] out string? error);

    // /// <summary>Normalize a token value so it matches the format expected by the value provider, if needed.</summary>
    // /// <param name="value">This receives the raw value, already trimmed and non-empty.</param>
    // public delegate string NormalizeValue(string value);


    /****
    ** State
    ****/
    private bool isReady = false;

    /// <summary>Update the values when the context changes.</summary>
    /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
    public bool UpdateContext()
    {
        bool changed = !isReady;
        isReady = true;
        return changed;
    }

    /// <summary>Get whether the token is available for use.</summary>
    public bool IsReady() => isReady;

    /// <summary>Get the current values.</summary>
    /// <param name="input">The input arguments, if any.</param>
    public IEnumerable<string> GetValues(string? input)
    {
        if (input == null)
            yield break;
        string[] parts = input.Split('+');
        yield return $"{AssetManager.TxPrefix}{parts[0]}/{parts[1]}";
    }
}
