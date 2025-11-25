using System.Globalization;
using System.Text.Json.Serialization;
using EsiaUserGenerator.Utils.JsonConverter;

namespace EsiaUserGenerator.Dto.Model;

    public class EsiaUserInfo
    {
        private const string DATE_FORMAT = "dd.MM.yyyy";
        public string? LastName { get; set; }
        
        public string? FirstName { get; set; }
        
        public string? Gender { get; set; }
        
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? BirthDate { get; set; }
        
        public string? BirthPlace { get; set; }
        
        public string? Citizenship { get; set; }
        
        public string? Snils { get; set; }
        public DocumentsInfo? Documents { get; set; }
        
        public class DocumentsInfo
        {
            public Element[] Elements { get; set; }
            public class Element
            {
                public string? Number { get; set; }
                [JsonConverter(typeof(CustomDateTimeConverter))]
                public DateTime? IssueDate { get; set; }
                public string? Series { get; set; }
                public string? Type { get; set; }
                public string? IssueId { get; set; }
                public string? IssuedBy { get; set; }
            }
        }
    }







    
