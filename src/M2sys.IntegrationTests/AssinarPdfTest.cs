namespace M2sys.IntegrationTests
{
    using ClassifierPdf.AssinaturaDigital;
    using NUnit.Framework;

    [TestFixture]
    public class AssinarPdfTest
    {
        public void DeveAssinarUmPdf()
        {
            var assinadoPdf = new AssinarPdf(new TimeStampSigner(new CertificadoA3())).Execute("meu.pdf");
        }
    }
}
