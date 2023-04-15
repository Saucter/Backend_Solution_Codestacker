namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    public class GetKeyWordResponse
    {
        public GetKeyWordResponse(int PdfId, string PdfName, int Occurrences, List<string> Sentences, string Keyword, bool Exact, bool CaseSensitive)
        {
            this.PdfId = PdfId;
            this.PdfName = PdfName;
            this.Occurrences = Occurrences;
            this.Sentences = Sentences;
            this.Keyword = Keyword;
            this.Exact = Exact;
            this.CaseSensitive = CaseSensitive;
        }

        public int PdfId {get; set;}
        public string PdfName {get; set;}
        public string Keyword {get; set;}
        public int Occurrences   {get; set;}
        public int TotalOccurrences  {get; set;}
        public bool Exact {get; set;}
        public bool CaseSensitive {get; set;}
        public List<string> Sentences {get; set;} 
    }
}
