using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Diagnostics;

namespace CertInstaller
{
    public class CertificateInstaller
    {
        private X509Certificate2 cert;

        public CertificateInstaller(string certFilePath, string password)
        {
            if (!File.Exists(certFilePath))
            {
                throw new ArgumentException(String.Format("File '{0}' does not exist.", certFilePath), 
                    "certFilePath");
            }
            
            Trace.TraceInformation("Creating certificate from file '{0}', password '{1}'.", certFilePath,
                password);

            // MachineKeySet: make sure the private key gets stored in the local machine store
            this.cert = new X509Certificate2(certFilePath, password, 
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        }

        public void InstallCertificate()
        {
            InstallCertificate(StoreName.My, StoreLocation.LocalMachine);
            InstallCertificate(StoreName.Root, StoreLocation.LocalMachine);
        }

        public void UninstallCertificate()
        {
            UninstallCertificate(StoreName.My, StoreLocation.LocalMachine);
            UninstallCertificate(StoreName.Root, StoreLocation.LocalMachine);
        }

        private void InstallCertificate(StoreName storeName, StoreLocation storeLocation)
        {
            Trace.TraceInformation("Creating store object for '{1}', '{0}'.", storeName, storeLocation);
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            try
            {
                X509Certificate2Collection result = store.Certificates.Find(
                    X509FindType.FindByThumbprint, cert.Thumbprint, false);

                if (result.Count > 0)
                {
                    Trace.TraceWarning("Certificate with thumbprint '{0}', name '{1}' already in store.",
                        cert.Thumbprint, cert.Subject);
                }
                else
                {
                    store.Add(cert);
                    Trace.TraceInformation("Certificate successfully added to the store.");
                }
            }
            finally
            {
                store.Close();
            }
        }

        private void UninstallCertificate(StoreName storeName, StoreLocation storeLocation)
        {
            Trace.TraceInformation("Removing store object for '{1}', '{0}'.", storeName, storeLocation);
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            try
            {
                X509Certificate2Collection result = store.Certificates.Find(
                    X509FindType.FindByThumbprint, cert.Thumbprint, false);

                if (result.Count > 0)
                {
                    store.Remove(cert);
                    Trace.TraceInformation("Certificate successfully removed from the store.");
                }
                else
                {
                    Trace.TraceWarning("Certificate with thumbprint '{0}', name '{1}' not found in store.",
                        cert.Thumbprint, cert.Subject);
                }
            }
            finally
            {
                store.Close();
            }
        }
    }
}
