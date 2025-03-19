using System;

public static class Foo
{
    private readonly struct MockStruct
    {
    }

    public static void Test()
    {
        var @struct = new MockStruct();

        ArgumentNullInline.ThrowIfNull(@struct);
    }
}