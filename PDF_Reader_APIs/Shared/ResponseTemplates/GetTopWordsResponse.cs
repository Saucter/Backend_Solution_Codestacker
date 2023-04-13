namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    public class GetTopWordsResponse
    {
        public GetTopWordsResponse(string Word, int Occurrances, int Position, bool Success)
        {
            this.Word = Word;
            this.Occurrances = Occurrances;
            this.Position = Position;
            this.Success = Success;
        }

        public string Word {get; set;}
        public int Occurrances {get; set;}
        public int Position {get; set;}
        public bool Success {get; set;}
    }
}
