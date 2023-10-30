using System;

namespace FFC.Payment.Integrations.Function.Helpers;

/// <summary>
/// Date functions - primarily so they can be overridden for unit testing
/// </summary>
public class DateFunctions : IDateFunctions
{
    /// <inheritdoc/>
    public string GetUtcNow()
    {
        return DateTime.UtcNow.ToString();
    }
}
