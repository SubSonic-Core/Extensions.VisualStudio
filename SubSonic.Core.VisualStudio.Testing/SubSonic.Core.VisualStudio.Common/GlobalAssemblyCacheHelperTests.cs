using FluentAssertions;
using NUnit.Framework;
using SubSonic.Core.VisualStudio.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SubSonic.Core.VisualStudio.Testing.SubSonic.Core.VisualStudio.Common
{
    [TestFixture]
    public class GlobalAssemblyCacheHelperTests
    {
        [Test]
        public void CanIdentifyStrongName()
        {
            string name = "SubSonic.Core, Version=1.1.0.0, Culture=neutral, PublicKeyToken=abcD0E9fghj";

            GlobalAssemblyCacheHelper.IsStrongName(name).Should().BeTrue();
        }

        [Test]
        public void ShouldThroughArgumentNullException()
        {
            FluentActions.Invoking(() =>
            {
                GlobalAssemblyCacheHelper.GetLocation(null);
            }).Should().Throw<ArgumentNullException>();
        }

        [Test]
        
        [TestCase("", "")]
        [TestCase("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", @"C:\WINDOWS\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll")]
        [TestCase("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", @"C:\WINDOWS\Microsoft.Net\assembly\GAC_64\System.Data\v4.0_4.0.0.0__b77a5c561934e089\System.Data.dll")]
        [TestCase("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", @"C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Runtime\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Runtime.dll")]
        [TestCase("System.Data.Common, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a","")]
        public void TestMethod(string strongNameReference, string location)
        {
            string path = null;

            FluentActions.Invoking(() =>
            {
                path = GlobalAssemblyCacheHelper.GetLocation(strongNameReference);
            }).Should().NotThrow();

            path.Should().Be(location);
        }
    }
}
