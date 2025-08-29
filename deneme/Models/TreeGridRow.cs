using System.Xml.Linq;

namespace validator.Models
{
    public class TreeGridRow
    {
        public string FieldName { get; set; } = "";
        public string Value { get; set; } = "";
        public int Level { get; set; } = 0;
        public bool IsHeader { get; set; } = false;
        public bool IsExpanded { get; set; } = true;
        public List<TreeGridRow> Children { get; set; } = new List<TreeGridRow>();
        public XElement Element { get; set; }
        public string Path { get; set; } = "";
        public List<string> EnumValues { get; set; } = new List<string>();
        public string DataType { get; set; } = "string"; // string, boolean, enum, number, date
        public bool IsBoolean => DataType == "boolean";
        public bool IsEnum => DataType == "enum";
        public bool IsNumber => DataType == "number";
        public bool IsDate => DataType == "date";

        // Validasyon durumu için 
        public bool IsValid { get; set; } = true;
        public string ValidationError { get; set; } = "";
        public string WarningMessage { get; set; } = ""; // Uyarı mesajı eklendi
        public bool IsMissing { get; set; } = false; // Eksik element kontrolü
    }
}

