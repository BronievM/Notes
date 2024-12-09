using Course.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace Course
{
    [Serializable]
    public class Note
    {
        public string Name { get; set; }
        public string RTF { get; set; }
        public string EvId { get; set; }
        public string URL { get; set; }
        public DateTime time { get; set; }
        public DateTime? timeOfNot { get; set; }
        public Account AccountN { get; set; }
        public List<string> Tags { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsEventCr { get; set; }
        public bool IsHidden { get; set; }
        public bool IsArchived { get; set; }

        public Note()
        {
            Name = "My note";
            RTF = "My text";
            EvId = String.Empty;
            URL = String.Empty;
            AccountN = new Account();
            Tags = new List<string>();
            IsPrivate = false;
            IsEventCr = false;
            IsHidden = false;
            IsArchived = false;
        }

        public void NoteClearEvent()
        {
            IsEventCr = false;
            timeOfNot = null;
            EvId = string.Empty;
            URL = string.Empty;
        }

        public void MakeNotePrivate() {
            IsPrivate = true; 
        }

        public void MakeNotePublic()
        {
            IsPrivate = false;
        }

        public void ArchiveNote()
        {
            IsArchived = true;
        }
        public string PlainText
        {
            get { return ConvertRtfToPlainText(RTF); }
        }

        private string ConvertRtfToPlainText(string rtfText)
        {
            if (string.IsNullOrEmpty(rtfText)) return String.Empty;
            string plainText;
            var document = new FlowDocument();
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            try
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(rtfText)))
                {
                    range.Load(memoryStream, DataFormats.Rtf);
                }
                using (var memoryStream = new MemoryStream())
                {
                    range = new TextRange(document.ContentStart, document.ContentEnd);
                    range.Save(memoryStream, DataFormats.Text);
                    plainText = Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while converting RTF to plain text: " + ex.Message);
                plainText = string.Empty;
            }
            return plainText;
        }
    }
}
