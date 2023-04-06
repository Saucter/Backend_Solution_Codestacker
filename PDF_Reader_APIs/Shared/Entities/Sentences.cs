using System.ComponentModel.DataAnnotations;
namespace PDF_Reader_APIs.Shared.Entities
{
    public class Sentences
    {
        [Key]
        public int id {get; set;}
        public int PDFid {get; set;}
        public string Sentence {get; set;}
        public PDF PDF {get; set;}
    }
}
