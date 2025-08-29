namespace validator.Models
{
    public class XsdNodeModel
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Value { get; set; } = "";
        public List<XsdNodeModel> Children { get; set; } = new List<XsdNodeModel>();
    }
}

