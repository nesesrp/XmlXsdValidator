using System.Xml.Linq;
using validator.Models;

namespace validator.Operations
{
    public static class XmlOperations
    {
        public static XmlNodeModel ParseElement(XElement element)
        {
            var node = new XmlNodeModel
            {
                Name = element.Name.LocalName,
                Value = element.HasElements ? null : element.Value,
                Element = element,
                Path = BuildXPathWithIndex(element)
            };

            foreach (var child in element.Elements())
                node.Children.Add(ParseElement(child));

            return node;
        }

        public static XsdNodeModel ParseXsdElement(XElement element)
        {
            var node = new XsdNodeModel
            {
                Name = element.Name.LocalName,
                Value = element.HasElements ? null : element.Value
            };

            // Enum tanımlarını kontrol et
            if (element.Name.LocalName == "simpleType")
            {
                var nameAttr = element.Attribute("name");
                if (nameAttr != null)
                {
                    node.Name = nameAttr.Value;
                }
            }

            foreach (var child in element.Elements())
            {
                node.Children.Add(ParseXsdElement(child));
            }

            return node;
        }

        public static void DisplayXmlInRichTextBox(XDocument xmlDoc, RichTextBox richTextBox)
        {
            try
            {
                Console.WriteLine($" DisplayXmlInRichTextBox başladı");
                Console.WriteLine($" XML Doc null mu? {xmlDoc == null}");
                Console.WriteLine($" XML Root: {xmlDoc?.Root?.Name.LocalName}");

                richTextBox.Clear();
                Console.WriteLine($" RichTextBox temizlendi");

                string formattedXml = xmlDoc.ToString();
                Console.WriteLine($" XML string'e çevrildi, uzunluk: {formattedXml.Length}");

                // XML'i daha okunabilir hale getir 
                formattedXml = formattedXml.Replace("><", ">\n<");
                Console.WriteLine($" XML formatlandı, yeni uzunluk: {formattedXml.Length}");

                
                var preview = formattedXml.Substring(0, Math.Min(200, formattedXml.Length));
                Console.WriteLine($" XML önizleme: {preview}...");

                // RichTextBox'ı temizle ve yeni XML'i ekle
                richTextBox.Clear();
                richTextBox.AppendText(formattedXml);
                Console.WriteLine($" RichTextBox'a XML yazıldı");
                Console.WriteLine($" RichTextBox Text uzunluğu: {richTextBox.Text.Length}");

                // Revision elementini özel olarak kontrol et
                var revisionElement = xmlDoc.Descendants("Revision").FirstOrDefault();
                if (revisionElement != null)
                {
                    Console.WriteLine($" Revision element değeri: '{revisionElement.Value}'");
                }
                else
                {
                    Console.WriteLine($" Revision element bulunamadı!");
                }

                // Font ve stil ayarları
                richTextBox.SelectAll();
                richTextBox.SelectionFont = new Font("Consolas", 9);
                richTextBox.SelectionColor = Color.Black;
                richTextBox.DeselectAll();

                // Scroll'u en üste al
                richTextBox.SelectionStart = 0;
                richTextBox.ScrollToCaret();

                // RichTextBox'ı  yenile
                richTextBox.Invalidate();
                richTextBox.Update();

                Console.WriteLine(" XML sol tarafta RichTextBox'ta gösterildi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" DisplayXmlInRichTextBox hatası: {ex.Message}");
                Console.WriteLine($" Stack trace: {ex.StackTrace}");
                richTextBox.Text = $"XML görüntüleme hatası: {ex.Message}";
            }
        }

        public static void UpdateXmlElementByName(XDocument xmlDoc, string elementName, string newValue)
        {
            try
            {
                if (xmlDoc == null) return;

                Console.WriteLine($"UpdateXmlElementByName: {elementName} = '{newValue}'");

                // XML'de bu isimdeki tüm elementleri bul
                var elements = xmlDoc.Descendants(elementName).ToList();
                Console.WriteLine($"  '{elementName}' isimli {elements.Count} element bulundu");

                int foundCount = 0;
                foreach (var element in elements)
                {
                    if (!element.HasElements) // Sadece Leaf elementler
                    {
                        Console.WriteLine($"  Element bulundu: {element.Name.LocalName}, Path: {BuildXPathWithIndex(element)}, Eski değer: '{element.Value}', Yeni değer: '{newValue}'");
                        element.Value = newValue ?? "";
                        foundCount++;
                    }
                }

                Console.WriteLine($"  Toplam {foundCount} element güncellendi");

            
                if (foundCount == 0)
                {
                    Console.WriteLine($"  Hiç element güncellenmedi. XML içeriği kontrol ediliyor...");
                    var allElements = xmlDoc.Descendants().Select(e => e.Name.LocalName).Distinct().ToList();
                    Console.WriteLine($"  XML'de bulunan tüm element isimleri: {string.Join(", ", allElements.Take(20))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateXmlElementByName hatası: {ex.Message}");
                throw new Exception($"Element '{elementName}' güncellenirken hata: {ex.Message}");
            }
        }

        // Daha spesifik güncelleme için Path kullanarak elementi bul ve güncelle
        public static void UpdateXmlElementByPath(XDocument xmlDoc, string elementPath, string newValue)
        {
            try
            {
                if (xmlDoc == null) return;

                Console.WriteLine($"UpdateXmlElementByPath: {elementPath} = '{newValue}'");

                // Path'e göre elementi bul
                var pathParts = elementPath.Split('/');
                if (pathParts.Length == 0) return;

                var currentElement = xmlDoc.Root;
                foreach (var part in pathParts)
                {
                    if (currentElement == null) break;

                 
                    var cleanPart = part;
                    var bracketIndex = part.IndexOf('[');
                    if (bracketIndex > 0)
                    {
                        cleanPart = part.Substring(0, bracketIndex);
                    }

                    // Element'i bul
                    var foundElement = currentElement.Element(cleanPart);
                    if (foundElement == null)
                    {
                        // Eğer bulunamazsa, tüm child elementlerde ara
                        foundElement = currentElement.Elements(cleanPart).FirstOrDefault();
                    }

                    currentElement = foundElement;
                }

                if (currentElement != null && !currentElement.HasElements)
                {
                    Console.WriteLine($"  Path ile element bulundu: {currentElement.Name.LocalName}, Eski değer: '{currentElement.Value}', Yeni değer: '{newValue}'");
                    currentElement.Value = newValue ?? "";
                    Console.WriteLine($"  Element güncellendi: {currentElement.Name.LocalName} = '{currentElement.Value}'");
                }
                else
                {
                    Console.WriteLine($"  Path ile element bulunamadı: {elementPath}");
                   
                    Console.WriteLine("  Fallback: UpdateXmlElementByName deneniyor...");
                    UpdateXmlElementByName(xmlDoc, ExtractElementNameFromPath(elementPath), newValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateXmlElementByPath hatası: {ex.Message}");
           
                try
                {
                    Console.WriteLine("  Fallback: UpdateXmlElementByName deneniyor...");
                    UpdateXmlElementByName(xmlDoc, ExtractElementNameFromPath(elementPath), newValue);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"  Fallback da başarısız: {fallbackEx.Message}");
                }
            }
        }

        // Path'den element adını çıkar
        public static string ExtractElementNameFromPath(string elementPath)
        {
            if (string.IsNullOrEmpty(elementPath)) return "";

            var parts = elementPath.Split('/');
            var lastPart = parts.LastOrDefault();
            if (string.IsNullOrEmpty(lastPart)) return "";

            
            var bracketIndex = lastPart.IndexOf('[');
            if (bracketIndex > 0)
            {
                return lastPart.Substring(0, bracketIndex);
            }

            return lastPart;
        }

        public static string SanitizeName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "_";
            var invalid = new[] { '\\', '/', '[', ']', ' ', ':', '.' };
            var result = new string(input.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return result;
        }

        // 
        public static string BuildXPathWithIndex(XElement element)
        {
            var parts = new Stack<string>();
            XElement current = element;
            while (current != null)
            {
                int index = 1;
                if (current.Parent != null)
                {
                    index = current.ElementsBeforeSelf(current.Name).Count() + 1;
                }
                parts.Push($"{current.Name.LocalName}[{index}]");
                current = current.Parent;
            }
            return "/" + string.Join("/", parts);
        }

        public static string BuildXPathForElement(XElement element)
        {
            var parts = new Stack<string>();
            XElement current = element;
            while (current != null)
            {
                int index = 1;
                if (current.Parent != null)
                {
                    index = current.ElementsBeforeSelf(current.Name).Count() + 1;
                }
                parts.Push($"{current.Name.LocalName}[{index}]");
                current = current.Parent;
            }
            return "/" + string.Join("/", parts);
        }
    }
}

