namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System.Collections;
    using System.IO;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Pkcs;

    public class Certificado
    {
        private string path = string.Empty;
        private string password = string.Empty;
        
        public Certificado(string cpath)
        {
            this.path = cpath;
            this.ProcessarCertificado();
        }

        public Certificado(string cpath, string cpassword)
        {
            this.path = cpath;
            this.Password = cpassword;
            this.ProcessarCertificado();
        }

        public Org.BouncyCastle.X509.X509Certificate[] Chain
        {
            get;
            set;
        }

        public AsymmetricKeyParameter Akp
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        private void ProcessarCertificado()
        {
            string alias = null;

            var pk12 = new Pkcs12Store(new FileStream(this.Path, FileMode.Open, FileAccess.Read), this.password.ToCharArray());

            IEnumerator i = (IEnumerator) pk12.Aliases;

            while (i.MoveNext())
            {
                alias = (string)i.Current;

                if (pk12.IsKeyEntry(alias))
                {
                    break;
                }
            }

            this.Akp = pk12.GetKey(alias).Key;

            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);

            this.Chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];

            for (int k = 0; k < ce.Length; ++k)
            {
                this.Chain[k] = ce[k].Certificate;
            }
        }
    }
}