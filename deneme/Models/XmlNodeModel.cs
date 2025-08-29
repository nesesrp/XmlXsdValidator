using System.Xml.Linq;

namespace validator.Models
{
    public class XmlNodeModel
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public XElement Element { get; set; }
        public string Path { get; set; } = ""; // /Root[1]/Child[2]/Leaf[1]
        public List<XmlNodeModel> Children { get; set; } = new List<XmlNodeModel>();
    }
}

