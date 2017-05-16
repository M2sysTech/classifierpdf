namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.security;
    using System.Security.Cryptography;
    using LogHelpers;

    public class PdfSigner : IPdfSigner
    {
        public void Verify()
        {
        }
        
        public void Sign(SingInfo singInfo)
        {
            try
            {
                var metaData = new MetaData
                {
                    Author = singInfo.Author,
                    Title = singInfo.Title,
                    Subject = singInfo.Subject,
                    Keywords = singInfo.Keywords,
                    Creator = singInfo.Creator,
                    Producer = singInfo.Producer
                };

                var store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                if (store.Certificates.Count == 0)
                {
                    Log.Application.Error("Certificados Locais do usuario indisponiveis.");
                }

                var posicaoCertificado = 0;

                var cert = store.Certificates[posicaoCertificado];

                Log.Application.InfoFormat(
                    "Certificado Selecionado {0} Serial {1} Subject {2}", 
                    cert.IssuerName.Name, 
                    cert.SerialNumber, 
                    cert.SubjectName);

                var cp = new Org.BouncyCastle.X509.X509CertificateParser();

                var chain = new[]
                {
                    cp.ReadCertificate(cert.RawData)
                };

                IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");

                var rsa = (RSACryptoServiceProvider)cert.PrivateKey;

                var secureString = new SecureString();
                
                secureString.AppendChar(char.Parse("1"));
                secureString.AppendChar(char.Parse("2"));
                secureString.AppendChar(char.Parse("3"));
                secureString.AppendChar(char.Parse("4"));
                
                var cspp = new CspParameters();
                cspp.KeyContainerName = rsa.CspKeyContainerInfo.KeyContainerName;
                cspp.ProviderName = rsa.CspKeyContainerInfo.ProviderName;
                cspp.Flags = CspProviderFlags.UseUserProtectedKey;
                cspp.KeyPassword = secureString;

                cspp.ProviderType = rsa.CspKeyContainerInfo.ProviderType;

                cspp.Flags = CspProviderFlags.NoPrompt;

                RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider(cspp);

                rsa.PersistKeyInCsp = true;

                PdfReader reader = new PdfReader(singInfo.SourcePdf);
                PdfAStamper stamper = PdfAStamper.CreateSignature(
                    reader, 
                    new FileStream(singInfo.TargetPdf, FileMode.Create, FileAccess.Write), 
                    '\0', 
                    PdfAConformanceLevel.PDF_A_2A);
                
                PdfSignatureAppearance appearance = stamper.SignatureAppearance;

                var caminhoFonte2 = @"C:\Windows\Fonts\cour.ttf";
                var caminhoGrafico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "key.jpg");
                
                appearance.Layer2Font = FontFactory.GetFont(caminhoFonte2, BaseFont.WINANSI, BaseFont.EMBEDDED, 8);
                appearance.Reason = singInfo.SignatureReason;
                appearance.Contact = singInfo.SignatureContact;
                appearance.Location = singInfo.SignatureLocation;
                appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                appearance.CertificationLevel = PdfSignatureAppearance.CERTIFIED_NO_CHANGES_ALLOWED;
                appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(600, 100, 0, 0), 1, "Assinatura");    
                
                MakeSignature.SignDetached(appearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
                stamper.Close();
            }
            catch (Exception exception)
            {
                Log.Application.Error("Falha ao assinar! ", exception);
                throw;
            }
        } 
    }
}