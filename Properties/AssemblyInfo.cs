using Microsoft.VisualStudio.Shell;
using SubSonic.Core.VisualStudio;
using SubSonic.Core.VisualStudio.Attributes;
using SubSonic.Core.VisualStudio.CustomTools;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SubSonic.Core.VisualStudio")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SubSonic.Core.VisualStudio")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: ProvideAssemblyObject(typeof(SubSonicTemplatingFileGenerator))]
[assembly: ProvideAssemblyObject(typeof(ISubSonicTemplatingService), RegistrationMethod = RegistrationMethod.Assembly)]
[assembly: ProvideAssemblyObject(typeof(IDataConnection), RegistrationMethod = RegistrationMethod.Assembly)]
[assembly: ProvideAssemblyObject(typeof(IConnectionManager), RegistrationMethod = RegistrationMethod.Assembly)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
