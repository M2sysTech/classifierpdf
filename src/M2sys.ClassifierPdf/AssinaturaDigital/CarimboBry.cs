namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.util;
    using M2sys.ClassifierPdf.LogHelpers;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Tsp;
    using iTextSharp.text.error_messages;
    using iTextSharp.text.pdf.security;

    public class CarimboBry : ITSAClient 
    {
        protected ITSAInfoBouncyCastle tsaInfo;
        private string caminhoPfx;
        private string usuario;
        private string senha;
        private string url;
        private int tokenSizeEstimate;
        private string digestAlgorithm = "SHA-256";

        public CarimboBry(string url, string usuario, string senha)
        {
            this.usuario = usuario;
            this.senha = senha;
            this.url = url;
            this.tokenSizeEstimate = 4096;
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

            var tsqGenerator = new TimeStampRequestGenerator();
            tsqGenerator.SetCertReq(true);

            tsqGenerator.SetReqPolicy("2.16.76.1.6.6");
            var nonce = BigInteger.ValueOf(DateTime.Now.Ticks + Environment.TickCount);
            var request = tsqGenerator.Generate(DigestAlgorithms.GetAllowedDigests(this.digestAlgorithm), imprint, nonce);
            var requestBytes = request.GetEncoded();

            respBytes = this.GetTsaResponse(requestBytes);

            var response = new TimeStampResponse(respBytes);

            response.Validate(request);
            
            var failure = response.GetFailInfo();
            var value = (failure == null) ? 0 : failure.IntValue;

            if (value != 0)
            {
                //// @todo: Translate value of 15 error codes defined by PKIFailureInfo to string
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.tsa.1.response.code.2", this.url, value));
            }
            
            //// @todo: validate the time stap certificate chain (if we want assure we do not sign using an invalid timestamp).

            var timeStampToken = response.TimeStampToken;
            
            if (timeStampToken == null)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("tsa.1.failed.to.return.time.stamp.token.2", this.url, response.GetStatusString()));
            }

            var timeStampInfo = timeStampToken.TimeStampInfo;
            var encoded = timeStampToken.GetEncoded();

            Log.Application.Info("Timestamp generated: " + timeStampInfo.GenTime);

            if (this.tsaInfo != null)
            {
                this.tsaInfo.InspectTimeStampTokenInfo(timeStampInfo);
            }
            
            this.tokenSizeEstimate = encoded.Length + 32;

            return encoded;
        }

        protected internal virtual byte[] GetTsaResponse(byte[] requestBytes)
        {
            var con = (HttpWebRequest)WebRequest.Create(this.url);
            con.ContentLength = requestBytes.Length;
            con.ContentType = "application/timestamp-query";
            con.Method = "POST";

            if (string.IsNullOrEmpty(this.usuario) == false)
            {
                var authInfo = this.usuario + ":" + this.senha;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo), Base64FormattingOptions.None);
                con.Headers["Authorization"] = "Basic " + authInfo;
            }

            var outp = con.GetRequestStream();
            outp.Write(requestBytes, 0, requestBytes.Length);
            outp.Close();
            var response = (HttpWebResponse)con.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.http.response.1", (int) response.StatusCode));
            }

            var inp = response.GetResponseStream();

            var baos = new MemoryStream();
            var buffer = new byte[1024];
            var bytesRead = 0;
           
            while ((bytesRead = inp.Read(buffer, 0, buffer.Length)) > 0)
            {
                baos.Write(buffer, 0, bytesRead);
            }

            inp.Close();
            response.Close();
            byte[] respBytes = baos.ToArray();

            var encoding = response.ContentEncoding;
            
            if (encoding != null && Util.EqualsIgnoreCase(encoding, "base64"))
            {
                respBytes = Convert.FromBase64String(Encoding.ASCII.GetString(respBytes));
            }

            return respBytes;
        }    
    }
}