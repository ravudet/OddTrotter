namespace System
{
    public sealed class AbsoluteUri : Uri
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="uri"/> is not an absolute URI</exception>
        public AbsoluteUri(Uri uri)
            : base(
                uri == null ? 
                    throw new ArgumentNullException(nameof(uri)) : 
                    !uri.IsAbsoluteUri ? 
                        throw new ArgumentException($"{nameof(uri)} can only be an absolute URI") : 
                        uri.OriginalString, 
                UriKind.Absolute)
        {
        }
    }
}
