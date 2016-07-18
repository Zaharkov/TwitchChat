using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHelper
{
    public static class Check
    {
        public static void ForNullReference<TObj>(TObj obj, string param = null, string message = null)
            where TObj : class
        {
            if (null == obj)
                throw CreateArgumentNullExceptionObject(param, message);
        }

        public static void ForEmptyString(string obj, string param = null, string message = null)
        {
            if (string.IsNullOrWhiteSpace(obj)) throw CreateArgumentNullExceptionObject(param, message);
        }

        public static void ForEmptyEnumerable<T>(IEnumerable<T> obj, string param = null, string message = null)
        {
            if (!obj.Any()) throw CreateArgumentExceptionObject(param, message);
        }

        private static ArgumentException CreateArgumentExceptionObject(string param, string message)
        {
            ArgumentException exception;
            if (!string.IsNullOrWhiteSpace(param) && string.IsNullOrWhiteSpace(message))
                exception = new ArgumentException(param);
            else if (!string.IsNullOrWhiteSpace(param) && !string.IsNullOrWhiteSpace(message))
            {
                exception = new ArgumentException(param, message);
            }
            else
            {
                exception = new ArgumentException();
            }
            return exception;
        }

        private static ArgumentNullException CreateArgumentNullExceptionObject(string param, string message)
        {
            ArgumentNullException exception;
            if (!string.IsNullOrWhiteSpace(param) && string.IsNullOrWhiteSpace(message))
                exception = new ArgumentNullException(param);
            else if (!string.IsNullOrWhiteSpace(param) && !string.IsNullOrWhiteSpace(message))
            {
                exception = new ArgumentNullException(param, message);
            }
            else
            {
                exception = new ArgumentNullException();
            }
            return exception;
        }
    }
}
