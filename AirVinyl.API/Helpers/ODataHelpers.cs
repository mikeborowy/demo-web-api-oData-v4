using Microsoft.OData.Core.UriParser;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;

namespace AirVinyl.API.Helpers
{
    /// <summary>
    /// OData Helper methods - slightly adjusted from OData helpers provided by Microsoft
    /// </summary>
    public static class ODataHelpers
    {
        public static bool HasProperty(this object instance, string propertyName)
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            return (propertyInfo != null);
        }

        public static object GetValue(this object instance, string propertyName)
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new HttpException("Can't find property with name " + propertyName);
            }
            var propertyValue = propertyInfo.GetValue(instance, new object[] { });

            return propertyValue;
        }

        public static IHttpActionResult CreateOKHttpActionResult(this ODataController controller, object propertyValue)
        {
            var okMethod = default(MethodInfo);

            // find the ok method on the current controller
            var methods = controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.Name == "Ok" && method.GetParameters().Length == 1)
                {
                    okMethod = method;
                    break;
                }
            }

            // invoke the method, passing in the propertyValue
            okMethod = okMethod.MakeGenericMethod(propertyValue.GetType());
            var returnValue = okMethod.Invoke(controller, new object[] { propertyValue });
            return (IHttpActionResult)returnValue;
        }


        /// <summary>
        /// Helper method to get the odata path for an arbitrary odata uri.
        /// </summary>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri</param>
        /// <returns>The parsed odata path</returns>
        public static ODataPath CreateODataPath(this HttpRequestMessage request, Uri uri)
        {
            //uri = {http://localhost:64951/odata/People(1)}

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var route = request.GetRouteData().Route;

            var newRoute = new HttpRoute(
                route.RouteTemplate,
                new HttpRouteValueDictionary(route.Defaults),
                new HttpRouteValueDictionary(route.Constraints),
                new HttpRouteValueDictionary(route.DataTokens),
                route.Handler);

            var config = request.GetConfiguration();
            //{"/"}
            var virtualPathRoot = config.VirtualPathRoot;
            //{Method: GET, RequestUri: 'http://localhost:64951/odata/People(1)', Version: 1.1, Content: <null>, Headers:}
            var newRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            //Values = {
            //  { Key = "odataPath", value: "People(1)" }, 
            //  { Key = "controller", value: "People" }}
            var routeData = newRoute.GetRouteData(virtualPathRoot, newRequest);

            if (routeData == null)
            {
                throw new InvalidOperationException("This link is not a valid OData link.");
            }

            var newReqProp = newRequest.ODataProperties();
            //Path = {People(1)}
            var newPath = newReqProp.Path;

            return newPath;
        }

        public static TKey GetKeyValue<TKey>(this HttpRequestMessage request, Uri uri)
        {
            //request = {Method: POST, RequestUri: 'http://localhost:64951/odata/People(7)/Friends/$ref'..}
            //uri = {http://localhost:64951/odata/People(1)}

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            //get the odata path Ex: ~/entityset/key/$links/navigation
            //odataPath = {People(1)}
            var odataPath = request.CreateODataPath(uri);
            //[0]{1} => 
            var segments = odataPath.Segments.OfType<KeyValuePathSegment>();
            //{SegmentKind : "key", Value = "1"} 
            var keySegment = segments.LastOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("This link does not contain a key.");
            }
            //value = 1
            var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, Microsoft.OData.Core.ODataVersion.V4);

            return (TKey)value;
        }
    }
}
