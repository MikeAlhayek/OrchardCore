using System;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Navigation;

public static class MenuItemExtensions
{
    public static bool HasRouteValuesAs(this MenuItem menuItem, RouteValueDictionary routeValues)
    {
        if (menuItem.RouteValues == null || routeValues == null)
        {
            return false;
        }

        foreach (var routeValue in routeValues)
        {
            if (!String.Equals(routeValue.Value?.ToString(), menuItem.RouteValues[routeValue.Key]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
