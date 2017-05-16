namespace M2sys.ClassifierPdf.AssinaturaDigital
{
    using System.Collections.Generic;
    using iTextSharp.text.xml.xmp;

    public class MetaData
    {
        public MetaData()
        {
            this.Info = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Info
        {
            get;
            set;
        }

        public string Author
        {
            get { return (string) this.Info["Author"]; }
            set { this.Info.Add("Author", value); }
        }

        public string Title
        {
            get { return (string) this.Info["Title"]; }
            set { this.Info.Add("Title", value); }
        }

        public string Subject
        {
            get { return (string) this.Info["Subject"]; }
            set { this.Info.Add("Subject", value); }
        }

        public string Keywords
        {
            get { return (string) this.Info["Keywords"]; }
            set { this.Info.Add("Keywords", value); }
        }

        public string Producer
        {
            get { return (string) this.Info["Producer"]; }
            set { this.Info.Add("Producer", value); }
        }

        public string Creator
        {
            get { return (string) this.Info["Creator"]; }
            set { this.Info.Add("Creator", value); }
        }

        public IDictionary<string, string> GetMetaData()
        {
            return this.Info;
        }

        public byte[] GetStreamedMetaData()
        {
            var os = new System.IO.MemoryStream();
            var xmp = new XmpWriter(os, this.Info);

            xmp.Close();
            return os.ToArray();
        } 
    }
}