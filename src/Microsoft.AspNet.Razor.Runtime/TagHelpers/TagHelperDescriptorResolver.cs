﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Used to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDescriptorResolver : ITagHelperDescriptorResolver
    {
        private readonly TagHelperTypeResolver _typeResolver;

        // internal for testing
        internal TagHelperDescriptorResolver(TagHelperTypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperDescriptorResolver"/> class.
        /// </summary>
        public TagHelperDescriptorResolver()
            : this(new TagHelperTypeResolver())
        {
        }

        /// <inheritdoc />
        public IEnumerable<TagHelperDescriptor> Resolve(string lookupText)
        {
            var lookupStrings = lookupText?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Ensure that we have valid lookupStrings to work with. Valid formats are:
            // "assemblyName"
            // "typeName, assemblyName"
            if (string.IsNullOrEmpty(lookupText) ||
                (lookupStrings.Length != 1 && lookupStrings.Length != 2))
            {
                throw new ArgumentException(
                    Resources.FormatTagHelperDescriptorResolver_InvalidTagHelperLookupText(lookupText),
                    nameof(lookupText));
            }

            // Grab the assembly name from the lookup text strings. Due to our supported lookupText formats it will 
            // always be the last element provided.
            var assemblyName = lookupStrings.Last().Trim();
            var descriptors = ResolveDescriptorsInAssembly(assemblyName);

            // Check if the lookupText specifies a type to search for.
            if (lookupStrings.Length == 2)
            {
                // The user provided a type name. Retrieve it so we can prune our descriptors.
                var typeName = lookupStrings[0].Trim();

                descriptors = descriptors.Where(descriptor =>
                    string.Equals(descriptor.TypeName, typeName, StringComparison.Ordinal));
            }

            return descriptors;
        }

        /// <summary>
        /// Resolves all <see cref="TagHelperDescriptor"/>s for <see cref="ITagHelper"/>s from the given 
        /// <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly to resolve <see cref="TagHelperDescriptor"/>s from.
        /// </param>
        /// <returns><see cref="TagHelperDescriptor"/>s for <see cref="ITagHelper"/>s from the given
        /// <paramref name="assemblyName"/>.</returns>
        // This is meant to be overridden by tooling to enable assembly level caching.
        protected virtual IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(string assemblyName)
        {
            // Resolve valid tag helper types from the assembly.
            var tagHelperTypes = _typeResolver.Resolve(assemblyName);

            // Convert types to TagHelperDescriptors
            var descriptors = tagHelperTypes.SelectMany(TagHelperDescriptorFactory.CreateDescriptors);

            return descriptors;
        }
    }
}