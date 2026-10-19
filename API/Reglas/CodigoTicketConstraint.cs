using System.Text.RegularExpressions;

namespace API.Reglas
{
    public class CodigoTicketConstraint : IRouteConstraint
    {
        private static readonly Regex RegexCodigo = new Regex(@"^INC-\d{6}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool Match(
            HttpContext? httpContext,
            IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var val) && val != null)
            {
                var codigo = val.ToString();
                return !string.IsNullOrEmpty(codigo) && RegexCodigo.IsMatch(codigo);
            }

            return false;
        }
    }
}
