namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    public class GetPdfResponse
    {
        public GetPdfResponse(int id, string Name, double FileSize, int NumberOfPages, string FileLink, string SentencesLinkTxt)
        {
            this.id = id;
            this.Name = Name;
            this.FileSize = FileSize;
            this.NumberOfPages = NumberOfPages;
            this.FileLink = FileLink;
            this.SentencesLinkTxt = SentencesLinkTxt;
            this.TimeOfUpload = DateTime.Now;
        }

        public int id {get; set;}
        public string Name {get; set;}
        public DateTime TimeOfUpload {get; set;}
        public double FileSize {get; set;}
        public int NumberOfPages {get; set;}
        public string SentencesLinkTxt {get; set;}
        public string FileLink {get; set;}
    }
}
