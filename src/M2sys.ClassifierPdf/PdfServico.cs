namespace M2sys.ClassifierPdf
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using AForge.Imaging.Filters;
    using AForge.Imaging.Formats;
    using Entidades;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using LogHelpers;

    public class PdfServico
    {
        private const float MasterWidth = 589;
        private const float MasterHeight = 833;

        public string MontarPdfComImagens(List<string> listaDeImagens, string nomeArquivoPdf)
        {
            Log.Application.DebugFormat("Iniciando montagem de PDF com {0} paginas.", listaDeImagens.Count);

            var document = new Document(PageSize.A4, 0, 0, 0, 0);
            var imgAtual = string.Empty;
            try
            {
                PdfWriter.GetInstance(document, new FileStream(nomeArquivoPdf, FileMode.Create));
                document.Open();
                foreach (var image in listaDeImagens)
                {
                    imgAtual = image;
                    iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(image);

                    if (pic.Height > pic.Width)
                    {
                        float percentage = 0.0f;
                        percentage = PageSize.A4.Height / pic.Height;
                        pic.ScalePercent(percentage * 100);
                    }
                    else
                    {
                        float percentage = 0.0f;
                        percentage = PageSize.A4.Width / pic.Width;
                        pic.ScalePercent(percentage * 100);
                    }

                    document.Add(pic);
                    document.NewPage();
                }
            }
            catch (DocumentException exception)
            {
                Log.Application.Error(string.Format("Erro ao executar ItextSharp (Document Exception), na imagem {0}: ", imgAtual), exception);
                nomeArquivoPdf = string.Empty;
            }
            catch (IOException ioe)
            {
                Log.Application.Error(string.Format("Erro ao executar ItextSharp (IO), na imagem {0}: ", imgAtual), ioe);
                nomeArquivoPdf = string.Empty;
            }

            document.Close();

            return nomeArquivoPdf;
        }

        public string MontarPdfPesquisavel(List<Pagina> listaDePaginas, string nomeArquivoPdf, IList<PalavraReconhecida> palavrasReconhecidas)
        {
            Log.Application.DebugFormat("Iniciando montagem de PDF PESQUISÁVEL tipo PDF/A - 2com {0} paginas.", listaDePaginas.Count);
            //// resources para PDF/A
            string caminhoFonte = @"C:\Windows\Fonts\cour.ttf";
            var iccfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "sRGB_CS_profile.icm");
            if (!File.Exists(iccfile))
            {
                throw new Exception(string.Format("Um dos Resources não foi localizado. Impossível gerar PDF/A. Arquivo não encontrado ICC file :{0}", iccfile));
            }

            var fontPadrao = FontFactory.GetFont(caminhoFonte, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 8);
            var primeiraImagem = listaDePaginas[0].CaminhoCompletoDoArquivo;
            var document = new Document(this.DefinirOrientacao(primeiraImagem), 0, 0, 0, 0);
            ////var writer = PdfWriter.GetInstance(document, new FileStream(nomeArquivoPdf, FileMode.Create));
            var writer = PdfAWriter.GetInstance(document, new FileStream(nomeArquivoPdf, FileMode.Create), PdfAConformanceLevel.PDF_A_2A);

            writer.CreateXmpMetadata();
            writer.SetTagged();
            document.Open();
            document.AddLanguage("pt-br");
            ICC_Profile icc = ICC_Profile.GetInstance(iccfile);
            writer.SetOutputIntents("Custom", string.Empty, "http://www.color.org", "sRGB IEC61966-2.1", icc);

            var contentByte = writer.DirectContent;
            var contador = 0;
            var contadorPaginasInseridas = 1;

            foreach (var pagina in listaDePaginas)
            {
                try
                {
                    contador++;
                    
                    //// insere os textos
                    var palavrasAtuais = palavrasReconhecidas.Where(x => x.Pagina.Id == pagina.Id);
                    if (palavrasAtuais.Any())
                    {
                        //// se poucas palavras, checar se é uma pagina em branco
                        bool excluirPaginasBrancas = true;
                        decimal minimoPalavrasPaginaBranca = 2;

                        if (palavrasAtuais.Count() <= minimoPalavrasPaginaBranca && excluirPaginasBrancas)
                        {
                            Log.Application.DebugFormat("Possivel Página em branco encontrada (poucas palavras) {0}: ", pagina.CaminhoCompletoDoArquivo);
                            if (this.ChecarPontosBrancos(pagina.CaminhoCompletoDoArquivo, 30, 150))
                            {
                                Log.Application.DebugFormat("Analise de pixels detectou página em branco. Pagina {0} será desconsiderada no PDF: {1} ", contador, pagina.CaminhoCompletoDoArquivo);
                                continue;
                            }
                        }

                        //// transparencia
                        PdfGState graphicState = new PdfGState();
                        graphicState.FillOpacity = 0.01f;
                        contentByte.SetGState(graphicState);
                        contentByte.BeginText();
                        ColumnText ct = new ColumnText(contentByte);

                        foreach (var palavraReconhecida in palavrasAtuais)
                        {
                            var texto = new Phrase(new Chunk(palavraReconhecida.Texto, fontPadrao));
                            var pontoEsquerdo = this.ConversorGenerico(palavraReconhecida.Esquerda, pagina.WidthPixels, document.PageSize.Width);
                            var pontoDireito = this.ConversorGenerico(palavraReconhecida.Direita, pagina.WidthPixels, document.PageSize.Width);
                            var pontoBottom = document.PageSize.Height - this.ConversorGenerico(palavraReconhecida.Topo + palavraReconhecida.Altura, pagina.HeightPixels, document.PageSize.Height);
                            var pontoTop = document.PageSize.Height - this.ConversorGenerico(palavraReconhecida.Topo, pagina.HeightPixels, document.PageSize.Height);
                            ////var pontoEsquerdo = this.ConversorWidth(palavraReconhecida.Esquerda, pagina.WidthPixels);
                            ////var pontoDireito = this.ConversorWidth(palavraReconhecida.Direita, pagina.WidthPixels);
                            ////var pontoBottom = MasterHeight - this.ConversorHeight(palavraReconhecida.Topo + palavraReconhecida.Altura, pagina.HeightPixels);
                            ////var pontoTop = MasterHeight - this.ConversorHeight(palavraReconhecida.Topo, pagina.HeightPixels);
                            ct.SetSimpleColumn(texto, pontoEsquerdo, pontoBottom, pontoDireito + 100, pontoTop + 10, 10, Element.ALIGN_LEFT);
                            ct.Go();
                        }

                        contentByte.EndText();
                    }
                    else
                    {
                        var excluirPaginasBrancas = true;

                        if (excluirPaginasBrancas)
                        {
                            Log.Application.DebugFormat("Possivel Página em branco encontrada (nenhuma palavra reconhecida) {0}: ", pagina.CaminhoCompletoDoArquivo);
                            if (this.ChecarPontosBrancos(pagina.CaminhoCompletoDoArquivo, 35, 150))
                            {
                                Log.Application.DebugFormat("Analise de pixels detectou página em branco. Pagina {0} será desconsiderada no PDF: {1} ", contador, pagina.CaminhoCompletoDoArquivo);
                                continue;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Application.Error(string.Format("Erro ao executar inserir texto no PDF, na imagem {0}: ", pagina.CaminhoCompletoDoArquivo), exception);
                    nomeArquivoPdf = string.Empty;
                }

                //// insere a imagem 
                try
                {
                    iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(pagina.CaminhoCompletoDoArquivo);

                    if (pic.Height > pic.Width)
                    {
                        float percentage = 0.0f;
                        percentage = document.PageSize.Height / pic.Height;
                        pic.ScalePercent(percentage * 100);
                    }
                    else
                    {
                        float percentage = 0.0f;
                        percentage = document.PageSize.Width / pic.Width;
                        pic.ScalePercent(percentage * 100);
                    }

                    pic.SetAccessibleAttribute(PdfName.ALT, new PdfString(string.Format("Pagina-{0}", contadorPaginasInseridas)));
                    document.Add(pic);
                    contadorPaginasInseridas++;
                }
                catch (DocumentException exception)
                {
                    Log.Application.Error(string.Format("Erro ao executar ItextSharp (Document Exception), na imagem {0}: ", pagina.CaminhoCompletoDoArquivo), exception);
                    nomeArquivoPdf = string.Empty;
                }
                catch (IOException ioe)
                {
                    Log.Application.Error(string.Format("Erro ao executar ItextSharp (IO), na imagem {0}: ", pagina.CaminhoCompletoDoArquivo), ioe);
                    nomeArquivoPdf = string.Empty;
                }

                ////verifica se precisa rotacionar para paisagem na proxima pagina 
                if (contador < listaDePaginas.Count)
                {
                    var proximaImagem = listaDePaginas[contador];
                    document.SetPageSize(this.DefinirOrientacao(proximaImagem.CaminhoCompletoDoArquivo));
                }

                document.NewPage();
            }

            document.Close();
            return nomeArquivoPdf;
        }

        private iTextSharp.text.Rectangle DefinirOrientacao(string caminho)
        {
            try
            {
                ImageInfo imageInfo;

                var bitmap = ImageDecoder.DecodeFromFile(caminho, out imageInfo);
                if (bitmap.Width > bitmap.Height)
                {
                    bitmap.Dispose();
                    return PageSize.A4.Rotate();
                }
                else
                {
                    bitmap.Dispose();
                    return PageSize.A4;
                }
            }
            catch (Exception exception)
            {
                Log.Application.ErrorFormat("Erro ao tentar definir orientação: ", exception);
            }

            return PageSize.A4;
        }

        private bool ChecarPontosBrancos(string caminhoCompletoDoArquivo, int pixelsWidt, int margem)
        {
            ImageInfo imageInfo;
            var bitmap = ImageDecoder.DecodeFromFile(caminhoCompletoDoArquivo, out imageInfo);
            var converteu = false;
            if (imageInfo.BitsPerPixel < 8)
            {
                var clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                using (Graphics gr = Graphics.FromImage(clone))
                {
                    gr.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, clone.Width, clone.Height));
                    gr.Save();
                }

                bitmap.Dispose();
                bitmap = clone;
                converteu = true;
            }

            if (converteu || imageInfo.BitsPerPixel > 8)
            {
                var grayImage = Grayscale.CommonAlgorithms.Y.Apply(bitmap);
                bitmap.Dispose();
                bitmap = grayImage;
            }

            ////var manipulador = new ManipulaImagemServico();
            ////var resultado = manipulador.AvaliarPaginaEmbranco(bitmap, pixelsWidt, margem);

            bitmap.Dispose();

            return true; ////resultado;
        }

        private float ConversorHeight(int pontoY, float heightReferencia)
        {
            return (pontoY * MasterHeight) / heightReferencia;
        }

        private float ConversorWidth(int pontoX, float widthReferencia)
        {
            return (pontoX * MasterWidth) / widthReferencia;
        }

        private float ConversorGenerico(int ponto, float referencia, float master)
        {
            return (ponto * master) / referencia;
        }
    }
}
