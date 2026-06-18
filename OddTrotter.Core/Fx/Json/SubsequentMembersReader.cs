namespace Fx.Json
{
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class SubsequentMembersReader<TNextReader> : ICategoryReader<SubsequentMembersReader<TNextReader>, SubsequentMembersCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out SubsequentMembersCategory<TNextReader> category)
        {
            Helpers.EnsureValidBytes(context);
            if (Helpers.NeedsMoreBytes(context, out category))
            {
                return false;
            }

            if (context.Buffer[context.CurrentByteIndex] == ',')
            {
                category = SubsequentMembersCategory<TNextReader>.Some();
            }
            else
            {
                category = SubsequentMembersCategory<TNextReader>.None();
            }

            return true;
        }
    }
}
