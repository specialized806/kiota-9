﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kiota.Builder.Lock;

namespace Kiota.Builder.Configuration;
#pragma warning disable CA2227
#pragma warning disable CA1002
#pragma warning disable CA1056
public class GenerationConfiguration : ICloneable
{
    public static GenerationConfiguration DefaultConfiguration
    {
        get;
    } = new();
    public bool ShouldGetApiManifest
    {
        get
        {
            return (string.IsNullOrEmpty(OpenAPIFilePath) || OpenAPIFilePath.Equals(DefaultConfiguration.OpenAPIFilePath, StringComparison.OrdinalIgnoreCase)) &&
                (!string.IsNullOrEmpty(ApiManifestPath) || !ApiManifestPath.Equals(DefaultConfiguration.ApiManifestPath, StringComparison.OrdinalIgnoreCase)) &&
                (ApiManifestPath.StartsWith("http", StringComparison.OrdinalIgnoreCase) || File.Exists(ApiManifestPath));
        }
    }
    public string OpenAPIFilePath { get; set; } = "openapi.yaml";
    public string ApiManifestPath { get; set; } = "apimanifest.json";
    public string OutputPath { get; set; } = "./output";
    public string ClientClassName { get; set; } = "ApiClient";
    public string ClientNamespaceName { get; set; } = "ApiSdk";
    public string NamespaceNameSeparator { get; set; } = ".";

    public string ModelsNamespaceName
    {
        get => $"{ClientNamespaceName}{NamespaceNameSeparator}models";
    }
    public GenerationLanguage Language { get; set; } = GenerationLanguage.CSharp;
    public string? ApiRootUrl
    {
        get; set;
    }
    public bool UsesBackingStore
    {
        get; set;
    }
    public bool ExcludeBackwardCompatible
    {
        get; set;
    }
    public bool IncludeBackwardCompatible
    {
        get => !ExcludeBackwardCompatible;
    }
    public bool IncludeAdditionalData { get; set; } = true;
    public HashSet<string> Serializers
    {
        get; set;
    } = new(4, StringComparer.OrdinalIgnoreCase){
        "Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory",
        "Microsoft.Kiota.Serialization.Text.TextSerializationWriterFactory",
        "Microsoft.Kiota.Serialization.Form.FormSerializationWriterFactory",
        "Microsoft.Kiota.Serialization.Multipart.MultipartSerializationWriterFactory"
    };
    public HashSet<string> Deserializers
    {
        get; set;
    } = new(3, StringComparer.OrdinalIgnoreCase) {
        "Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory",
        "Microsoft.Kiota.Serialization.Text.TextParseNodeFactory",
        "Microsoft.Kiota.Serialization.Form.FormParseNodeFactory",
    };
    public bool ShouldWriteNamespaceIndices
    {
        get
        {
            return BarreledLanguages.Contains(Language);
        }
    }
    public bool ShouldWriteBarrelsIfClassExists
    {
        get
        {
            return BarreledLanguagesWithConstantFileName.Contains(Language);
        }
    }
    public bool ShouldRenderMethodsOutsideOfClasses
    {
        get
        {
            return MethodOutsideOfClassesLanguages.Contains(Language);
        }
    }
    private static readonly HashSet<GenerationLanguage> MethodOutsideOfClassesLanguages = new(1) {
        GenerationLanguage.Go,
    };
    private static readonly HashSet<GenerationLanguage> BarreledLanguages = new(3) {
        GenerationLanguage.Ruby,
        GenerationLanguage.TypeScript,
        GenerationLanguage.Swift,
    };
    private static readonly HashSet<GenerationLanguage> BarreledLanguagesWithConstantFileName = new(1) {
        GenerationLanguage.TypeScript
    };
    public bool CleanOutput
    {
        get; set;
    }
    internal StructuredMimeTypesCollection OrderedStructuredMimeTypes
    {
        get; set;
    } = new StructuredMimeTypesCollection {
        "application/json;q=1",
        "text/plain;q=0.9",
        "application/x-www-form-urlencoded;q=0.2",
        "multipart/form-data;q=0.1",
    };
    public List<string> StructuredMimeTypes
    {
        get => OrderedStructuredMimeTypes.ToList(); set => OrderedStructuredMimeTypes = new StructuredMimeTypesCollection(value);
    }
    public HashSet<string> IncludePatterns { get; set; } = new(0, StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludePatterns { get; set; } = new(0, StringComparer.OrdinalIgnoreCase);
    public bool ClearCache
    {
        get; set;
    }
    public HashSet<string> DisabledValidationRules { get; set; } = new(0, StringComparer.OrdinalIgnoreCase);
    public int MaxDegreeOfParallelism { get; set; } = -1;
    public object Clone()
    {
        return new GenerationConfiguration
        {
            ExcludeBackwardCompatible = ExcludeBackwardCompatible,
            OpenAPIFilePath = OpenAPIFilePath,
            OutputPath = OutputPath,
            ClientClassName = ClientClassName,
            ClientNamespaceName = ClientNamespaceName,
            NamespaceNameSeparator = NamespaceNameSeparator,
            Language = Language,
            ApiRootUrl = ApiRootUrl,
            UsesBackingStore = UsesBackingStore,
            IncludeAdditionalData = IncludeAdditionalData,
            Serializers = new(Serializers ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
            Deserializers = new(Deserializers ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
            CleanOutput = CleanOutput,
            StructuredMimeTypes = new List<string>(StructuredMimeTypes ?? Enumerable.Empty<string>()),
            IncludePatterns = new(IncludePatterns ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
            ExcludePatterns = new(ExcludePatterns ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
            ClearCache = ClearCache,
            DisabledValidationRules = new(DisabledValidationRules ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
        };
    }
    private static readonly StringIEnumerableDeepComparer comparer = new();
    internal void UpdateConfigurationFromLanguagesInformation(LanguagesInformation languagesInfo)
    {
        if (!languagesInfo.TryGetValue(Language.ToString(), out var languageInfo)) return;

        var defaultConfiguration = new GenerationConfiguration();
        if (!string.IsNullOrEmpty(languageInfo.ClientClassName) &&
            ClientClassName.Equals(defaultConfiguration.ClientClassName, StringComparison.Ordinal) &&
            !ClientClassName.Equals(languageInfo.ClientClassName, StringComparison.Ordinal))
            ClientClassName = languageInfo.ClientClassName;
        if (!string.IsNullOrEmpty(languageInfo.ClientNamespaceName) &&
            ClientNamespaceName.Equals(defaultConfiguration.ClientNamespaceName, StringComparison.Ordinal) &&
            !ClientNamespaceName.Equals(languageInfo.ClientNamespaceName, StringComparison.Ordinal))
            ClientNamespaceName = languageInfo.ClientNamespaceName;
        if (languageInfo.StructuredMimeTypes.Any() &&
            comparer.Equals(StructuredMimeTypes, defaultConfiguration.StructuredMimeTypes) &&
            !comparer.Equals(languageInfo.StructuredMimeTypes, StructuredMimeTypes))
            StructuredMimeTypes = new List<string>(languageInfo.StructuredMimeTypes);
    }
}
#pragma warning restore CA1056
#pragma warning restore CA1002
#pragma warning restore CA2227
