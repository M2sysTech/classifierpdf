namespace M2sys.ClassifierPdf.Entidades
{
    public class PalavraReconhecida
    {
        public Pagina Pagina { get; set; }

        public string Texto { get; set; }

        public int Esquerda { get; set; }

        public int Direita { get; set; }

        public int Altura { get; set; }

        public int Topo { get; set; }
    }
}