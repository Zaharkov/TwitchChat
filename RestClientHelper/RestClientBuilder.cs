using System;
using RestSharp;

namespace RestClientHelper
{
    public class RestClientBuilder
    {
        private Uri _baseUri;

        public RestClientBuilder()
        {
        }

        public RestClientBuilder(Uri baseUri)
        {
            Check.ForNullReference(baseUri, nameof(baseUri));

            _baseUri = baseUri;
        }

        public RestClientBuilder BaseUri(string baseUri)
        {
            Check.ForNullReference(baseUri, nameof(baseUri));

            _baseUri = new Uri(baseUri);

            return this;
        }

        public RestClientBuilder BaseUri(Uri baseUri)
        {
            Check.ForNullReference(baseUri, nameof(baseUri));

            _baseUri = baseUri;

            return this;
        }

        public IRestClient Build()
        {
            Check.ForNullReference(_baseUri, nameof(_baseUri));

            var client = new RestClient(_baseUri);

            // Override with Newtonsoft JSON Handler
            client.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/json", NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/x-json", NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/javascript", NewtonsoftJsonSerializer.Default);
            client.AddHandler("*+json", NewtonsoftJsonSerializer.Default);
            client.AddHandler("application/vnd.twitchtv.v3", NewtonsoftJsonSerializer.Default);

            return client;
        }
    }
}
