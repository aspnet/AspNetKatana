// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CertInstaller
{
    public class CertificateInstaller
    {
        private readonly X509Certificate2 _cert;

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
            _cert = new X509Certificate2(certFilePath, password,
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
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            try
            {
                X509Certificate2Collection result = store.Certificates.Find(
                    X509FindType.FindByThumbprint, _cert.Thumbprint, false);

                if (result.Count > 0)
                {
                    Trace.TraceWarning("Certificate with thumbprint '{0}', name '{1}' already in store.",
                        _cert.Thumbprint, _cert.Subject);
                }
                else
                {
                    store.Add(_cert);
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
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            try
            {
                X509Certificate2Collection result = store.Certificates.Find(
                    X509FindType.FindByThumbprint, _cert.Thumbprint, false);

                if (result.Count > 0)
                {
                    store.Remove(_cert);
                    Trace.TraceInformation("Certificate successfully removed from the store.");
                }
                else
                {
                    Trace.TraceWarning("Certificate with thumbprint '{0}', name '{1}' not found in store.",
                        _cert.Thumbprint, _cert.Subject);
                }
            }
            finally
            {
                store.Close();
            }
        }
    }
}
