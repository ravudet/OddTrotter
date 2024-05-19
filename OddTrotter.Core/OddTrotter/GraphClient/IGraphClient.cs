﻿namespace OddTrotter.GraphClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IGraphClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeUri"/> is <see langword="null"/></exception>
        /// <exception cref="HttpRequestException">
        /// Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout
        /// </exception>
        /// <exception cref="InvalidAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
        Task<HttpResponseMessage> GetAsync(RelativeUri relativeUri);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteUri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="absoluteUri"/> is <see langword="null"/></exception>
        /// <exception cref="HttpRequestException">
        /// Thrown if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </exception>
        /// <exception cref="InvalidAccessTokenException">Thrown if the access token used is invalid or provides insufficient privileges for the request</exception>
        Task<HttpResponseMessage> GetAsync(AbsoluteUri absoluteUri);

        Task<HttpResponseMessage> PatchAsync(RelativeUri relativeUri, HttpContent httpContent); //// TODO document this

        Task<HttpResponseMessage> PostAsync(RelativeUri relativeUri, HttpContent httpContent); //// TODO document this
    }
}
