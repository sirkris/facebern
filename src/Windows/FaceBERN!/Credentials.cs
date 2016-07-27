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
        private SecureString twitterUsername;
        private SecureString twitterUserID;

        private SecureString birdieUsername;
        private SecureString birdiePassword;

        private RegistryKey softwareKey;
        private RegistryKey appKey;
        private RegistryKey credentialsKey;
        private RegistryKey facebookKey;
        private RegistryKey twitterKey;
        private RegistryKey birdieKey;

        private byte[] facebookEntropy;
        private byte[] twitterEntropy;
        private byte[] birdieEntropy;

        internal Credentials(bool loadFacebook = false, bool loadTwitter = false, bool loadBirdie = false)
        {
            try
            {
                softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                appKey = softwareKey.CreateSubKey("FaceBERN!");
                credentialsKey = appKey.CreateSubKey("Credentials");

                facebookKey = credentialsKey.CreateSubKey("Facebook");
                twitterKey = credentialsKey.CreateSubKey("Twitter");
                birdieKey = credentialsKey.CreateSubKey("Birdie");

                SaveKeys();
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                return;
            }

            if (loadFacebook)
            {
                LoadFacebook();
            }

            if (loadTwitter)
            {
                LoadTwitter();
            }

            if (loadBirdie)
            {
                LoadBirdie();
            }
        }

        internal bool SetFacebook(string username, string password)
        {
            return SetFacebook(ToSecureString(username), ToSecureString(password));
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

        internal bool SetTwitter(string accessToken, string accessTokenSecret, string username = null, string userId = null)
        {
            return SetTwitter(ToSecureString(accessToken), ToSecureString(accessTokenSecret), ToSecureString(username), ToSecureString(userId));
        }

        internal bool SetTwitter(SecureString accessToken, SecureString accessTokenSecret, SecureString username = null, SecureString userId = null)
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
                byte[] u = (username != null ? ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(username)), twitterEntropy, DataProtectionScope.CurrentUser) : null);
                byte[] uID = (userId != null ? ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(userId)), twitterEntropy, DataProtectionScope.CurrentUser) : null);
                
                twitterKey.SetValue("entropy", twitterEntropy, RegistryValueKind.Binary);
                twitterKey.SetValue("tAC", tAC, RegistryValueKind.Binary);
                twitterKey.SetValue("tACS", tACS, RegistryValueKind.Binary);

                if (u != null)
                {
                    twitterKey.SetValue("u", u, RegistryValueKind.Binary);
                }

                if (uID != null)
                {
                    twitterKey.SetValue("uID", uID, RegistryValueKind.Binary);
                }

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
                byte[] u = (byte[]) twitterKey.GetValue("u", null);
                byte[] uID = (byte[]) twitterKey.GetValue("uID", null);

                twitterAccessToken = (tAC != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(tAC, twitterEntropy, DataProtectionScope.CurrentUser))) : null);
                twitterAccessTokenSecret = (tACS != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(tACS, twitterEntropy, DataProtectionScope.CurrentUser))) : null);
                twitterUsername = (u != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(u, twitterEntropy, DataProtectionScope.CurrentUser))) : null);
                twitterUserID = (uID != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(uID, twitterEntropy, DataProtectionScope.CurrentUser))) : null);
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                return false;
            }

            return true;
        }

        internal bool SetBirdie(string username, string password)
        {
            return SetBirdie(ToSecureString(username), ToSecureString(password));
        }

        internal bool SetBirdie(SecureString username, SecureString password)
        {
            try
            {
                birdieUsername = username;
                birdiePassword = password;

                birdieEntropy = (byte[]) birdieKey.GetValue("entropy", null);

                if (birdieEntropy == null)
                {
                    birdieEntropy = new byte[20];
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(birdieEntropy);
                    }
                }

                byte[] u = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(username)), birdieEntropy, DataProtectionScope.CurrentUser);
                byte[] p = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToString(password)), birdieEntropy, DataProtectionScope.CurrentUser);

                birdieKey.SetValue("entropy", birdieEntropy, RegistryValueKind.Binary);
                birdieKey.SetValue("u", u, RegistryValueKind.Binary);
                birdieKey.SetValue("p", p, RegistryValueKind.Binary);

                birdieKey.Flush();
            }
            catch (IOException e)
            {
                // TODO - Log the exception.  --Kris

                birdieUsername = null;
                birdiePassword = null;

                return false;
            }

            return true;
        }

        internal bool LoadBirdie()
        {
            try
            {
                birdieEntropy = (byte[]) birdieKey.GetValue("entropy", null);
                byte[] u = (byte[]) birdieKey.GetValue("u", null);
                byte[] p = (byte[]) birdieKey.GetValue("p", null);

                birdieUsername = (u != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(u, birdieEntropy, DataProtectionScope.CurrentUser))) : null);
                birdiePassword = (p != null ? ToSecureString(Encoding.Unicode.GetString(ProtectedData.Unprotect(p, birdieEntropy, DataProtectionScope.CurrentUser))) : null);
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
            DestroyFacebook(clearRegistry);
            DestroyTwitter(clearRegistry);
            DestroyBirdie(clearRegistry);

            facebookKey.Close();
            twitterKey.Close();
            birdieKey.Close();
            credentialsKey.Close();
            appKey.Close();
            softwareKey.Close();
        }

        internal void DestroyFacebook(bool clearRegistry = false)
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

                SaveKeys();
            }
        }

        internal void DestroyTwitter(bool clearRegistry = false)
        {
            if (twitterAccessToken != null)
            {
                twitterAccessToken.Dispose();
            }
            if (twitterAccessTokenSecret != null)
            {
                twitterAccessTokenSecret.Dispose();
            }
            if (twitterUsername != null)
            {
                twitterUsername.Dispose();
            }
            if (twitterUserID != null)
            {
                twitterUserID.Dispose();
            }

            if (clearRegistry)
            {
                twitterKey.DeleteValue("entropy", false);
                twitterKey.DeleteValue("tAC", false);
                twitterKey.DeleteValue("tACS", false);
                twitterKey.DeleteValue("u", false);
                twitterKey.DeleteValue("uID", false);

                SaveKeys();
            }
        }

        internal void DestroyBirdie(bool clearRegistry = false)
        {
            if (birdieUsername != null)
            {
                birdieUsername.Dispose();
            }
            if (birdiePassword != null)
            {
                birdiePassword.Dispose();
            }

            if (clearRegistry)
            {
                birdieKey.DeleteValue("entropy", false);
                birdieKey.DeleteValue("u", false);
                birdieKey.DeleteValue("p", false);

                SaveKeys();
            }
        }

        private void SaveKeys()
        {
            facebookKey.Flush();
            twitterKey.Flush();
            birdieKey.Flush();
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

        internal SecureString GetTwitterUsername()
        {
            return twitterUsername;
        }

        internal SecureString GetTwitterUserID()
        {
            return twitterUserID;
        }

        internal SecureString GetBirdieUsername()
        {
            return birdieUsername;
        }

        internal SecureString GetBirdiePassword()
        {
            return birdiePassword;
        }

        internal bool IsAssociated()
        {
            return (twitterAccessToken != null && twitterAccessTokenSecret != null && twitterUsername != null && twitterUserID != null);
        }

        internal SecureString ToSecureString(string str)
        {
            if (str == null)
            {
                return null;
            }

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
            catch (Exception e)
            {
                return null;
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
