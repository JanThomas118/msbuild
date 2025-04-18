// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Xunit;

#nullable disable

namespace Microsoft.Build.UnitTests
{
    public sealed class AssignCulture_Tests
    {
        /// <summary>
        /// Tests the basic functionality.
        /// </summary>
        [Fact]
        public void Basic()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("fr", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Any pre-existing Culture attribute on the item is to be ignored
        /// </summary>
        [Fact]
        public void CultureAttributePrecedence()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("Culture", "en-GB");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("fr", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// This is really a corner case.
        /// If the incoming item has a 'Culture' attribute already, but that culture is invalid,
        /// we still overwrite that culture.
        /// </summary>
        [Fact]
        public void CultureAttributePrecedenceWithBogusCulture()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("Culture", "invalid");   // Bogus culture.
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("fr", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Make sure that attributes set on input items are forwarded to output items.
        /// This applies to every attribute except for the one pointed to by CultureAttribute.
        /// </summary>
        [Fact]
        public void AttributeForwarding()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("MyAttribute", "My Random String");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("fr", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("My Random String", t.AssignedFiles[0].GetMetadata("MyAttribute"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }


        /// <summary>
        /// Test the case where an item has no embedded culture. For example:
        /// "MyResource.resx"
        /// </summary>
        [Fact]
        public void NoCulture()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(String.Empty, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Test the case where an item has no extension. For example "MyResource".
        /// </summary>
        [Fact]
        public void NoExtension()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(String.Empty, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        ///  Test the case where an item has two dots embedded, but otherwise looks
        /// like a well-formed item.For example "MyResource..resx".
        /// </summary>
        [Fact]
        public void DoubleDot()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource..resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(String.Empty, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource..resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource..resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// If an item has a "DependentUpon" who's base name matches exactly, then just assume this
        /// is a resource and form that happen to have an embedded culture. That is, don't assign a
        /// culture to these.
        /// </summary>
        [Fact]
        public void Regress283991()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("DependentUpon", "MyResourcE.fr.vb");
            t.Files = new ITaskItem[] { i };

            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Empty(t.AssignedFilesWithCulture);
            Assert.Single(t.AssignedFilesWithNoCulture);
        }

        /// <summary>
        /// Test the usage of Windows Pseudo-Locales
        /// https://docs.microsoft.com/en-gb/windows/desktop/Intl/pseudo-locales
        /// </summary>
        /// <param name="culture"></param>
        [Theory]
        [InlineData("qps-ploc")]
        [InlineData("qps-plocm")]
        [InlineData("qps-ploca")]
        [InlineData("qps-Latn-x-sh")] // Windows 10+
        public void PseudoLocalization(string culture)
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem($"MyResource.{culture}.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(culture, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal($"MyResource.{culture}.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Testing that certain aliases are considered valid cultures. Regression test for https://github.com/dotnet/msbuild/issues/3897.
        /// </summary>
        /// <param name="culture"></param>
        [Theory]
        [InlineData("zh-TW")]
        [InlineData("zh-MO")]
        public void SupportAliasedCultures(string culture)
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem($"MyResource.{culture}.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(culture, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal($"MyResource.{culture}.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        [DotNetOnlyTheory(additionalMessage: "These cultures are not returned via Culture api on net472.")]
        [InlineData("sh-BA")]
        [InlineData("shi-MA")]
        public void AliasedCultures_SupportedOnNetCore(string culture)
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem($"MyResource.{culture}.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal(culture, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal($"MyResource.{culture}.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        [DotNetOnlyFact(additionalMessage: "Pseudoloc is special-cased in .NET relative to Framework.")]
        public void Pseudolocales_CaseInsensitive()
        {
            string culture = "qps-Ploc";
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem($"MyResource.{culture}.resx");
            t.Files = new ITaskItem[] { i };
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("true", t.AssignedFiles[0].GetMetadata("WithCulture"));
            Assert.Equal(culture, t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal($"MyResource.{culture}.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Any pre-existing Culture attribute on the item is to be respected
        /// </summary>
        [Fact]
        public void CultureMetaDataShouldBeRespected()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("Culture", "en-GB");
            t.Files = new ITaskItem[] { i };
            t.RespectAlreadyAssignedItemCulture = true;
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("en-GB", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.fr.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }

        /// <summary>
        /// Any pre-existing Culture attribute on the item is not to be respected, because culture is not set
        /// </summary>
        [Fact]
        public void CultureMetaDataShouldNotBeRespected()
        {
            AssignCulture t = new AssignCulture();
            t.BuildEngine = new MockEngine();
            ITaskItem i = new TaskItem("MyResource.fr.resx");
            i.SetMetadata("Culture", "");
            t.Files = new ITaskItem[] { i };
            t.RespectAlreadyAssignedItemCulture = true;
            t.Execute();

            Assert.Single(t.AssignedFiles);
            Assert.Single(t.CultureNeutralAssignedFiles);
            Assert.Equal("fr", t.AssignedFiles[0].GetMetadata("Culture"));
            Assert.Equal("MyResource.fr.resx", t.AssignedFiles[0].ItemSpec);
            Assert.Equal("MyResource.resx", t.CultureNeutralAssignedFiles[0].ItemSpec);
        }
    }
}
