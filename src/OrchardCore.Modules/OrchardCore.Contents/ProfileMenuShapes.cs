using System;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Contents;

public class ProfileMenuShapes : IShapeTableProvider
{
    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("Navigation")
            .OnDisplaying(displaying =>
            {
                var menu = displaying.Shape;
                var menuName = menu.GetProperty<string>("MenuName");

                if (String.IsNullOrEmpty(menuName) || !menuName.StartsWith("Profile."))
                {
                    return;
                }

                var contentType = menuName.Replace("Profile.", "");

                menu.Classes.Add("profile-menu");
                menu.Classes.Add("profile-menu-" + contentType.HtmlClassify());
                menu.Classes.Add("menu-" + contentType.HtmlClassify());
                menu.Classes.Add("menu");
                menu.Metadata.Alternates.Add("Profile__" + contentType + "__Navigation");
                menu.Metadata.Alternates.Add("Profile__Navigation");
            });

        builder.Describe("NavigationItem")
            .OnDisplaying(displaying =>
            {
                var menuItem = displaying.Shape;
                var menu = menuItem.GetProperty<IShape>("Menu");
                var menuName = menu.GetProperty<string>("MenuName");
                var level = menuItem.GetProperty<int>("Level");

                if (String.IsNullOrEmpty(menuName) || !menuName.StartsWith("Profile."))
                {
                    return;
                }
                var contentType = menuName.Replace("Profile.", "");

                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItem__level__" + level);
                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItem");
                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItem__level__" + level);

                menuItem.Metadata.Alternates.Add("Profile__NavigationItem__level__" + level);
                menuItem.Metadata.Alternates.Add("Profile__NavigationItem");
            });

        builder.Describe("NavigationItemLink")
            .OnDisplaying(displaying =>
            {
                var menuItem = displaying.Shape;
                var menuName = menuItem.GetProperty<IShape>("Menu").GetProperty<string>("MenuName");

                if (String.IsNullOrEmpty(menuName) || !menuName.StartsWith("Profile."))
                {
                    return;
                }
                var contentType = menuName.Replace("Profile.", "");
                var level = menuItem.GetProperty<int>("Level");

                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItemLink__level__" + level);

                // Profile__NavigationItemLink__[ContentType] e.g. NavigationItemLink-Main-Menu
                // Profile__NavigationItemLink__[ContentType]__level__[level] e.g. NavigationItemLink-Main-Menu-level-2
                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItemLink");
                menuItem.Metadata.Alternates.Add("Profile__" + EncodeAlternateElement(contentType) + "__NavigationItemLink__level__" + level);


                // Profile__NavigationItemLink__[MenuName] e.g. NavigationItemLink-Main-Menu
                // Profile__NavigationItemLink__[MenuName]__level__[level] e.g. NavigationItemLink-Main-Menu-level-2
                menuItem.Metadata.Alternates.Add("Profile__NavigationItemLink");
                menuItem.Metadata.Alternates.Add("Profile__NavigationItemLink__level__" + level);
            });
    }

    /// <summary>
    /// Encodes dashed and dots so that they don't conflict in filenames
    /// </summary>
    /// <param name="alternateElement"></param>
    /// <returns></returns>
    private static string EncodeAlternateElement(string alternateElement)
    {
        return alternateElement.Replace("-", "__").Replace('.', '_');
    }
}
