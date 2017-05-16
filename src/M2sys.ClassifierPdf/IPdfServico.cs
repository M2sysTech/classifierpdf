namespace M2sys.ClassifierPdf
{
    using Entidades;
    using System.Collections.Generic;

    public interface IPdfServico
    {
        string MontarPdfComImagens(List<string> listaDeImagens, string nomeArquivoPdf);

        string MontarPdfPesquisavel(List<Pagina> listaDePaginas, string nomeArquivoPdf, IList<PalavraReconhecida> palavrasReconhecidas);
    }
}
