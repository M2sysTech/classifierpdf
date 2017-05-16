namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using LogHelpers;
    using System;
    using System.IO;

    public class AssinarPdf 
    {
        private readonly IPdfSigner pdfSigner;

        public AssinarPdf(IPdfSigner pdfSigner)
        {
            this.pdfSigner = pdfSigner;
        }

        public string Execute(string pdfMontado)
        {
            var singInfo = new SingInfo();

            var diretorio = Path.GetDirectoryName(pdfMontado);
            var fileName = Path.GetFileNameWithoutExtension(pdfMontado);

            singInfo.SourcePdf = pdfMontado;
            singInfo.TargetPdf = Path.Combine(diretorio, fileName + "-ass.pdf");

            singInfo.Author = "author da assinatura";
            singInfo.Creator = "quem criou";
            singInfo.Keywords = "palavras-chave";
            singInfo.Producer = "quem produziu";
            singInfo.SignatureContact = "contato";
            singInfo.SignatureLocation = "endereco";
            singInfo.SignatureReason = "motivo";
            singInfo.Subject = "assunto";
            singInfo.Title = "titulo";

            try
            {
                this.pdfSigner.Sign(singInfo);
                return singInfo.TargetPdf;
            }
            catch (Exception exception)
            {
                Log.Application.Error(exception);
                return singInfo.SourcePdf;
            }
        }
    }
}