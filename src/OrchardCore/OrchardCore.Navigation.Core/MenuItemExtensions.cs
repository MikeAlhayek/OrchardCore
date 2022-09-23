using System;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Navigation;

public static class MenuItemExtensions
{
    public static bool HasRouteValuesAs(this MenuItem menuItem, RouteValueDictionary routeValues, bool ignoreEmptyValues = true)
    {
        if (menuItem.RouteValues == null || routeValues == null)
        {
            return false;
        }

        foreach (var routeValue in menuItem.RouteValues)
        {
            var value = routeValue.Value?.ToString();

            if (ignoreEmptyValues && String.IsNullOrEmpty(value))
            {
                // Ignore any optional route values
                continue;
            }

            if (!String.Equals(value, routeValues[routeValue.Key]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
