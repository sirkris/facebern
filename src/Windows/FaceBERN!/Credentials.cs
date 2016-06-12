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

        private SecureString twitterAccessToken;
        private SecureString twitterAccessTokenSecret;

        private RegistryKey softwareKey;
        private RegistryKey appKey;
        private RegistryKey credentialsKey;
        private RegistryKey facebookKey;
        private RegistryKey twitterKey;

        private byte[] facebookEntropy;
        private byte[] twitterEntropy;

        internal Credentials()
        {
            try
            {
                softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                appKey = softwareKey.CreateSubKey("FaceBERN!");
                credentialsKey = appKey.CreateSubKey("Credentials");

                facebookKey = credentialsKey.CreateSubKey("Facebook");
                twitterKey = credentialsKey.CreateSubKey("Twitter");

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

                facebookEntropy = (byte[]) facebookKey.GetValue("entropy", null);
                
                if (facebookEntropy == null)
                {
                    facebookEntropy = new byte[20];
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

        internal bool SetTwitter(SecureString accessToken, SecureString accessTokenSecret)
        {
            try
            {
                twitterAccessToken = accessToken;
                twitterAccessTokenSecret = accessTokenSecret;

                twitterEntropy = (byte[]) facebookKey.GetValue("entropy", null);

                if (twitterEntropy == null)
                {
                    twitterEntropy = new byte[20];
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(twitterEntropy);
                    }
                }

                byte[] tAC = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(accessToken)), twitterEntropy, DataProtectionScope.CurrentUser);
                byte[] tACS = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(accessTokenSecret)), twitterEntropy, DataProtectionScope.CurrentUser);

                twitterKey.SetValue("entropy", twitterEntropy, RegistryValueKind.Binary);
                twitterKey.SetValue("tAC", tAC, RegistryValueKind.Binary);
                twitterKey.SetValue("tACS", tACS, RegistryValueKind.Binary);

                twitterKey.Flush();
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                twitterAccessToken = null;
                twitterAccessTokenSecret = null;

                return false;
            }

            return true;
        }

        internal bool LoadTwitter()
        {
            try
            {
                twitterEntropy = (byte[]) twitterKey.GetValue("entropy", null);
                byte[] tAC = (byte[]) twitterKey.GetValue("tAC", null);
                byte[] tACS = (byte[]) twitterKey.GetValue("tACS", null);

                twitterAccessToken = (tAC != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(tAC, facebookEntropy, DataProtectionScope.CurrentUser))) : null);
                twitterAccessTokenSecret = (tACS != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(tACS, facebookEntropy, DataProtectionScope.CurrentUser))) : null);
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                return false;
            }

            return true;
        }

        internal void Destroy(bool clearRegistry = false)
        {
            if (facebookUsername != null)
            {
                facebookUsername.Dispose();
            }
            if (facebookPassword != null)
            {
                facebookPassword.Dispose();
            }

            if (clearRegistry)
            {
                facebookKey.DeleteValue("entropy", false);
                facebookKey.DeleteValue("cU", false);
                facebookKey.DeleteValue("cP", false);

                twitterKey.DeleteValue("entropy", false);
                twitterKey.DeleteValue("tAC", false);
                twitterKey.DeleteValue("tACS", false);
            }

            facebookKey.Close();
            twitterKey.Close();
            credentialsKey.Close();
            appKey.Close();
            softwareKey.Close();
        }

        private void SaveKeys()
        {
            facebookKey.Flush();
            twitterKey.Flush();
            credentialsKey.Flush();
            appKey.Flush();
            softwareKey.Flush();
        }

        internal SecureString GetFacebookUsername()
        {
            return facebookUsername;
        }

        internal SecureString GetFacebookPassword()
        {
            return facebookPassword;
        }

        internal SecureString GetTwitterAccessToken()
        {
            return twitterAccessToken;
        }

        internal SecureString GetTwitterAccessTokenSecret()
        {
            return twitterAccessTokenSecret;
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
