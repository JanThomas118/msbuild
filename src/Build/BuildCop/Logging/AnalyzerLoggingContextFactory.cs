﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.BackEnd.Logging;
using Microsoft.Build.Experimental.BuildCop;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildCop.Logging;
internal class AnalyzerLoggingContextFactory(ILoggingService loggingService) : IBuildAnalysisLoggingContextFactory
{
    public IBuildAnalysisLoggingContext CreateLoggingContext(BuildEventContext eventContext) =>
        new AnalyzerLoggingContext(loggingService, eventContext);
}
