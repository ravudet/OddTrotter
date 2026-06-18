namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class MembersReader<TNextReader> : ICategoryReader<MembersReader<TNextReader>, MembersCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out MembersCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            //// TODO you are here
            //// TODO this is actually broken because subsequent members are comma delimited
            //// TODO make sure to update the array reading to do the same

            if (context.Buffer[context.CurrentByteIndex] == '}')
            {
                category = MembersCategory<TNextReader>.None();
            }
            else
            {
                category = MembersCategory<TNextReader>.Some();
            }

            return true;
        }
    }
}
