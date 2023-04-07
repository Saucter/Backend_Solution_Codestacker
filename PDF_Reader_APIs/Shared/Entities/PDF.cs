namespace PDF_Reader_APIs.Shared.Entities
{
    public class PDF
    {
        public PDF(){}
        public PDF(string Name, double FileSize, int NumberOfPages, List<Sentences> Sentences)
        {
            this.Name = Name;
            this.FileSize = FileSize;
            this.NumberOfPages = NumberOfPages;
            this.Sentences = Sentences;
            this.TimeOfUpload = DateTime.Now;
        }

        public int id {get; set;}
        public string Name {get; set;}
        public DateTime TimeOfUpload {get; set;}
        public double FileSize {get; set;}
        public int NumberOfPages {get; set;}
        public List<Sentences> Sentences {get; set;}
        public string FileLink {get; set;}
    }
}