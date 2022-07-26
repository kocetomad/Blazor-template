// The following example demonstrates the use of the WindowsIdentity class to impersonate a user.   
// IMPORTANT NOTE:   
// This sample asks the user to enter a password on the console screen.   
// The password will be visible on the screen, because the console window   
// does not support masked input natively.  

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

public class ImpersonationDemo
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

    public static void Main()
    {
       
        string domainName = "MUC";
        string userName = "{USR}";
        string userPassword = "{PASS}";
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_INTERACTIVE = 2;
        SafeAccessTokenHandle safeAccessTokenHandle;
        bool returnValue = LogonUser(userName, domainName, userPassword,
            LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
            out safeAccessTokenHandle);
        if (false == returnValue)
        {
            int ret = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(ret);
        }
        // Note: if you want to run as unimpersonated, pass  
        //       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'  
        WindowsIdentity.RunImpersonated(
            safeAccessTokenHandle,
            // User action  
            () =>
            {
                // Check the identity.  
                Console.WriteLine("During impersonation: " + WindowsIdentity.GetCurrent().Name);
            }
            );

        // Check the identity again.  
    }
}