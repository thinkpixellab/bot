using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Pixel Lab")]
[assembly: AssemblyCopyright("Copyright © 2010, 2011 Pixel Lab. All rights reserved.")]
[assembly: AssemblyVersion("3.0.0")]
[assembly: AssemblyFileVersion("3.0.0")]
[assembly: AssemblyDescription("https://github.com/thinkpixellab/bot/")]

#if (DEBUG)
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
