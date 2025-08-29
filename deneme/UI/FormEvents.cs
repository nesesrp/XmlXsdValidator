using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;
using validator.Models;
using validator.Operations;
using validator.Validation;

namespace validator.UI
{
    public static class FormEvents
    {
        public static void btnBrowseXml_Click(object sender, EventArgs e, TextBox txtXmlPath)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML dosyaları (*.xml)|*.xml|Tüm dosyalar (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtXmlPath.Text = openFileDialog.FileName;
                }
            }
        }

        public static void btnBrowseXsdHead_Click(object sender, EventArgs e, TextBox txtXsdHeadPath)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XSD dosyaları (*.xsd)|*.xsd|Tüm dosyalar (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtXsdHeadPath.Text = openFileDialog.FileName;
                }
            }
        }

        public static void btnBrowseXsdPacs_Click(object sender, EventArgs e, TextBox txtXsdPacsPath)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XSD dosyaları (*.xsd)|*.xsd|Tüm dosyalar (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtXsdPacsPath.Text = openFileDialog.FileName;
                }
            }
        }

        public static void btnValidate_Click(object sender, EventArgs e, TextBox txtXmlPath, TextBox txtXsdHeadPath, TextBox txtXsdPacsPath, RichTextBox txtResults, RichTextBox richTextBox1, List<string> validationErrors)
        {
            if (string.IsNullOrEmpty(txtXmlPath.Text) ||
                string.IsNullOrEmpty(txtXsdHeadPath.Text) ||
                string.IsNullOrEmpty(txtXsdPacsPath.Text))
            {
                MessageBox.Show("Lütfen tüm dosya yollarını belirtin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(txtXmlPath.Text))
            {
                MessageBox.Show("XML dosyası bulunamadı.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtXsdHeadPath.Text))
            {
                MessageBox.Show("XSD header dosyası bulunamadı.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtXsdPacsPath.Text))
            {
                MessageBox.Show("XSD document dosyası bulunamadı.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                txtResults.Clear();
                txtResults.AppendText("XML/XSD Validasyon başlatılıyor...\r\n\r\n");

                // XML dosyasını yükle
                XDocument xmlDoc = XDocument.Load(txtXmlPath.Text);
                txtResults.AppendText("XML dosyası başarıyla yüklendi.\r\n");

                // XML'i RichTextBox'ta göster
                XmlOperations.DisplayXmlInRichTextBox(xmlDoc, richTextBox1);



                // Header validasyonu 
                var (headValid, headErrors) = ValidateHeadSection(xmlDoc, txtXsdHeadPath.Text, txtResults);
                var (pacsValid, pacsErrors) = ValidatePacsSection(xmlDoc, txtXsdPacsPath.Text, txtResults);

                // Tüm hataları topla
                var allErrors = new List<string>();
                allErrors.AddRange(headErrors);
                allErrors.AddRange(pacsErrors);

                // Validasyon hatalarını sakla (XML editör için)
                validationErrors.Clear();
                validationErrors.AddRange(allErrors);

                // Validasyon sonucunu göster
                ShowValidationResult(allErrors, txtResults);

                txtResults.AppendText("\r\nValidasyon tamamlandı.\r\n");
            }
            catch (Exception ex)
            {
                txtResults.AppendText($"Hata oluştu: {ex.Message}\r\n");
                MessageBox.Show($"Validasyon sırasında hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static (bool isValid, List<string> errors) ValidateHeadSection(XDocument xmlDoc, string xsdHeadPath, RichTextBox txtResults)
        {
            bool isValid = true;
            List<string> errors = new List<string>();
            txtResults.AppendText("\r\n=== Header Validasyonu ===\r\n");

            try
            {
                // AppHdr elementini bul
                var appHdrElement = xmlDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "AppHdr");

                if (appHdrElement == null)
                {
                    txtResults.AppendText(" Header elementi bulunamadı.\r\n");
                    errors.Add("Header elementi bulunamadı.");
                    return (false, errors);
                }

                txtResults.AppendText(" Header elementi bulundu.\r\n");

                // XSD şema analizörünü oluştur
                var schemaAnalyzer = new XsdSchemaAnalyzer(xsdHeadPath);

                // Header XSD şemasını yükle
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add("", xsdHeadPath);

                // AppHdr elementini ayrı bir XML olarak oluştur
                XDocument headDoc = new XDocument(appHdrElement);

                // Validasyon yap
                headDoc.Validate(schemaSet, (o, e) =>
                {
                    errors.Add(e.Message);
                });

                if (errors.Count == 0)
                {
                    txtResults.AppendText(" Header validasyonu başarılı.\r\n");
                }
                else
                {
                    txtResults.AppendText(" Header validasyon hataları:\r\n");
                    foreach (var error in errors)
                    {
                        txtResults.AppendText($"\r\n{error}\r\n");
                        txtResults.AppendText("─".PadRight(50, '─') + "\r\n");
                        isValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                txtResults.AppendText($" Header validasyon hatası: {ex.Message}\r\n");
                isValid = false;
            }
            return (isValid, errors);
        }

        private static (bool isValid, List<string> errors) ValidatePacsSection(XDocument xmlDoc, string xsdPacsPath, RichTextBox txtResults)
        {
            bool isValid = true;
            List<string> errors = new List<string>();
            txtResults.AppendText("\r\n=== Document Validasyonu ===\r\n");

            try
            {
                var documentElement = xmlDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Document");

                if (documentElement == null)
                {
                    txtResults.AppendText(" Document elementi bulunamadı.\r\n");
                    errors.Add("Document elementi bulunamadı.");
                    return (false, errors);
                }

                // XSD şema analizörünü oluştur
                var schemaAnalyzer = new XsdSchemaAnalyzer(xsdPacsPath);

                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add("", xsdPacsPath);

                XDocument pacsDoc = new XDocument(documentElement);

                pacsDoc.Validate(schemaSet, (o, e) =>
                {
                    errors.Add(e.Message);
                });

                if (errors.Count == 0)
                {
                    txtResults.AppendText(" Document validasyonu başarılı.\r\n");
                }
                else
                {
                    isValid = false;
                    txtResults.AppendText(" Document validasyon hataları:\r\n");

                    foreach (var error in errors)
                    {
                        txtResults.AppendText($"\r\n{error}\r\n");
                        txtResults.AppendText("─".PadRight(50, '─') + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                txtResults.AppendText($" Document validasyon hatası: {ex.Message}\r\n");
                isValid = false;
            }

            return (isValid, errors);
        }

        public static void ShowValidationResult(List<string> errors, RichTextBox txtResults)
        {
            txtResults.AppendText("\r\n=== VALİDASYON SONUCU ===\r\n");

            if (errors.Count == 0)
            {
                txtResults.AppendText("XML dosyası Valid. \r\n");
            }
            else
            {
                txtResults.AppendText($"XML dosyası Invalid.  {errors.Count} hata bulundu\r\n");

                txtResults.AppendText("\r\nHata detayları:\r\n");
                txtResults.AppendText("─".PadRight(50, '─') + "\r\n");

                foreach (var error in errors)
                {
                    txtResults.AppendText($"• {error}\r\n");
                }
            }

            txtResults.AppendText("─".PadRight(50, '─') + "\r\n");

            // NOT: Validasyon hatalarını XML editörde gösterme - sadece "XML Editöre Yükle" butonuna basıldığında gösterilir
        }

        public static void btnEditor_Click(object sender, EventArgs e, TextBox txtXmlPath, TextBox txtXsdPacsPath, TextBox txtXsdHeadPath, DataGridView dataGridViewXml, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer, XDocument currentXmlDoc, List<TreeGridRow> treeGridData, List<string> validationErrors)
        {
            if (!File.Exists(txtXmlPath.Text))
            {
                MessageBox.Show("Geçerli bir XML dosya yolu girin.");
                return;
            }

            if (!File.Exists(txtXsdPacsPath.Text))
            {
                MessageBox.Show("Geçerli bir Document XSD dosya yolu girin.");
                return;
            }

            if (!File.Exists(txtXsdHeadPath.Text))
            {
                MessageBox.Show("Geçerli bir Header XSD dosya yolu girin.");
                return;
            }

            try
            {
                // Document XSD analizörünü başlat 
                schemaAnalyzer = new XsdSchemaAnalyzer(txtXsdPacsPath.Text);

                // Header XSD analizörünü de başlat 
                headerSchemaAnalyzer = new XsdSchemaAnalyzer(txtXsdHeadPath.Text);

                // Xml dosyasını yükle
                currentXmlDoc = XDocument.Load(txtXmlPath.Text);

                
                dataGridViewXml.Visible = true;
                LoadXmlContentToGrid(currentXmlDoc, dataGridViewXml, treeGridData, txtXsdPacsPath.Text, txtXsdHeadPath.Text);

             
                if (validationErrors != null && validationErrors.Count > 0)
                {
                    AnalyzeValidationErrors(validationErrors, treeGridData);
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Editör yüklenirken hata oluştu: {ex.Message}");
            }
        }

        public static void LoadXmlContentToGrid(XDocument xmlDoc, DataGridView dataGridViewXml, List<TreeGridRow> treeGridData, string xsdPacsPath, string xsdHeadPath)
        {
            try
            {
                Console.WriteLine("LoadXmlContentToGrid başladı");

                dataGridViewXml.Rows.Clear();
                treeGridData.Clear();

                if (xmlDoc != null)
                {
                    Console.WriteLine("XML dokümanı yükleniyor...");

                    // XML'i TreeGrid yapısına yükle
                    LoadXmlToTreeGrid(xmlDoc.Root, 0, "", treeGridData);
                    Console.WriteLine($"TreeGrid yüklendi: {treeGridData.Count} satır");
                    
                    // Create schema analyzers and load XSD constraints
                    XsdSchemaAnalyzer schemaAnalyzer = null;
                    XsdSchemaAnalyzer headerSchemaAnalyzer = null;
                    
                    try
                    {
                        if (!string.IsNullOrEmpty(xsdPacsPath) && System.IO.File.Exists(xsdPacsPath))
                        {
                            schemaAnalyzer = new XsdSchemaAnalyzer(xsdPacsPath);
                        }
                        if (!string.IsNullOrEmpty(xsdHeadPath) && System.IO.File.Exists(xsdHeadPath))
                        {
                            headerSchemaAnalyzer = new XsdSchemaAnalyzer(xsdHeadPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Schema analyzer creation error: {ex.Message}");
                    }
                    
                    DataGridOperations.RefreshTreeGridDisplay(dataGridViewXml, treeGridData, schemaAnalyzer, headerSchemaAnalyzer);
                }
                else
                {
                    Console.WriteLine("XML dosyası bulunamadı");
                    dataGridViewXml.Rows.Add("Bilgi", "XML dosyası yüklenmemiş");
                }

                Console.WriteLine("LoadXmlContentToGrid tamamlandı");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadXmlContentToGrid hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                dataGridViewXml.Rows.Add("Hata", ex.Message);
            }
        }

        public static TreeGridRow LoadXmlToTreeGrid(XElement element, int level, string parentPath, List<TreeGridRow> treeGridData)
        {
            try
            {
                if (element == null) return null;

                string currentPath = string.IsNullOrEmpty(parentPath) ? element.Name.LocalName : $"{parentPath}/{element.Name.LocalName}";

                // Ana element satırını oluştur
                var mainRow = new TreeGridRow
                {
                    FieldName = element.Name.LocalName ?? "Unknown",
                    Value = element.HasElements ? "" : (element.Value ?? ""),
                    Level = level,
                    IsHeader = element.HasElements,
                    IsExpanded = true,
                    Element = element,
                    Path = currentPath,
              
                    IsValid = true,
                    ValidationError = "",
                    IsMissing = false
                };

                // Veri tipini belirle
                if (!element.HasElements)
                {
                    var value = element.Value ?? "";

              
                    if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("no", StringComparison.OrdinalIgnoreCase))
                    {
                        mainRow.DataType = "boolean";
                     
                        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                            value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                            mainRow.Value = "true";
                        else if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                            value.Equals("no", StringComparison.OrdinalIgnoreCase))
                            mainRow.Value = "false";

                        // Boolean alanlar için enum değerleri ekle
                        mainRow.EnumValues = new List<string> { "true", "false" };
                        Console.WriteLine($" Boolean alan tespit edildi: {mainRow.FieldName} = {mainRow.Value} (true/false seçenekleri)");
                    }
                   
                    else if (ValidationHelper.IsValidDateFormat(value))
                    {
                        mainRow.DataType = "date";
                        Console.WriteLine($" Tarih alanı tespit edildi: {mainRow.FieldName} = {mainRow.Value}");
                    }
                    // Sayısal kontrolü - daha kapsamlı
                    else if (decimal.TryParse(value, out _) ||
                             int.TryParse(value, out _) ||
                             double.TryParse(value, out _))
                    {
                        mainRow.DataType = "number";
                    }
                    else
                    {
                        // Varsayılan olarak string
                        mainRow.DataType = "string";
                        mainRow.EnumValues = new List<string>();
                        Console.WriteLine($" String olarak ayarlandı: {mainRow.FieldName}");
                    }
                }

                treeGridData.Add(mainRow);

                // Alt elementleri recursive olarak ekle ve parent-child ilişkisini kur
                if (element.HasElements)
                {
                    try
                    {
                        foreach (var child in element.Elements())
                        {
                            var childRow = LoadXmlToTreeGrid(child, level + 1, currentPath, treeGridData);
                            if (childRow != null)
                            {
                                mainRow.Children.Add(childRow);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Alt elementler yüklenirken hata: {ex.Message}");
                    }
                }

                return mainRow;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadXmlToTreeGrid hatası: {ex.Message}");
                return null;
            }
        }

        private static void AnalyzeValidationErrors(List<string> validationErrors, List<TreeGridRow> treeGridData)
        {
            try
            {
                Console.WriteLine($" AnalyzeValidationErrors çağrıldı - {validationErrors.Count} hata");

                // Tüm alanları önce geçerli olarak işaretle
                foreach (var row in treeGridData)
                {
                    row.IsValid = true;
                    row.ValidationError = "";
                    row.IsMissing = false;
                }

                Console.WriteLine($" {treeGridData.Count} alan sıfırlandı");

                // Validasyon hatalarını analiz et
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($" Validasyon hatası analiz ediliyor: {error}");

                    // Eksik element hatalarını kontrol et
                    if (error.Contains("AppHdr") || error.Contains("AppHdr elementi bulunamadı"))
                    {
                        Console.WriteLine($" AppHdr hatası bulundu, Header alanları işaretleniyor");
                        // AppHdr elementi eksik - Header alanlarını işaretle
                        MarkHeaderFieldsAsInvalid(treeGridData, "AppHdr elementi bulunamadı");
                    }

                    // Diğer validasyon hatalarını da kontrol et
                    if (error.Contains("elementi bulunamadı"))
                    {
                        // Genel eksik element hatası
                        var elementName = ExtractElementNameFromError(error);
                        if (!string.IsNullOrEmpty(elementName))
                        {
                            Console.WriteLine($"  -> Eksik element: {elementName}");
                            MarkElementAsInvalid(treeGridData, elementName, error);
                        }
                    }

                    // Pattern, length, range gibi diğer validasyon hatalarını da kontrol et
                    if (error.Contains("pattern") || error.Contains("length") || error.Contains("range"))
                    {
                        // Değer validasyon hatası - ilgili alanı bul ve işaretle
                        MarkValueValidationError(treeGridData, error);
                    }
                }

                Console.WriteLine($" Grid yenileniyor...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" AnalyzeValidationErrors hatası: {ex.Message}");
            }
        }

        // Header alanlarını geçersiz olarak işaretle
        private static void MarkHeaderFieldsAsInvalid(List<TreeGridRow> treeGridData, string errorMessage)
        {
            Console.WriteLine($" MarkHeaderFieldsAsInvalid çağrıldı: {errorMessage}");

            foreach (var row in treeGridData)
            {
                // Header seviyesi ve Header altındaki tüm alanları işaretle
                if (row.Level == 1 && row.FieldName == "Header")
                {
                    Console.WriteLine($"  Header elementi işaretleniyor: {row.FieldName}");
                    row.IsValid = false;
                    row.ValidationError = errorMessage;
                    row.IsMissing = true;

                    // Header altındaki tüm alanları da işaretle
                    MarkChildrenAsInvalid(row, errorMessage);
                }
                // AppHdr elementi eksik olduğunda, Header ile ilgili diğer alanları da işaretle
                else if (row.FieldName == "Sender" || row.FieldName == "Receiver" || row.FieldName == "MessageType")
                {
                    Console.WriteLine($" Header ile ilgili alan işaretleniyor: {row.FieldName}");
                    row.IsValid = false;
                    row.ValidationError = errorMessage;
                    row.IsMissing = false; // Eksik değil, sadece invalid
                }
            }
        }

        // Alt elementleri recursive olarak invalid olarak işaretle
        private static void MarkChildrenAsInvalid(TreeGridRow parent, string errorMessage)
        {
            foreach (var child in parent.Children)
            {
                Console.WriteLine($"  Alt element işaretleniyor: {child.FieldName}");
                child.IsValid = false;
                child.ValidationError = errorMessage;
                child.IsMissing = true;

                // Recursive olarak alt elementleri de işaretle
                MarkChildrenAsInvalid(child, errorMessage);
            }
        }

        // Belirli bir elementi geçersiz olarak işaretle
        private static void MarkElementAsInvalid(List<TreeGridRow> treeGridData, string elementName, string errorMessage)
        {
            foreach (var row in treeGridData)
            {
                if (row.FieldName == elementName)
                {
                    row.IsValid = false;
                    row.ValidationError = errorMessage;
                    row.IsMissing = true;
                }
            }
        }

        // Değer validasyon hatalarını işaretle
        private static void MarkValueValidationError(List<TreeGridRow> treeGridData, string errorMessage)
        {
            // Hata mesajından alan adını çıkarmaya çalış
            var fieldName = ExtractFieldNameFromError(errorMessage);
            if (!string.IsNullOrEmpty(fieldName))
            {
                foreach (var row in treeGridData)
                {
                    if (row.FieldName == fieldName)
                    {
                        row.IsValid = false;
                        row.ValidationError = errorMessage;
                    }
                }
            }
        }

        // Hata mesajından element adını çıkar
        private static string ExtractElementNameFromError(string error)
        {
            // "X elementi bulunamadı" formatından element adını çıkar
            if (error.Contains(" elementi bulunamadı"))
            {
                var parts = error.Split(" elementi bulunamadı");
                return parts[0].Trim();
            }
            return "";
        }

        // Hata mesajından alan adını çıkar
        private static string ExtractFieldNameFromError(string error)
        {
            // Pattern, length, range hatalarından alan adını çıkarmaya çalış
            // Bu kısım XSD validasyon mesajlarına göre özelleştirilebilir
            return "";
        }
    }
}
