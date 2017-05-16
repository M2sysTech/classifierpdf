namespace M2sys.ClassifierPdf
{
    using AssinaturaDigital;

    public interface IPdfSigner
    {
        void Sign(SingInfo singInfo);
    }
}