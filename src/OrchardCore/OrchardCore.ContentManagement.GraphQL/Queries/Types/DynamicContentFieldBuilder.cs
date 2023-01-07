using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class DynamicContentFieldBuilder : IContentTypeBuilder
{
    private readonly GraphQLContentOptions _contentOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer S;
    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly Dictionary<string, FieldType> _dynamicPartFields;

    public DynamicContentFieldBuilder(
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<DynamicContentFieldBuilder> stringLocalizer,
        IEnumerable<IContentFieldProvider> contentFieldProviders)
    {
        _contentOptions = contentOptionsAccessor.Value;
        _httpContextAccessor = httpContextAccessor;
        _contentFieldProviders = contentFieldProviders;
        _dynamicPartFields = new Dictionary<string, FieldType>();

        S = stringLocalizer;
    }

    public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
    {
        if (_contentOptions.ShouldSkipContentType(contentTypeDefinition.Name))
        {
            return;
        }

        var whereArgument = contentQuery.Arguments.FirstOrDefault(x => x.Name == "where");

        if (whereArgument == null)
        {
            return;
        }

        var whereInput = (ContentItemWhereInput)whereArgument.ResolvedType;

        if (whereInput == null)
        {
            return;
        }

        foreach (var part in contentTypeDefinition.Parts)
        {
            if (_contentOptions.ShouldSkip(part))
            {
                continue;
            }

            var scalers = new List<FieldType>();

            foreach (var field in part.PartDefinition.Fields)
            {
                foreach (var provider in _contentFieldProviders)
                {
                    var fieldType = provider.GetField(field);

                    if (fieldType != null && typeof(ScalarGraphType).IsAssignableFrom(fieldType.Type))
                    {
                        scalers.Add(fieldType);
                    }
                }
            }

            if (scalers.Count == 0)
            {
                // no scaler field to build inputs. 
                continue;
            }

            // if the part registred manually, do not procees?
            if (_contentOptions.ShouldCollapse(part))
            {
                foreach (var scaler in scalers)
                {
                    whereInput.AddScalarFilterFields(scaler.Type, scaler.Name, scaler.Description);
                }
            }
            else
            {
                if (_dynamicPartFields.TryGetValue(part.Name, out var fieldType))
                {
                    whereInput.AddScalarFilterFields(fieldType.Type, fieldType.Name, fieldType.Description);
                }
                else
                {
                    var field = whereInput.Field(
                    typeof(DynamicPartInputGraphType),
                        part.Name.ToFieldName(),
                        description: S["Represents a {0}.", part.PartDefinition.Name],
                        resolve: context =>
                        {
                            var nameToResolve = part.Name;

                            var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType.GetGenericArguments().First();

                            return ((ContentItem)context.Source).Get(typeToResolve, nameToResolve);
                        }).WithPartNameMetaData(part.Name);

                    field.ResolvedType = new DynamicPartInputGraphType(part, scalers);

                    _dynamicPartFields[part.Name] = field;
                }
            }
        }
    }
    public void Clear()
    {
        _dynamicPartFields.Clear();
    }
}
