namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    /*Class containing all the required responses in the HTTP GET response of the GetPDF API
    This was needed in order to resolve a caching bug which caused issues when trying to use GetKeyword or GetTopWords after using GetPDF
    This response does not contain a list of sentences as the sentences can be unnecessary in the response based on user requirements 
    */
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
