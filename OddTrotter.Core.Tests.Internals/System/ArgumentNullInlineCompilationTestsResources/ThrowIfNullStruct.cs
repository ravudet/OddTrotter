/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

public static class ThrowIfNullStruct
{
    private readonly struct MockStruct
    {
    }

    public static void Method()
    {
        var @struct = new MockStruct();

        ArgumentNullInline.ThrowIfNull(@struct);
    }
}