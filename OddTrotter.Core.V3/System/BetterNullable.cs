/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Diagnostics.CodeAnalysis;

    public readonly struct BetterNullable<T>
    {
        private readonly bool hasValue;
        private readonly T value;

        public BetterNullable(T value)
        {
            this.value = value;

            hasValue = true;
        }

        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            value = this.value;
            return hasValue;
        }
    }













    //// TODO then split this into files
    //// TODO implement a test with ref structs
    //// TODO implement a test using actual async (like reading a file or something)
    //// TODO implement any unimplemented methods in these files, probably adding a test or two as you go
    //// TODO then, implement the bare minimum needed for oddtrotter to make sure you have a real POC
    //// TODO go through oddtrotter.core.v2 to see if there's any ideas to pull from there
    //// TODO then, implement everything, ensuring that the oddtrotter POC still compiles
    //// TODO it seems like you have determine that there's iawaitable, which both allows for a state machine that waits and gives the result; and then there's irealizable which can be continued and can have its value realized; maybe play with the idea that these are isomorphic and can be adapted and such
}
