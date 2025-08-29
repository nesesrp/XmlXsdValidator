using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;


namespace validator
{
    public class XsdSchemaAnalyzer
    {
        private readonly string _xsdPath;
        private XmlSchemaSet _schemaSet;
        private Dictionary<string, SchemaConstraints> _constraintsCache;

        public XsdSchemaAnalyzer(string xsdPath)
        {
            _xsdPath = xsdPath;
            _constraintsCache = new Dictionary<string, SchemaConstraints>();
            LoadSchema();
        }

        private void LoadSchema()
        {
            try
            {
                _schemaSet = new XmlSchemaSet();

                // XSD dosyasını önce XDocument olarak yükle ve targetNamespace'i çıkar
                var xsdDoc = XDocument.Load(_xsdPath);
                var targetNamespace = ExtractTargetNamespace(xsdDoc);

                if (!string.IsNullOrEmpty(targetNamespace))
                {
                    // targetNamespace varsa, onu kullanarak şemayı ekle
                    _schemaSet.Add(targetNamespace, _xsdPath);
                }
                else
                {
                    // targetNamespace yoksa, boş namespace ile ekle
                    _schemaSet.Add("", _xsdPath);
                }

                _schemaSet.Compile();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"XSD şeması yüklenemedi: {ex.Message}", ex);
            }
        }

        private string ExtractTargetNamespace(XDocument xsdDoc)
        {
            try
            {
                // Schema elementinde targetNamespace attribute'unu ara
                var schemaElement = xsdDoc.Root;
                if (schemaElement?.Name.LocalName == "schema")
                {
                    var targetNamespaceAttr = schemaElement.Attribute("targetNamespace");
                    if (targetNamespaceAttr != null)
                    {
                        return targetNamespaceAttr.Value;
                    }
                }

                // Eğer bulunamazsa, boş string döndür
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public SchemaConstraints GetConstraintsByPath(string xpath)
        {
            if (_constraintsCache.ContainsKey(xpath))
            {
                return _constraintsCache[xpath];
            }

            var constraints = new SchemaConstraints();

            try
            {
                // XPath'e göre XSD elementini bul
                var element = FindElementByXPath(xpath);
                if (element != null)
                {
                    constraints = ExtractConstraintsFromElement(element);
                }

                // Eğer enum değerleri bulunamadıysa, element adı ile de dene
                if (constraints.EnumerationValues.Count == 0)
                {
                    var elementName = ExtractElementNameFromXPath(xpath);
                    Console.WriteLine($"Enum değerleri bulunamadı, element adı ile deneniyor: {elementName}");

                    var elementByName = FindElementByName(elementName);
                    if (elementByName != null)
                    {
                        Console.WriteLine($"Element bulundu: {elementName}, enum değerleri çıkarılıyor...");
                        var constraintsByName = ExtractConstraintsFromElement(elementByName);
                        if (constraintsByName.EnumerationValues.Count > 0)
                        {
                            Console.WriteLine($"Enum değerleri bulundu: {constraintsByName.EnumerationValues.Count} adet");
                            constraints = constraintsByName;
                        }
                        else
                        {
                            Console.WriteLine($"Element bulundu ama enum değerleri yok: {elementName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Element bulunamadı: {elementName}");
                    }
                }
            }
            catch (Exception)
            {
                // Hata durumunda varsayılan constraints döndür
                constraints.DataType = "string";
            }

            _constraintsCache[xpath] = constraints;
            return constraints;
        }

        private XElement FindElementByXPath(string xpath)
        {
            try
            {
                // XPath'i basitleştir ve element adını çıkar
                var elementName = ExtractElementNameFromXPath(xpath);

                // XSD dosyasını yükle ve element'i bul
                var xsdDoc = XDocument.Load(_xsdPath);
                var element = xsdDoc.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == elementName);

                return element;
            }
            catch
            {
                return null;
            }
        }

        private XElement FindElementByName(string elementName)
        {
            try
            {
                // XSD dosyasını yükle ve element'i bul
                var xsdDoc = XDocument.Load(_xsdPath);

                // Önce xs:element olarak tanımlanan elementleri ara
                var element = xsdDoc.Descendants(XName.Get("element", "http://www.w3.org/2001/XMLSchema"))
                    .FirstOrDefault(e => e.Attribute("name")?.Value == elementName);

                if (element != null)
                {
                    return element;
                }

                // Eğer bulunamazsa, xs:simpleType olarak tanımlanan type'ları ara (enum değerleri için)
                var simpleType = xsdDoc.Descendants(XName.Get("simpleType", "http://www.w3.org/2001/XMLSchema"))
                    .FirstOrDefault(e => e.Attribute("name")?.Value == elementName);

                if (simpleType != null)
                {
                    return simpleType;
                }

                // Son olarak tüm elementlerde ara
                return xsdDoc.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == elementName);
            }
            catch
            {
                return null;
            }
        }

        private XElement FindTypeDefinition(string typeName)
        {
            try
            {
                // XSD dosyasını yükle ve type'ı bul
                var xsdDoc = XDocument.Load(_xsdPath);

                // Önce xs:simpleType olarak tanımlanan type'ları ara
                var simpleType = xsdDoc.Descendants(XName.Get("simpleType", "http://www.w3.org/2001/XMLSchema"))
                    .FirstOrDefault(e => e.Attribute("name")?.Value == typeName);

                if (simpleType != null)
                {
                    return simpleType;
                }

                // Eğer bulunamazsa, xs:complexType olarak tanımlanan type'ları ara
                var complexType = xsdDoc.Descendants(XName.Get("complexType", "http://www.w3.org/2001/XMLSchema"))
                    .FirstOrDefault(e => e.Attribute("name")?.Value == typeName);

                return complexType;
            }
            catch
            {
                return null;
            }
        }

        private string ExtractElementNameFromXPath(string xpath)
        {
            // XPath'den element adını çıkar (örn: /Root[1]/Child[2]/Leaf[1] -> Leaf)
            var parts = xpath.Split('/');
            var lastPart = parts.LastOrDefault();

            if (string.IsNullOrEmpty(lastPart))
                return string.Empty;

            // [1] gibi index kısımlarını kaldır
            var bracketIndex = lastPart.IndexOf('[');
            if (bracketIndex > 0)
            {
                return lastPart.Substring(0, bracketIndex);
            }

            return lastPart;
        }

        private SchemaConstraints ExtractConstraintsFromElement(XElement element)
        {
            var constraints = new SchemaConstraints();

            try
            {
                // Element'in type attribute'unu kontrol et
                var typeAttr = element.Attribute("type");
                if (typeAttr != null)
                {
                    var typeName = typeAttr.Value;
                    constraints.DataType = MapXsdTypeToDataType(typeName);

                    // Type tanımını bul ve detayları çıkar
                    var typeDefinition = FindTypeDefinition(typeName);
                    if (typeDefinition != null)
                    {
                        ExtractConstraintsFromTypeDefinition(typeDefinition, constraints);
                    }
                }

                // Element'in kendi içindeki kısıtlamaları da kontrol et
                ExtractConstraintsFromElementContent(element, constraints);

                // Eğer hala DataType belirlenememişse, varsayılan olarak string
                if (string.IsNullOrEmpty(constraints.DataType))
                {
                    constraints.DataType = "string";
                }

                Console.WriteLine($"Constraints extracted for {element.Name.LocalName}: DataType={constraints.DataType}, Pattern={constraints.Pattern}, MinLength={constraints.MinLength}, MaxLength={constraints.MaxLength}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExtractConstraintsFromElement hatası: {ex.Message}");
                constraints.DataType = "string";
            }

            return constraints;
        }

        private void ExtractConstraintsFromTypeDefinition(XElement typeDefinition, SchemaConstraints constraints)
        {
            try
            {
                // xs:restriction içindeki kısıtlamaları kontrol et
                var restriction = typeDefinition.Element(XName.Get("restriction", "http://www.w3.org/2001/XMLSchema"));
                if (restriction != null)
                {
                    // Base type'ı kontrol et
                    var baseAttr = restriction.Attribute("base");
                    if (baseAttr != null)
                    {
                        var baseType = baseAttr.Value;
                        if (string.IsNullOrEmpty(constraints.DataType))
                        {
                            constraints.DataType = MapXsdTypeToDataType(baseType);
                        }
                    }

                    // Pattern (regex) kısıtlaması
                    var patternElement = restriction.Element(XName.Get("pattern", "http://www.w3.org/2001/XMLSchema"));
                    if (patternElement != null)
                    {
                        var valueAttr = patternElement.Attribute("value");
                        if (valueAttr != null)
                        {
                            constraints.Pattern = valueAttr.Value;
                            Console.WriteLine($"Pattern bulundu: {constraints.Pattern}");
                        }
                    }

                    // Length kısıtlamaları
                    var minLengthElement = restriction.Element(XName.Get("minLength", "http://www.w3.org/2001/XMLSchema"));
                    if (minLengthElement != null)
                    {
                        var valueAttr = minLengthElement.Attribute("value");
                        if (valueAttr != null && int.TryParse(valueAttr.Value, out int minLength))
                        {
                            constraints.MinLength = minLength;
                        }
                    }

                    var maxLengthElement = restriction.Element(XName.Get("maxLength", "http://www.w3.org/2001/XMLSchema"));
                    if (maxLengthElement != null)
                    {
                        var valueAttr = maxLengthElement.Attribute("value");
                        if (valueAttr != null && int.TryParse(valueAttr.Value, out int maxLength))
                        {
                            constraints.MaxLength = maxLength;
                        }
                    }

                    // Enumeration değerleri
                    var enumerationElements = restriction.Elements(XName.Get("enumeration", "http://www.w3.org/2001/XMLSchema"));
                    foreach (var enumElement in enumerationElements)
                    {
                        var valueAttr = enumElement.Attribute("value");
                        if (valueAttr != null)
                        {
                            constraints.EnumerationValues.Add(valueAttr.Value);
                        }
                    }

                    // Sayısal kısıtlamalar
                    var minInclusiveElement = restriction.Element(XName.Get("minInclusive", "http://www.w3.org/2001/XMLSchema"));
                    if (minInclusiveElement != null)
                    {
                        var valueAttr = minInclusiveElement.Attribute("value");
                        if (valueAttr != null && decimal.TryParse(valueAttr.Value, out decimal minInclusive))
                        {
                            constraints.MinInclusive = minInclusive;
                        }
                    }

                    var maxInclusiveElement = restriction.Element(XName.Get("maxInclusive", "http://www.w3.org/2001/XMLSchema"));
                    if (maxInclusiveElement != null)
                    {
                        var valueAttr = maxInclusiveElement.Attribute("value");
                        if (valueAttr != null && decimal.TryParse(valueAttr.Value, out decimal maxInclusive))
                        {
                            constraints.MaxInclusive = maxInclusive;
                        }
                    }

                    var minExclusiveElement = restriction.Element(XName.Get("minExclusive", "http://www.w3.org/2001/XMLSchema"));
                    if (minExclusiveElement != null)
                    {
                        var valueAttr = minExclusiveElement.Attribute("value");
                        if (valueAttr != null && decimal.TryParse(valueAttr.Value, out decimal minExclusive))
                        {
                            constraints.MinExclusive = minExclusive;
                        }
                    }

                    var maxExclusiveElement = restriction.Element(XName.Get("maxExclusive", "http://www.w3.org/2001/XMLSchema"));
                    if (maxExclusiveElement != null)
                    {
                        var valueAttr = maxExclusiveElement.Attribute("value");
                        if (valueAttr != null && decimal.TryParse(valueAttr.Value, out decimal maxExclusive))
                        {
                            constraints.MaxExclusive = maxExclusive;
                        }
                    }

                    // TotalDigits ve FractionDigits (sayısal hassasiyet)
                    var totalDigitsElement = restriction.Element(XName.Get("totalDigits", "http://www.w3.org/2001/XMLSchema"));
                    if (totalDigitsElement != null)
                    {
                        var valueAttr = totalDigitsElement.Attribute("value");
                        if (valueAttr != null && int.TryParse(valueAttr.Value, out int totalDigits))
                        {
                            constraints.TotalDigits = totalDigits;
                        }
                    }

                    var fractionDigitsElement = restriction.Element(XName.Get("fractionDigits", "http://www.w3.org/2001/XMLSchema"));
                    if (fractionDigitsElement != null)
                    {
                        var valueAttr = fractionDigitsElement.Attribute("value");
                        if (valueAttr != null && int.TryParse(valueAttr.Value, out int fractionDigits))
                        {
                            constraints.FractionDigits = fractionDigits;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExtractConstraintsFromTypeDefinition hatası: {ex.Message}");
            }
        }

        private void ExtractConstraintsFromElementContent(XElement element, SchemaConstraints constraints)
        {
            try
            {
                // Element'in kendi içindeki simpleType tanımını kontrol et
                var simpleType = element.Element(XName.Get("simpleType", "http://www.w3.org/2001/XMLSchema"));
                if (simpleType != null)
                {
                    ExtractConstraintsFromTypeDefinition(simpleType, constraints);
                }

                // Element'in kendi içindeki kısıtlamaları da kontrol et
                var restriction = element.Element(XName.Get("restriction", "http://www.w3.org/2001/XMLSchema"));
                if (restriction != null)
                {
                    ExtractConstraintsFromTypeDefinition(restriction, constraints);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExtractConstraintsFromElementContent hatası: {ex.Message}");
            }
        }

        private string MapXsdTypeToDataType(string xsdType)
        {
            if (string.IsNullOrEmpty(xsdType)) return "string";

            // XSD type'larını uygulama veri tiplerine eşle
            switch (xsdType.ToLower())
            {
                case "xs:string":
                case "string":
                    return "string";

                case "xs:int":
                case "xs:integer":
                case "xs:long":
                case "xs:short":
                case "xs:byte":
                case "int":
                case "integer":
                case "long":
                case "short":
                case "byte":
                    return "number";

                case "xs:decimal":
                case "xs:float":
                case "xs:double":
                case "decimal":
                case "float":
                case "double":
                    return "number";

                case "xs:boolean":
                case "boolean":
                    return "boolean";

                case "xs:date":
                case "xs:dateTime":
                case "xs:time":
                case "date":
                case "datetime":
                case "time":
                    return "date";

                default:
                    return "string";
            }
        }

        internal IEnumerable<object> GetAllConstraints()
        {
            throw new NotImplementedException();
        }

        public class SchemaConstraints
        {
            public int? MaxLength { get; set; }
            public int? MinLength { get; set; }
            public string Pattern { get; set; } = ""; // Regex pattern for validation
            public string DataType { get; set; } = "string";

            public int? TotalDigits { get; set; }
            public int? FractionDigits { get; set; }
            public decimal? MinInclusive { get; set; }
            public decimal? MaxInclusive { get; set; }
            public decimal? MinExclusive { get; set; }
            public decimal? MaxExclusive { get; set; }

            public List<string> EnumerationValues { get; set; } = new();
            public object Enumeration { get; internal set; }

            public bool IsValidInput(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    // Boş değer kontrolü - gerekirse burada validation eklenebilir
                    return true;
                }

                // Veri tipi kontrolü
                if (!IsValidDataType(input))
                    return false;

                // Uzunluk kontrolü
                if (MinLength.HasValue && input.Length < MinLength.Value)
                    return false;

                if (MaxLength.HasValue && input.Length > MaxLength.Value)
                    return false;

                // Sayısal değer kontrolü
                if (IsNumericType() && !IsValidNumericValue(input))
                    return false;



                // Enumeration kontrolü
                if (EnumerationValues.Count > 0 && !EnumerationValues.Contains(input))
                    return false;

                // Pattern kontrolü
                if (!string.IsNullOrEmpty(Pattern))
                {
                    try
                    {
                        var regex = new System.Text.RegularExpressions.Regex(Pattern);
                        if (!regex.IsMatch(input))
                            return false;
                    }
                    catch
                    {
                        // Regex hatası durumunda geçersiz kabul et
                        return false;
                    }
                }

                return true;
            }

            private bool IsValidDataType(string input)
            {
                if (string.IsNullOrEmpty(DataType))
                    return true;

                switch (DataType.ToLower())
                {
                    case "string":
                        return true;

                    case "integer":
                    case "int":
                    case "long":
                    case "short":
                    case "byte":
                    case "positiveinteger":
                    case "negativeinteger":
                    case "nonpositiveinteger":
                    case "nonnegativeinteger":
                        return int.TryParse(input, out _);

                    case "decimal":
                    case "float":
                    case "double":
                        return decimal.TryParse(input, out _);

                    case "boolean":
                        return bool.TryParse(input, out _) || input == "0" || input == "1";

                    case "date":
                        return DateTime.TryParse(input, out _);

                    case "datetime":
                        return DateTime.TryParse(input, out _);

                    case "time":
                        return TimeSpan.TryParse(input, out _);

                    default:
                        return true;
                }
            }

            private bool IsNumericType()
            {
                var numericTypes = new[] { "integer", "int", "long", "short", "byte", "decimal", "float", "double" };
                return !string.IsNullOrEmpty(DataType) && numericTypes.Contains(DataType.ToLower());
            }

            private bool IsValidNumericValue(string input)
            {
                if (!decimal.TryParse(input, out decimal value))
                    return false;

                // Min/Max değer kontrolleri
                if (MinInclusive.HasValue && value < MinInclusive.Value)
                    return false;

                if (MaxInclusive.HasValue && value > MaxInclusive.Value)
                    return false;

                if (MinExclusive.HasValue && value <= MinExclusive.Value)
                    return false;

                if (MaxExclusive.HasValue && value >= MaxExclusive.Value)
                    return false;

                // Basamak kontrolleri
                if (TotalDigits.HasValue)
                {
                    var totalDigits = input.Replace(".", "").Replace("-", "").Length;
                    if (totalDigits > TotalDigits.Value)
                        return false;
                }

                if (FractionDigits.HasValue)
                {
                    var decimalIndex = input.IndexOf('.');
                    if (decimalIndex >= 0)
                    {
                        var fractionDigits = input.Length - decimalIndex - 1;
                        if (fractionDigits > FractionDigits.Value)
                            return false;
                    }
                }

                return true;
            }




        }
    }
}
