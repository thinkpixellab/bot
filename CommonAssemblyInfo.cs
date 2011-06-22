using System;
using System.Reflection;
using System.Runtime.InteropServices;

#region General information
[assembly: AssemblyCompany("Pixel Lab")]
[assembly: AssemblyCopyright("Copyright © 2010, 2011 Pixel Lab. All rights reserved.")]
[assembly: AssemblyVersion("1.1.0")]
[assembly: AssemblyFileVersion("1.1.0")]
[assembly: AssemblyDescription("https://github.com/thinkpixellab/bot/")]

#if (DEBUG)
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

#endregion

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
