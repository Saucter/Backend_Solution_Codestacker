namespace PDF_Reader_APIs.Shared.Entities
{
    //Class containing all the required properties for a PDF entity in the DB
    public class PDF
    {
        public PDF(){}
        public PDF(string Name, double FileSize, int NumberOfPages, List<Sentences> Sentences, string FileLink, string SentencesLinkTxt)
        {
            this.Name = Name;
            this.FileSize = FileSize;
            this.NumberOfPages = NumberOfPages;
            this.Sentences = Sentences;
            this.FileLink = FileLink;
            this.SentencesLinkTxt = SentencesLinkTxt;
            this.TimeOfUpload = DateTime.Now;
        }

        public int id {get; set;}
        public string Name {get; set;}
        public DateTime TimeOfUpload {get; set;}
        public double FileSize {get; set;}
        public int NumberOfPages {get; set;}
        public List<Sentences> Sentences {get; set;} //One-To-Many relationship
        public string SentencesLinkTxt {get; set;}
        public string FileLink {get; set;}
    }
}
