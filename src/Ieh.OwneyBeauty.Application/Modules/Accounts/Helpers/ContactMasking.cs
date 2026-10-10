using System;

namespace Ieh.OwneyBeauty.Modules.Accounts.Helpers;

public static class ContactMasking
{
    public static string Mask(string value)
    {
        var at = value.IndexOf('@');
        if (at > 0)
        {
            var visible = Math.Min(2, at);
            return value[..visible] + new string('*', Math.Max(3, at - visible)) + value[at..];
        }

        return value.Length <= 6 ? new string('*', value.Length) : value[..5] + new string('*', value.Length - 7) + value[^2..];
    }
}
