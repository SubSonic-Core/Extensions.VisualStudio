using FluentAssertions;
using NUnit.Framework;
using SubSonic.Core.VisualStudio.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Testing.SubSonic.Core.VisualStudio.Common
{
    [TestFixture]
    public class RemoteTransformationHostTests
    {
        [Test]
        [TestCase("SubSonic.Core.Abstractions", true, @"C:\Users\kccar\.nuget\packages\subsonic.core.abstractions\4.2.2\lib\netstandard2.1")]
        [TestCase("SubSonic.Core.NotGonnaFindIt", false, @"SubSonic.Core.NotGonnaFindIt")]
        public void CanResolveAssemblyReferenceByNuget(string assemblyReference, bool isPathRooted, string expectedPath)
        {
            RemoteTransformationHost host = new RemoteTransformationHost();

            host.TryResolveAssemblyReferenceByNuget(assemblyReference, out string assemblyPath).Should().Be(isPathRooted);

            Path.IsPathRooted(assemblyPath).Should().Be(isPathRooted);

            if (Path.IsPathRooted(assemblyPath))
            {
                Path.GetDirectoryName(assemblyPath).Should().BeEquivalentTo(expectedPath);
            }
            else
            {
                assemblyPath.Should().Be(expectedPath);
            }
        }
    }
}
