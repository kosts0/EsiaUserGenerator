using System.Globalization;
using EsiaUserGenerator.Utils.JsonConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EsiaUserGenerator.Dto.Model;

    public class EsiaUserInfo
    {
        private const string DATE_FORMAT = "dd.MM.yyyy";
        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("birthDate")]
        [JsonConverter(typeof(CustomDateTimeConverter), DATE_FORMAT)]
        public DateTime? BirthDate { get; set; }

        [JsonProperty("birthPlace")]
        public string? BirthPlace { get; set; }

        [JsonProperty("citizenship")]
        public string? Citizenship { get; set; }

        [JsonProperty("snils")]
        public string? Snils { get; set; }
        [JsonProperty("documents")]
        public DocumentsInfo? Documents { get; set; }
        
        public class DocumentsInfo
        {
            [JsonProperty("elements")]
            public Element[] Elements { get; set; }
            public class Element
            {
                [JsonProperty("number")]
                [JsonConverter(typeof(LongParseConverter))]
                public long? Number { get; set; }

                [JsonProperty("issueDate")]
                [JsonConverter(typeof(CustomDateTimeConverter), DATE_FORMAT)]
                public DateTime? IssueDate { get; set; }

                [JsonProperty("series")]
                [JsonConverter(typeof(LongParseConverter))]
                public long? Series { get; set; }

                [JsonProperty("type")]
                public string? Type { get; set; }

                [JsonProperty("issueId")]
                [JsonConverter(typeof(LongParseConverter))]
                public long? IssueId { get; set; }

                [JsonProperty("issuedBy")]
                public string? IssuedBy { get; set; }
            }
        }
    }







    
