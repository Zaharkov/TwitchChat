using System.Collections.Generic;
using CommonHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestMethod = RestSharp.Method;

namespace RestClientHelper
{
    public class RestRequestBuilder
    {
        private readonly IRestRequest _request;

        public RestRequestBuilder(string resourceUri)
        {
            Check.ForNullReference(resourceUri);

            _request = new RestRequest(resourceUri)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = NewtonsoftJsonSerializer.Default
            };
        }

        public RestRequestBuilder Method(RestMethod method)
        {
            _request.Method = method;
            return this;
        }

        /// <summary>
		/// Adds parameter into query string if it was specified.
		/// Multimple values will be joined with ',' as a separator.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public RestRequestBuilder AddQueryParameterIfNotNull<T>(string name, IEnumerable<T> values)
        {
            Check.ForNullReference(name, "name");

            if (values == null)
                return this;

            var paramValue = string.Join(",", values);
            _request.AddQueryParameter(name, paramValue);

            return this;
        }

        /// <summary>
        /// Adds parameter into query string if it was specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestRequestBuilder AddQueryParameterIfNotNull<T>(string name, T? value)
            where T : struct
        {
            Check.ForNullReference(name, "name");

            if (value.HasValue)
            {
                if (typeof(T).IsEnum)
                {
                    var stringValue = JsonConvert.SerializeObject(value.Value, new JsonSerializerSettings
                    {
                        Converters = new JsonConverter[] { new StringEnumConverter() }
                    }).Trim('\"');

                    _request.AddQueryParameter(name, stringValue);
                }
                else
                    _request.AddQueryParameter(name, value.Value.ToString());
            }

            return this;
        }

        /// <summary>
        /// Adds parameter into query string if it was specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestRequestBuilder AddQueryParameterIfNotNull<T>(string name, T value)
            where T : class
        {
            Check.ForNullReference(name, "name");

            if (value != null)
                _request.AddQueryParameter(name, JsonConvert.SerializeObject(value));

            return this;
        }

        /// <summary>
        /// Adds parameter into query string if it was specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestRequestBuilder AddQueryParameterIfNotNull(string name, int value)
        {
            Check.ForNullReference(name, "name");

            _request.AddQueryParameter(name, value.ToString());

            return this;
        }

        /// <summary>
        /// Adds parameter into query string if it was specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestRequestBuilder AddQueryParameterIfNotNull(string name, string value)
        {
            Check.ForNullReference(name, "name");

            if (value != null)
                _request.AddQueryParameter(name, value);

            return this;
        }

        /// <summary>
        /// Adds parameter into query string if it was specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestRequestBuilder AddQueryParameterIfNotNull(string name, bool? value)
        {
            Check.ForNullReference(name, "name");

            if (value.HasValue)
                _request.AddQueryParameter(name, value.Value ? "1" : "0");

            return this;
        }

        public IRestRequest Build()
        {
            return _request;
        }
    }
}
