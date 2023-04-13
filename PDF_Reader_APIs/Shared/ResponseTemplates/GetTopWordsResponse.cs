namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    public class GetTopWordsResponse
    {
        public GetTopWordsResponse(string Word, int Occurrances, int Position)
        {
            this.Word = Word;
            this.Occurrances = Occurrances;
            this.Position = Position;
        }

        public string Word {get; set;}
        public int Occurrances {get; set;}
        public int Position {get; set;}
    }
}
