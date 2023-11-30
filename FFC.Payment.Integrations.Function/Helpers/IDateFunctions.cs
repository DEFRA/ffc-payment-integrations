namespace FFC.Payment.Integrations.Function.Helpers;

/// <summary>
/// Date functions - primarily so they can be overridden for unit testing
/// </summary>
public interface IDateFunctions
{
    /// <summary>
    /// Gets a string version of the current time in UTC format
    /// </summary>
    /// <returns>string of current UTC date/time</returns>
    string GetUtcNow();
}
