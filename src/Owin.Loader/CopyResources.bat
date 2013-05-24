rem The loder code is shared between projects, but the resource files cannot be due to msbuild constraints.
rem To add or edit resources, do so in Owin.Loader, run this script to copy the resx file,
rem and then touch the resx files in the destination projects so they regenerate the designer files.
rem In the future fix this by moving loader into a shared assembly.
copy LoaderResources.resx ..\Microsoft.Owin.Hosting\App_Packages\Owin.Loader\LoaderResources.resx
copy LoaderResources.resx ..\Microsoft.Owin.Host.SystemWeb\App_Packages\Owin.Loader\LoaderResources.resx
pause