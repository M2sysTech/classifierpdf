namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using LogHelpers;
    using Org.BouncyCastle.Security;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.security;

    public class TimeStampSigner
    {
        private const int EstimatedSize = 0;
        private readonly CertificadoA3 certificadoA3;
        private ICollection<Org.BouncyCastle.X509.X509Certificate> chain;
        private IOcspClient ocspClient;
        private ICollection<ICrlClient> crlList;
        private ITSAClient tsaClient;
        private ICollection<Org.BouncyCastle.X509.X509Certificate> cadeiaTempo;

        public TimeStampSigner(
            CertificadoA3 certificadoA3)
        {
            this.certificadoA3 = certificadoA3;
        }

        public string Execute(SingInfo singInfo)
        {
            var certificate = this.certificadoA3.Obter();
            this.cadeiaTempo = this.certificadoA3.CadeiaCertificadoTempo();

            this.MontarEstruturaCertificacao(certificate);

            var pdfAux = Path.Combine(
                Path.GetDirectoryName(singInfo.SourcePdf), 
                Path.GetFileNameWithoutExtension(singInfo.SourcePdf) + "-aux.pdf");

            using (var reader = new PdfReader(singInfo.SourcePdf))
            {
                using (var os = new FileStream(pdfAux, FileMode.Create))
                {
                    var stamper = PdfAStamper.CreateSignature(reader, os, '\0', PdfAConformanceLevel.PDF_A_2A);

                    var appearance = stamper.SignatureAppearance;

                    var caminhoFonte2 = @"C:\Windows\Fonts\cour.ttf";
                    appearance.Layer2Font = FontFactory.GetFont(caminhoFonte2, BaseFont.WINANSI, BaseFont.EMBEDDED, 8);
                    appearance.Reason = singInfo.SignatureReason;
                    appearance.Contact = singInfo.SignatureContact;
                    appearance.Location = singInfo.SignatureLocation;
                    
                    appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                    
                    var pks = new X509Certificate2Signature(certificate, DigestAlgorithms.SHA256);

                    MakeSignature.SignDetached(
                        appearance,
                        pks, 
                        this.chain, 
                        this.crlList, 
                        this.ocspClient,
                        this.tsaClient, 
                        EstimatedSize,
                        CryptoStandard.CMS);
                }
            }
            
            using (var reader = new PdfReader(pdfAux))
            {
                using (var os = new FileStream(singInfo.TargetPdf, FileMode.Create))
                {
                    var stamper = PdfStamper.CreateSignature(reader, os, '\0', null, true);
                    var appearance = stamper.SignatureAppearance;

                    this.AdicionarLtv(stamper, appearance);
                }
            }

            this.RemoverArquivoTemporario(pdfAux);
            
            return singInfo.TargetPdf;
        }

        private void RemoverArquivoTemporario(string pdfAux)
        {
            try
            {
                if (File.Exists(pdfAux))
                {
                    File.Delete(pdfAux);
                    Log.Application.Warn("Arquivo auxiliar removido com sucesso");
                }
            }   
            catch (Exception exception)
            {
                Log.Application.Warn("Nao consegui remover arquivo temporario. " + exception.Message);
            }
        }

        private void AdicionarLtv(PdfStamper stamper, PdfSignatureAppearance appearance)
        {
            Log.Application.Info("Validacao iniciado");
            var v = stamper.LtvVerification;
            var fields = stamper.AcroFields;
            var names = fields.GetSignatureNames();

            Log.Application.Info("fields-" + fields.TotalRevisions);
            Log.Application.Info("names-" + names.Count);

            var sigName = names[names.Count - 1];
            var pkcs7 = fields.VerifySignature(sigName);

            Log.Application.Info("verificando tsp");
            
            if (pkcs7.IsTsp)
            {
                Log.Application.Info("Adicionando verificacao para " + sigName);

                v.AddVerification(
                    sigName, 
                    this.ocspClient,
                    new CrlClientOnline(this.cadeiaTempo), 
                    LtvVerification.CertificateOption.SIGNING_CERTIFICATE, 
                    LtvVerification.Level.OCSP_CRL,
                    LtvVerification.CertificateInclusion.YES);
            }
            else
            {
                foreach (var name in names)
                {
                    Log.Application.Info("Adicionando verificacao para nomes" + name);
                    v.AddVerification(
                        name, 
                        this.ocspClient,
                        new CrlClientOnline(this.cadeiaTempo), 
                        LtvVerification.CertificateOption.WHOLE_CHAIN, 
                        LtvVerification.Level.OCSP_CRL,
                        LtvVerification.CertificateInclusion.YES);
                }
            }
            
            LtvTimestamp.Timestamp(appearance, this.tsaClient, null);
        }

        private void MontarEstruturaCertificacao(X509Certificate2 certificate)
        {
            this.chain = this.GetChain(certificate);

            foreach (var cadeia in this.chain)
            {
                Log.Application.Debug(cadeia.ToString());
            }

            Log.Application.Debug("Conseguiu pegar valor da cadeia? " + this.chain != null);

            this.ocspClient = new OcspClientBouncyCastle(null);

            this.crlList = new List<ICrlClient>
            {
                new CrlClientOnline(this.chain)
            };

            this.tsaClient = this.GetTsaClient(this.chain);

            Log.Application.Debug("Conseguiu pegar valor da autoridade de tempo? " + this.tsaClient != null);
        }

        private ICollection<Org.BouncyCastle.X509.X509Certificate> GetChain(X509Certificate2 certificado)
        {
            Log.Application.DebugFormat("Obtendo cadeia de certificacao");

            var x509Chain = new X509Chain();
            x509Chain.Build(certificado);
            
            var cadeia = (from X509ChainElement x509ChainElement in x509Chain.ChainElements
                select DotNetUtilities.FromX509Certificate(x509ChainElement.Certificate)).ToList();
            
            Log.Application.DebugFormat("Cadeia de certificação obtida com sucesso.");

            return cadeia;
        }

        private ITSAClient GetTsaClient(IEnumerable<Org.BouncyCastle.X509.X509Certificate> chain)
        {
            Log.Application.Debug("Obtendo TimeStamp Authority do certificado");
            
            ////https://freetsa.org/tsr
            
            return new CarimboBry(
                "https://bryurl",
                "usuario",
                "senha");
        }
    }
}