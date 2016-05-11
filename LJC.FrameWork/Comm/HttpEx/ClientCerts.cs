using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace LJC.FrameWork.Comm.ClientCert
{
    internal class ClientCerts
    {
        private static int CERT_STORE_PROV_SYSTEM = 10;
        private static int CERT_SYSTEM_STORE_CURRENT_USER = (1 << 16);
        ///private static int CERT_SYSTEM_STORE_LOCAL_MACHINE = (2 << 16);

        [DllImport("CRYPT32", EntryPoint = "CertOpenStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CertOpenStore(
            int storeProvider, int encodingType,
            int hcryptProv, int flags, string pvPara);

        [DllImport("CRYPT32", EntryPoint = "CertEnumCertificatesInStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CertEnumCertificatesInStore(
            IntPtr storeProvider,
            IntPtr prevCertContext);

        [DllImport("CRYPT32", EntryPoint = "CertCloseStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CertCloseStore(
            IntPtr storeProvider,
            int flags);

        X509CertificateCollection m_certs;

        public ClientCerts()
        {
            m_certs = new X509CertificateCollection();
        }

        public int Init()
        {
            m_certs.Clear();

            IntPtr storeHandle;
            storeHandle = CertOpenStore(CERT_STORE_PROV_SYSTEM, 0, 0, CERT_SYSTEM_STORE_CURRENT_USER, "MY");
            IntPtr currentCertContext;
            currentCertContext = CertEnumCertificatesInStore(storeHandle, (IntPtr)0);
            int i = 0;
            while (currentCertContext != (IntPtr)0)
            {
                m_certs.Insert(i++, new X509Certificate(currentCertContext));
                currentCertContext = CertEnumCertificatesInStore(storeHandle, currentCertContext);
            }
            CertCloseStore(storeHandle, 0);

            return m_certs.Count;
        }

        public X509Certificate this[int index]
        {
            get
            {
                // Check the index limits.
                if (index < 0 || index > m_certs.Count)
                    return null;
                else
                    return m_certs[index];
            }
        }

    }
}
