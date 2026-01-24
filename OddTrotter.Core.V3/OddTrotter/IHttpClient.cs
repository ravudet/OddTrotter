////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace OddTrotter.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteUri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="absoluteUri"/> or <paramref name="headers"/> is <see langword="null"/></exception>
        /// <exception cref="HttpRequestException">
        /// Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout
        /// </exception>
        Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri, IEnumerable<HttpHeader> headers);
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc2616#section-4.2
    /// </summary>
    public sealed class HttpHeader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="value"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is the empty string</exception>
        public HttpHeader(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)} cannot be empty", name);
            }

            if (value == null)
            {
                // empty values are allowed by the RFC
                throw new ArgumentNullException(nameof(value));
            }

            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }

    public sealed class HttpClientAdapter : IHttpClient
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is <see langword="null"/></exception>
        public HttpClientAdapter(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri, IEnumerable<HttpHeader> headers)
        {
            if (absoluteUri == null)
            {
                throw new ArgumentNullException(nameof(absoluteUri));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, absoluteUri))
            {
                foreach (var header in headers.Where(_ => _ != null))
                {
                    request.Headers.Add(header.Name, header.Value);
                }

                return await this.httpClient.SendAsync(request).ConfigureAwait(false);
            }
        }
    }
}
