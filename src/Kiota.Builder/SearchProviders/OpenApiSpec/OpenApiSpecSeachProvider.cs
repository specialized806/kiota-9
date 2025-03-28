﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kiota.Builder.SearchProviders.MSGraph;

public class OpenApiSpecSearchProvider : ISearchProvider
{
    public string ProviderKey => "oas";
    public HashSet<string> KeysToExclude { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Task<IDictionary<string, SearchResult>> SearchAsync(string term, string? version, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(term);

        if (term.Split(termSeparator, StringSplitOptions.RemoveEmptyEntries).Any(Keywords.Contains))
        {
            return Task.FromResult<IDictionary<string, SearchResult>>(new Dictionary<string, SearchResult> {
                { "petstore", new SearchResult(ApiTitle, ApiDescription, new Uri("https://petstore.swagger.io/v1"), new Uri("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/refs/heads/main/_archive_/schemas/v3.0/pass/petstore.yaml"), new List<string> { "1.0.0" }) }
            });
        }
        return Task.FromResult<IDictionary<string, SearchResult>>(new Dictionary<string, SearchResult>());
    }
    private const string ApiTitle = "Swagger Petstore";
    private const string ApiDescription = "Canonical API description used in many examples throughout the OpenAPI ecosystem.";
    private readonly HashSet<string> Keywords = new(10, StringComparer.OrdinalIgnoreCase) {
        "pet",
        "store",
        "petstore",
        "cat",
        "dog",
        "open",
        "api",
        "openapi",
        "example",
        "swagger",
    };
    private static readonly char[] termSeparator = [' ', '-'];
}
