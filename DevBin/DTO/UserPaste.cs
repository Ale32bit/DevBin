using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DevBin.Models
{
    

    public class UserPaste
    {
        public enum Exposures
        {
            Public = 1,
            Unlisted = 2,
            Private = 3,
            Encrypted = 4,
        }

        public string Title { get; set; }
        public string Syntax { get; set; }
        [EnumDataType(typeof(Exposures))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Exposures Exposure { get; set; }
        public string Content { get; set; }
        public bool AsGuest { get; set; } = false;
    }
}