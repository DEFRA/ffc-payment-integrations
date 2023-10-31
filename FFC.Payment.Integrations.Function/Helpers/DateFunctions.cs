using System;

namespace FFC.Payment.Integrations.Function.Helpers;

/// <summary>
/// Date functions - primarily so they can be overridden for unit testing.
/// The problem: Not easy to unit test a function that sets a date/time since the time may have changed
/// during execution by the time the unit test gets to check what the time should have been. So the
/// date/time functions are injected in order for unit tests to use a fixed date/time.
/// </summary>
public class DateFunctions : IDateFunctions
{
    /// <inheritdoc/>
    public string GetUtcNow()
    {
        return DateTime.UtcNow.ToString();
    }
}
