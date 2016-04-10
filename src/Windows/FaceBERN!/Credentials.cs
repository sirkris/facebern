using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    internal class Credentials
    {
        private SecureString facebookUsername;
        private SecureString facebookPassword;

        private RegistryKey softwareKey;
        private RegistryKey appKey;
        private RegistryKey credentialsKey;
        private RegistryKey facebookKey;

        private byte[] facebookEntropy;

        internal Credentials()
        {
            try
            {
                softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                appKey = softwareKey.CreateSubKey("FaceBERN!");
                credentialsKey = appKey.CreateSubKey("Credentials");

                facebookKey = credentialsKey.CreateSubKey("Facebook");

                SaveKeys();
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                return;
            }

            LoadFacebook();
        }

        internal bool SetFacebook(SecureString username, SecureString password)
        {
            try
            {
                facebookUsername = username;
                facebookPassword = password;

                facebookEntropy = (byte[]) facebookKey.GetValue("entropy", new byte[20]);
                
                if (facebookEntropy == null)
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(facebookEntropy);
                    }
                }

                byte[] cU = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(username)), facebookEntropy, DataProtectionScope.CurrentUser);
                byte[] cP = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(password)), facebookEntropy, DataProtectionScope.CurrentUser);

                facebookKey.SetValue("entropy", facebookEntropy, RegistryValueKind.Binary);
                facebookKey.SetValue("cU", cU, RegistryValueKind.Binary);
                facebookKey.SetValue("cP", cP, RegistryValueKind.Binary);

                facebookKey.Flush();
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                facebookUsername = null;
                facebookPassword = null;

                return false;
            }

            return true;
        }

        internal bool LoadFacebook()
        {
            try
            {
                facebookEntropy = (byte[]) facebookKey.GetValue("entropy", null);
                byte[] cU = (byte[]) facebookKey.GetValue("cU", null);
                byte[] cP = (byte[]) facebookKey.GetValue("cP", null);

                facebookUsername = (cU != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(cU, facebookEntropy, DataProtectionScope.CurrentUser))) : null);
                facebookPassword = (cP != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(cP, facebookEntropy, DataProtectionScope.CurrentUser))) : null);
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                return false;
            }

            return true;
        }

        internal void Destroy()
        {
            facebookUsername.Dispose();
            facebookPassword.Dispose();

            facebookKey.Close();
            credentialsKey.Close();
            appKey.Close();
            softwareKey.Close();
        }

        private void SaveKeys()
        {
            facebookKey.Flush();
            credentialsKey.Flush();
            appKey.Flush();
            softwareKey.Flush();
        }

        internal SecureString GetUsername()
        {
            return facebookUsername;
        }

        internal SecureString GetPassword()
        {
            return facebookPassword;
        }

        internal SecureString ToSecureString(string str)
        {
            SecureString secureString = new SecureString();

            Array.ForEach(str.ToCharArray(), secureString.AppendChar);

            return secureString;
        }

        // Use this sparingly and don't store the return in a variable if you can avoid it.  Should only be used for on-the-fly conversion when absolutely needed.  --Kris
        internal string ToString(SecureString sStr)
        {
            if (sStr == null)
            {
                return null;
            }
            
            IntPtr output = IntPtr.Zero;

            try
            {
                output = Marshal.SecureStringToGlobalAllocUnicode(sStr);

                return Marshal.PtrToStringUni(output);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(output);
            }
        }

        private byte[] ToByteArray(SecureString sStr)
        {
            byte[] res = new byte[sStr.Length * 2];
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(sStr);
                for (int i = 0; i < sStr.Length; i++)
                {
                    res[i] = Marshal.ReadByte(ptr, (i * 2));
                    res[i + 1] = Marshal.ReadByte(ptr, ((i * 2) + 1));
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }

            return res;
        }

        private SecureString FromByteArray(byte[] b)
        {
            SecureString res = new SecureString();
            UnicodeEncoding bc = new UnicodeEncoding();
            foreach (char c in bc.GetChars(b))
            {
                res.AppendChar(c);
            }

            return res;
        }
    }
}
