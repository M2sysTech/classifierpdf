namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.util;
    using M2sys.ClassifierPdf.LogHelpers;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Tsp;
    using iTextSharp.text.error_messages;
    using iTextSharp.text.pdf.security;

    public class CarimboCertisign : ITSAClient 
    {
        protected ITSAInfoBouncyCastle tsaInfo;
        private string caminhoPfx;
        private string senha;
        private string url;
        private int tokenSizeEstimate;
        private string digestAlgorithm = "SHA-256";

        public CarimboCertisign(string url, string caminhoPfx, string senha)
        {
            this.caminhoPfx = caminhoPfx;
            this.senha = senha;
            this.url = url;
            this.tokenSizeEstimate = 0;
        }

        public int GetTokenSizeEstimate()
        {
            return this.tokenSizeEstimate;
        }

        public IDigest GetMessageDigest()
        {
            return DigestAlgorithms.GetMessageDigest(this.digestAlgorithm);
        }

        public byte[] GetTimeStampToken(byte[] imprint)
        {
            byte[] respBytes = null;

            //// Setup the time stamp request
            var tsqGenerator = new TimeStampRequestGenerator();
            tsqGenerator.SetCertReq(true);

            //// tsqGenerator.setReqPolicy("1.3.6.1.4.1.601.10.3.1");
            var nonce = BigInteger.ValueOf(DateTime.Now.Ticks + Environment.TickCount);
            var request = tsqGenerator.Generate(DigestAlgorithms.GetAllowedDigests(this.digestAlgorithm), imprint, nonce);
            var requestBytes = request.GetEncoded();

            //// Call the communications layer
            respBytes = this.GetTsaResponse(requestBytes);

            //// Handle the TSA response
            var response = new TimeStampResponse(respBytes);

            //// validate communication level attributes (RFC 3161 PKIStatus)
            response.Validate(request);
            
            var failure = response.GetFailInfo();
            var value = (failure == null) ? 0 : failure.IntValue;
            if (value != 0)
            {
                //// @todo: Translate value of 15 error codes defined by PKIFailureInfo to string
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.tsa.1.response.code.2", this.url, value));
            }
            
            //// @todo: validate the time stap certificate chain (if we want assure we do not sign using an invalid timestamp).

            //// extract just the time stamp token (removes communication status info)
            var timeStampToken = response.TimeStampToken;

            if (timeStampToken == null)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("tsa.1.failed.to.return.time.stamp.token.2", this.url, response.GetStatusString()));
            }

            var timeStampInfo = timeStampToken.TimeStampInfo; // to view details
            var encoded = timeStampToken.GetEncoded();

            Log.Application.Info("Timestamp generated: " + timeStampInfo.GenTime);

            if (this.tsaInfo != null)
            {
                this.tsaInfo.InspectTimeStampTokenInfo(timeStampInfo);
            }
            
            //// Update our token size estimate for the next call (padded to be safe)
            this.tokenSizeEstimate = encoded.Length + 32;

            return encoded;
        }

        public virtual byte[] GetTsaResponse(byte[] requestBytes)
        {
            var requestCarimboTempo = (HttpWebRequest)WebRequest.Create(this.url);
            requestCarimboTempo.ContentLength = requestBytes.Length;
            requestCarimboTempo.ContentType = "application/timestamp-query";
            requestCarimboTempo.Method = "POST";

            var cert = new X509Certificate2(this.caminhoPfx, "123456", X509KeyStorageFlags.Exportable);
            requestCarimboTempo.ClientCertificates.Add(cert);

            var streamRequisicao = requestCarimboTempo.GetRequestStream();
            streamRequisicao.Write(requestBytes, 0, requestBytes.Length);
            streamRequisicao.Close();

            var response = (HttpWebResponse)requestCarimboTempo.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new IOException("Invalid HTTP response: " + (int) response.StatusCode);
            }

            var streamResposta = response.GetResponseStream();

            var baos = new MemoryStream();
            var buffer = new byte[1024];
            var bytesRead = 0;
            
            while ((bytesRead = streamResposta.Read(buffer, 0, buffer.Length)) > 0)
            {
                baos.Write(buffer, 0, bytesRead);
            }

            streamResposta.Close();
            response.Close();

            var respBytes = baos.ToArray();
            var encoding = response.ContentEncoding;

            if (encoding != null && Util.EqualsIgnoreCase(encoding, "base64"))
            {
                respBytes = Convert.FromBase64String(Encoding.ASCII.GetString(respBytes));
            }

            return respBytes;
        }    
    }
}