namespace M2sys.IntegrationTests
{
    using System.Collections.Generic;
    using ClassifierPdf;
    using NUnit.Framework;

    [TestFixture]
    public class PdfServicoTest
    {
        [Test]
        public void DeveMontarPdfComImagens()
        {
            var imagens = new List<string> { "imagem1.jpg", "imagem2.jpg"};
            new PdfServico().MontarPdfComImagens(imagens, "pdfMontado.pdf");
        }
    }
}