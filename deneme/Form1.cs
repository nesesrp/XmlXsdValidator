using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using validator.Models;
using validator.Operations;
using validator.UI;
using validator.Validation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace validator
{
    public partial class Form1 : Form
    {
        private XsdSchemaAnalyzer schemaAnalyzer;
        private XsdSchemaAnalyzer headerSchemaAnalyzer;
        private XDocument _currentXmlDoc;
        private Dictionary<int, bool> _expandedRows = new Dictionary<int, bool>();
        private List<TreeGridRow> _treeGridData = new List<TreeGridRow>();
        private List<string> _validationErrors = new List<string>(); // Validasyon hatalarını tutmak için
        private ErrorProvider errorProvider; // ErrorProvider eklendi

        public Form1()
        {
            InitializeComponent();
            this.btnSaveChanges.Click += new System.EventHandler(this.btnXmlKaydet_Click);
            this.btnViewUpdatedXml_Click.Click += new System.EventHandler(this.btnGuncelleneniGor_Click);

           
            dataGridViewXml.Visible = false;

            // Validasyon hatalarını saklamak için liste başlat
            _validationErrors = new List<string>();
            
            // ErrorProvider'ı başlat
            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.Icon = SystemIcons.Error; 

        
            DataGridOperations.SetupDataGridView(dataGridViewXml);

       
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Hücre değeri değiştiğinde XML'i güncelle
            dataGridViewXml.CellValueChanged += DataGridViewXml_CellValueChanged;

            // Hücre validasyonu ekle
            dataGridViewXml.CellValidating += DataGridViewXml_CellValidating;

            // Hücre tıklama olayını ekle (expand/collapse için)
            dataGridViewXml.CellClick += DataGridViewXml_CellClick;

            // Hücre boyama olayını ekle (hiyerarşik görünüm için)
            dataGridViewXml.CellPainting += DataGridViewXml_CellPainting;

            // Klavye olaylarını ekle
            dataGridViewXml.KeyDown += DataGridViewXml_KeyDown;

            // Sağ tık menüsünü ekle
            dataGridViewXml.CellMouseClick += DataGridViewXml_CellMouseClick;

            // Tooltip için mouse enter event'i ekle
            dataGridViewXml.CellMouseEnter += DataGridViewXml_CellMouseEnter;

            // Hücre düzenleme kontrolü gösterildiğinde event'i ekle
            dataGridViewXml.EditingControlShowing += DataGridViewXml_EditingControlShowing;

            // DataError event'ini ekle - ComboBox hatalarını handle et
            dataGridViewXml.DataError += DataGridViewXml_DataError;

            // Veri tipi renklendirmesi ekle
            dataGridViewXml.CellFormatting += DataGridViewXml_CellFormatting;
        }

        private void btnBrowseXml_Click(object sender, EventArgs e)
        {
            FormEvents.btnBrowseXml_Click(sender, e, txtXmlPath);
        }

        private void btnBrowseXsdHead_Click(object sender, EventArgs e)
        {
            FormEvents.btnBrowseXsdHead_Click(sender, e, txtXsdHeadPath);
        }

        private void btnBrowseXsdPacs_Click(object sender, EventArgs e)
        {
            FormEvents.btnBrowseXsdPacs_Click(sender, e, txtXsdPacsPath);
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            FormEvents.btnValidate_Click(sender, e, txtXmlPath, txtXsdHeadPath, txtXsdPacsPath, txtResults, richTextBox1, _validationErrors);
        }

        private (bool isValid, List<string> errors) ValidateHeadSection(XDocument xmlDoc)
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
                var schemaAnalyzer = new XsdSchemaAnalyzer(txtXsdHeadPath.Text);

                // Header XSD şemasını yükle
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add("", txtXsdHeadPath.Text);

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

        private (bool isValid, List<string> errors) ValidatePacsSection(XDocument xmlDoc)
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
                var schemaAnalyzer = new XsdSchemaAnalyzer(txtXsdPacsPath.Text);

                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add("", txtXsdPacsPath.Text);

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

        private void DisplayXmlInRichTextBox(XDocument xmlDoc)
        {
            XmlOperations.DisplayXmlInRichTextBox(xmlDoc, richTextBox1);
        }

        private void ShowValidationResult(List<string> errors)
        {
            FormEvents.ShowValidationResult(errors, txtResults);
        }

        public XmlNodeModel ParseElement(XElement element)
        {
            return XmlOperations.ParseElement(element);
        }


        public XsdNodeModel ParseXsdElement(XElement element)
        {
            return XmlOperations.ParseXsdElement(element);
        }
        private void btnEditor_Click(object sender, EventArgs e)
        {
            FormEvents.btnEditor_Click(sender, e, txtXmlPath, txtXsdPacsPath, txtXsdHeadPath, dataGridViewXml, schemaAnalyzer, headerSchemaAnalyzer, _currentXmlDoc, _treeGridData, _validationErrors);
        }

        private void ShowDataTypeSummary()
        {
            try
            {
                var typeCounts = new Dictionary<string, int>();
                var enumFields = new List<string>();

                foreach (var row in _treeGridData)
                {
                    if (!row.IsHeader) // Sadece leaf elementler
                    {
                        if (typeCounts.ContainsKey(row.DataType))
                            typeCounts[row.DataType]++;
                        else
                            typeCounts[row.DataType] = 1;

                        if (row.DataType == "enum" && row.EnumValues != null && row.EnumValues.Count > 0)
                        {
                            enumFields.Add($"{row.FieldName} ({row.EnumValues.Count} değer)");
                        }
                    }
                }

                var summary = "XML Veri Tipi Özeti:\n\n";
                foreach (var kvp in typeCounts.OrderByDescending(x => x.Value))
                {
                    summary += $"{kvp.Key}: {kvp.Value} alan\n";
                }

                if (enumFields.Count > 0)
                {
                    summary += $"\nEnum Alanları:\n{string.Join("\n", enumFields)}";
                }

                // Filtreleme seçenekleri ekle
                summary += "\n\nFiltreleme için sağ tık menüsünü kullanın.";

                MessageBox.Show(summary, "Veri Tipi Özeti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowDataTypeSummary hatası: {ex.Message}");
            }
        }

        private void FilterByDataType(string dataType)
        {
            try
            {
                foreach (var row in _treeGridData)
                {
                    if (row.IsHeader)
                    {
                        // Header elementler için üst elementlerin expanded durumunu koru
                        continue;
                    }

                    // Sadece belirtilen veri tipindeki alanları göster
                    if (row.DataType == dataType)
                    {
                        // Üst elementleri expanded yap
                        var current = row;
                        while (current.Element?.Parent != null)
                        {
                            var parentRow = _treeGridData.FirstOrDefault(r => r.Element == current.Element.Parent);
                            if (parentRow != null)
                            {
                                parentRow.IsExpanded = true;
                                current = parentRow;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                RefreshTreeGridDisplay();
                MessageBox.Show($"'{dataType}' veri tipindeki alanlar filtrelendi.", "Filtreleme", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FilterByDataType hatası: {ex.Message}");
            }
        }


        // Kullanıcı xml'i düzenledikten sonra bu butona basarak güncelleme yapar
        private void btnXmlKaydet_Click(object sender, EventArgs e)
        {
            if (_currentXmlDoc == null)
            {
                MessageBox.Show("Önce XML'i ve XSD'yi yükleyin.");
                return;
            }

            try
            {
                // Grid'deki tüm değişiklikleri XML'e uygula
                ApplyGridChangesToXml();

                // Güncellenmiş XML'i memory'de tut (dosya olarak kaydetme)
                // XML sol tarafta gösterilmez, sadece "Güncellenen XML'i Gör" butonunda gösterilir

                MessageBox.Show("XML güncellendi! 'Güncellenen XML'i Gör' butonuna basarak sol tarafta görüntüleyebilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"XML kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyGridChangesToXml()
        {
            try
            {
                Console.WriteLine("ApplyGridChangesToXml başladı");

                if (_currentXmlDoc == null || _treeGridData.Count == 0)
                {
                    Console.WriteLine("_currentXmlDoc null veya _treeGridData boş");
                    return;
                }

                Console.WriteLine($"XML güncelleme öncesi: {_currentXmlDoc.ToString().Substring(0, Math.Min(200, _currentXmlDoc.ToString().Length))}...");

                // Revision elementini özel olarak kontrol et
                var revisionBefore = _currentXmlDoc.Descendants("Revision").FirstOrDefault();
                if (revisionBefore != null)
                {
                    Console.WriteLine($" Güncelleme öncesi Revision değeri: '{revisionBefore.Value}'");
                }

                int updatedCount = 0;
                foreach (var treeRow in _treeGridData)
                {
                    if (!string.IsNullOrEmpty(treeRow.FieldName) && !treeRow.IsHeader)
                    {
                        Console.WriteLine($"Güncelleniyor: {treeRow.FieldName} = '{treeRow.Value}' (Path: {treeRow.Path})");

                        // Path varsa daha spesifik güncelleme yap
                        if (!string.IsNullOrEmpty(treeRow.Path))
                        {
                            UpdateXmlElementByPath(treeRow.Path, treeRow.Value);
                        }
                        else
                        {
                            // Path yoksa eski yöntemi kullan
                            UpdateXmlElementByName(treeRow.FieldName, treeRow.Value);
                        }

                        updatedCount++;
                    }
                }

                Console.WriteLine($"Toplam {updatedCount} element güncellendi");
                Console.WriteLine($"XML güncelleme sonrası: {_currentXmlDoc.ToString().Substring(0, Math.Min(200, _currentXmlDoc.ToString().Length))}...");

                // Güncellenmiş XML'i kontrol et
                if (updatedCount > 0)
                {
                    Console.WriteLine("Güncellenmiş XML içeriği kontrol ediliyor...");
                    var revisionElement = _currentXmlDoc.Descendants("Revision").FirstOrDefault();
                    if (revisionElement != null)
                    {
                        Console.WriteLine($"Revision element değeri: '{revisionElement.Value}'");

                        // XML string'ini de kontrol et
                        var xmlString = _currentXmlDoc.ToString();
                        if (xmlString.Contains("Revision"))
                        {
                            var revisionIndex = xmlString.IndexOf("Revision");
                            var revisionEndIndex = xmlString.IndexOf(">", revisionIndex);
                            if (revisionEndIndex > revisionIndex)
                            {
                                var revisionLine = xmlString.Substring(revisionIndex, revisionEndIndex - revisionIndex + 1);
                                Console.WriteLine($" XML string'inde Revision satırı: {revisionLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyGridChangesToXml hatası: {ex.Message}");
                MessageBox.Show($"Grid değişiklikleri XML'e uygulanırken hata oluştu: {ex.Message}");
            }
        }


        // güncellen xmli gösteren buton
        private void btnGuncelleneniGor_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentXmlDoc == null)
                {
                    MessageBox.Show("Önce XML'i düzenleyip 'Kaydet' butonuna basmalısınız.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Console.WriteLine($" btnGuncelleneniGor_Click: _currentXmlDoc null mu? {_currentXmlDoc == null}");
                Console.WriteLine($" XML Root Element: {_currentXmlDoc.Root?.Name.LocalName}");
                Console.WriteLine($" XML Content Length: {_currentXmlDoc.ToString().Length}");

                // Önce Grid'deki değişiklikleri XML'e uygula (eğer henüz uygulanmamışsa)
                Console.WriteLine(" Grid değişiklikleri XML'e uygulanıyor...");
                ApplyGridChangesToXml();

                // Revision elementini kontrol et
                var revisionElement = _currentXmlDoc.Descendants("Revision").FirstOrDefault();
                if (revisionElement != null)
                {
                    Console.WriteLine($" Güncelleme sonrası Revision değeri: '{revisionElement.Value}'");
                }

           
                Console.WriteLine(" XML sol tarafta gösteriliyor...");

            
                var testRevision = _currentXmlDoc.Descendants("Revision").FirstOrDefault();
                if (testRevision != null)
                {
                    Console.WriteLine($" DisplayXmlInRichTextBox öncesi Revision: '{testRevision.Value}'");
                }

                DisplayXmlInRichTextBox(_currentXmlDoc);

          
                richTextBox1.Refresh();
                richTextBox1.Invalidate();
                richTextBox1.Update();
                Console.WriteLine($" RichTextBox güncellendi. Text uzunluğu: {richTextBox1.Text.Length}");

                
                var currentText = richTextBox1.Text;
                if (currentText.Contains("Revision"))
                {
                    var revisionIndex = currentText.IndexOf("Revision");
                    var revisionEndIndex = currentText.IndexOf(">", revisionIndex);
                    if (revisionEndIndex > revisionIndex)
                    {
                        var revisionLine = currentText.Substring(revisionIndex, revisionEndIndex - revisionIndex + 1);
                        Console.WriteLine($" RichTextBox'ta Revision satırı: {revisionLine}");
                    }
                }

                // Sağ taraftaki DataGridView'de de güncelle (sadece display, veri kaybı olmadan)
                if (dataGridViewXml.Visible)
                {
                    RefreshTreeGridDisplay();
                }

                MessageBox.Show("Güncellenmiş XML sol tarafta gösteriliyor!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" btnGuncelleneniGor_Click hatası: {ex.Message}");
                MessageBox.Show($"XML gösterilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        // DataError event'ini handle et - ComboBox hatalarını önle
        private void DataGridViewXml_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                Console.WriteLine($"DataError event tetiklendi: Row={e.RowIndex}, Column={e.ColumnIndex}, Context={e.Context}, Exception={e.Exception?.Message}");

                // ComboBox hatası ise
                if (e.Exception is ArgumentException && e.Exception.Message.Contains("DataGridViewComboBoxCell"))
                {
                    Console.WriteLine("ComboBox hatası tespit edildi, hata mesajı gösterilmeyecek");

                    // Hata mesajını gösterme
                    e.ThrowException = false;

                    // Hücreyi güvenli bir değerle güncelle
                    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                    {
                        var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
                        if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                        {
                            var treeRow = _treeGridData[visibleRowIndex];

                            // ComboBox için geçerli bir değer seç
                            if (treeRow.DataType == "enum" && treeRow.EnumValues != null && treeRow.EnumValues.Count > 0)
                            {
                                // İlk enum değerini kullan
                                var safeValue = treeRow.EnumValues[0];
                                dataGridViewXml.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = safeValue;

                                // TreeGrid'i güncelle
                                treeRow.Value = safeValue;
                                Console.WriteLine($"ComboBox hatası düzeltildi: {treeRow.FieldName} = {safeValue}");
                            }
                        }
                    }
                }
                else
                {
                    // Diğer hatalar için varsayılan davranış
                    Console.WriteLine($"Diğer DataError: {e.Exception?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataError handler hatası: {ex.Message}");
            }
        }

        private void DataGridViewXml_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                // Sağ tık menüsü göster
                ShowContextMenu(e.RowIndex, e.ColumnIndex);
            }
        }

        private void DataGridViewXml_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
                if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                {
                    var treeRow = _treeGridData[visibleRowIndex];
                    var tooltipText = $"Field: {treeRow.FieldName}\nData Type: {treeRow.DataType}";

                    if (treeRow.DataType == "enum" && treeRow.EnumValues != null && treeRow.EnumValues.Count > 0)
                    {
                        tooltipText += $"\nEnum Values: {string.Join(", ", treeRow.EnumValues)}";
                    }

                    if (!string.IsNullOrEmpty(treeRow.Path))
                    {
                        tooltipText += $"\nPath: {treeRow.Path}";
                    }

                    // Tooltip'i göster
                    var cellRect = dataGridViewXml.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var screenPoint = dataGridViewXml.PointToScreen(new Point(cellRect.Left, cellRect.Top));

                    // Mevcut tooltip'i temizle ve yenisini göster
                    var toolTip = new ToolTip();
                    toolTip.Show(tooltipText, dataGridViewXml, cellRect.Left, cellRect.Top - 20, 3000);
                }
            }
        }







        private void ShowContextMenu(int rowIndex, int columnIndex)
        {
            try
            {
                var visibleRowIndex = GetVisibleRowIndex(rowIndex);
                if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                {
                    var treeRow = _treeGridData[visibleRowIndex];

                    var contextMenu = new ContextMenuStrip();

                    // Expand/Collapse menüsü
                    if (treeRow.Children.Count > 0)
                    {
                        var expandText = treeRow.IsExpanded ? "Collapse" : "Expand";
                        var expandItem = new ToolStripMenuItem(expandText);
                        expandItem.Click += (s, e) => ToggleRowExpansion(visibleRowIndex);
                        contextMenu.Items.Add(expandItem);

                        contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // Genel işlemler
                    var expandAllItem = new ToolStripMenuItem("Expand All");
                    expandAllItem.Click += (s, e) => ExpandAllRows();
                    contextMenu.Items.Add(expandAllItem);

                    var collapseAllItem = new ToolStripMenuItem("Collapse All");
                    collapseAllItem.Click += (s, e) => CollapseAllRows();
                    contextMenu.Items.Add(collapseAllItem);

                    // Validasyon özeti
                    contextMenu.Items.Add(new ToolStripSeparator());
                    var validationItem = new ToolStripMenuItem("Validasyon Özeti");
                    validationItem.Click += (s, e) => ShowValidationSummary();
                    contextMenu.Items.Add(validationItem);

                    // Filtreleme seçenekleri
                    contextMenu.Items.Add(new ToolStripSeparator());
                    var filterLabel = new ToolStripMenuItem("Filtrele:");
                    filterLabel.Enabled = false;
                    contextMenu.Items.Add(filterLabel);

                    var filterStringItem = new ToolStripMenuItem("String Alanları");
                    filterStringItem.Click += (s, e) => FilterByDataType("string");
                    contextMenu.Items.Add(filterStringItem);

                    var filterNumberItem = new ToolStripMenuItem("Sayısal Alanlar");
                    filterNumberItem.Click += (s, e) => FilterByDataType("number");
                    contextMenu.Items.Add(filterNumberItem);

                    var filterBooleanItem = new ToolStripMenuItem("Boolean Alanlar");
                    filterBooleanItem.Click += (s, e) => FilterByDataType("boolean");
                    contextMenu.Items.Add(filterBooleanItem);

                    var filterEnumItem = new ToolStripMenuItem("Enum Alanlar");
                    filterEnumItem.Click += (s, e) => FilterByDataType("enum");
                    contextMenu.Items.Add(filterEnumItem);

                    var clearFilterItem = new ToolStripMenuItem("Filtreyi Temizle");
                    clearFilterItem.Click += (s, e) => { ExpandAllRows(); };
                    contextMenu.Items.Add(clearFilterItem);

                    // Element bilgisi
                    contextMenu.Items.Add(new ToolStripSeparator());
                    var infoItem = new ToolStripMenuItem($"Element: {treeRow.FieldName}");
                    infoItem.Enabled = false;
                    contextMenu.Items.Add(infoItem);

                    // Veri tipi bilgisi
                    var dataTypeItem = new ToolStripMenuItem($"Data Type: {treeRow.DataType}");
                    dataTypeItem.Enabled = false;
                    contextMenu.Items.Add(dataTypeItem);

                    if (treeRow.DataType == "enum" && treeRow.EnumValues != null && treeRow.EnumValues.Count > 0)
                    {
                        var enumItem = new ToolStripMenuItem($"Enum Values: {string.Join(", ", treeRow.EnumValues)}");
                        enumItem.Enabled = false;
                        contextMenu.Items.Add(enumItem);
                    }

                    if (!string.IsNullOrEmpty(treeRow.Path))
                    {
                        var pathItem = new ToolStripMenuItem($"Path: {treeRow.Path}");
                        pathItem.Enabled = false;
                        contextMenu.Items.Add(pathItem);
                    }

                    // Menüyü göster
                    var cellRect = dataGridViewXml.GetCellDisplayRectangle(columnIndex, rowIndex, false);
                    var screenPoint = dataGridViewXml.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    contextMenu.Show(screenPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Context menu error: {ex.Message}");
            }
        }

        private void DataGridViewXml_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Add: // Ctrl + Plus
                        ExpandAllRows();
                        e.Handled = true;
                        break;
                    case Keys.Subtract: // Ctrl + Minus
                        CollapseAllRows();
                        e.Handled = true;
                        break;
                    case Keys.Enter: // Ctrl + Enter
                        if (dataGridViewXml.CurrentRow != null)
                        {
                            var currentRowIndex = dataGridViewXml.CurrentRow.Index;
                            var visibleRowIndex = GetVisibleRowIndex(currentRowIndex);
                            if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                            {
                                ToggleRowExpansion(visibleRowIndex);
                            }
                        }
                        e.Handled = true;
                        break;
                    case Keys.V: // Ctrl + V - Validasyon özeti
                        ShowValidationSummary();
                        e.Handled = true;
                        break;
                }
            }
            else if (e.KeyCode == Keys.Space)
            {
                // Space tuşu ile expand/collapse
                if (dataGridViewXml.CurrentRow != null)
                {
                    var currentRowIndex = dataGridViewXml.CurrentRow.Index;
                    var visibleRowIndex = GetVisibleRowIndex(currentRowIndex);
                    if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                    {
                        ToggleRowExpansion(visibleRowIndex);
                    }
                }
                e.Handled = true;
            }
        }





        private void DataGridViewXml_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    if (e.ColumnIndex == dataGridViewXml.Columns["FieldName"].Index)
                    {
                        // Field Name sütununa tıklandığında expand/collapse yap
                        ToggleRowExpansion(e.RowIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CellClick hatası: {ex.Message}");
            }
        }

        private void DataGridViewXml_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridViewXml.Columns["FieldName"].Index)
                {
                    // Field Name sütunu için hiyerarşik görünüm
                    PaintFieldNameColumn(e);
                }
                else if (e.ColumnIndex == dataGridViewXml.Columns["Value"].Index)
                {
                    // Value sütunu için ünlem işareti
                    PaintValueColumn(e);
                }
            }
        }

        private void PaintFieldNameColumn(DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            if (e.RowIndex < _treeGridData.Count)
            {
                var row = _treeGridData[e.RowIndex];

                if (row.Children.Count > 0)
                {
                    // İkon pozisyonu - sol tarafta
                    var iconRect = new Rectangle(
                        e.CellBounds.Left + 10 + (row.Level * 20), 
                        e.CellBounds.Top + 20, 
                        16, 16); // İkon boyutu

                    Color iconColor = Color.Black;
                    string iconText = row.IsExpanded ? "▼" : "▶";

                    using (var brush = new SolidBrush(iconColor))
                    using (var font = new Font("Arial", 12, FontStyle.Regular))
                    {
                        e.Graphics.DrawString(iconText, font, brush, iconRect);
                    }
                }

               
                var textRect = new Rectangle(
                    e.CellBounds.Left + 35 + (row.Level * 20), // İkon için 35px, her seviye için 20px indent
            e.CellBounds.Top + 20, // Yüksekliği artır
            e.CellBounds.Width - 40 - (row.Level * 20), // Geniş metin alanı
            e.CellBounds.Height - 40); // Yüksekliği artır

                using (var brush = new SolidBrush(Color.Black))
                using (var font = new Font("Arial", 9, FontStyle.Bold))
                {
                    string displayText = row.FieldName;

                    if (displayText.Length > 60)
                    {
                        displayText = displayText.Substring(0, 57) + "...";
                    }

                    e.Graphics.DrawString(displayText, font, brush, textRect);
                }
            }

            e.Handled = true;
        }

        private void PaintValueColumn(DataGridViewCellPaintingEventArgs e)
        {
            var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
            if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
            {
                var treeRow = _treeGridData[visibleRowIndex];
                if (!treeRow.IsHeader && !treeRow.IsValid)
                {
                    // Hatalı alan için ünlem işareti çiz
                    var cellRect = e.CellBounds;
                    var iconSize = 16;
                    var iconX = cellRect.Right - iconSize - 5;
                    var iconY = cellRect.Top + (cellRect.Height - iconSize) / 2;

                    // Ünlem işareti çiz
                    using (var brush = new SolidBrush(Color.Red))
                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    {
                        e.Graphics.DrawString("⚠", font, brush, iconX, iconY);
                    }

                    e.Handled = true;
                }
            }
        }

                private void DataGridViewXml_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridViewXml.Columns["Value"].Index)
            {
                var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
                if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                {
                    var treeRow = _treeGridData[visibleRowIndex];
                    
                    // Header elementler için renklendirme yapma
                    if (treeRow.IsHeader)
                    {
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = Color.Black;
                        // Header elementler için ErrorProvider'ı temizle
                        ClearErrorProviderForRow(e.RowIndex);
                        return;
                    }
                    
                    // Alan değerini al
                    var cellValue = dataGridViewXml.Rows[e.RowIndex].Cells["Value"].Value?.ToString() ?? "";
                    
                    // XSD kısıtlamalarına göre detaylı validasyon yap
                    var validationResult = ValidateFieldWithXsdConstraints(treeRow, cellValue);
                    
                    // Validasyon durumuna göre renklendirme - SADECE 2 RENK
                    if (!validationResult.IsValid)
                    {
                        // Hata durumu - kırmızı arka plan
                        e.CellStyle.BackColor = Color.FromArgb(255, 255, 200, 200); // Açık kırmızı
                        e.CellStyle.ForeColor = Color.DarkRed;
                        e.CellStyle.SelectionBackColor = Color.FromArgb(255, 255, 150, 150);
                        e.CellStyle.SelectionForeColor = Color.DarkRed;
                        
                        // ErrorProvider ile hata ikonu göster
                        ShowErrorProviderForRow(e.RowIndex, validationResult.ErrorMessage);
                    }
                    else
                    {
                        // Geçerli durum - normal beyaz arka plan
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = Color.Black;
                        e.CellStyle.SelectionBackColor = Color.LightGray;
                        e.CellStyle.SelectionForeColor = Color.Black;
                        
                        // ErrorProvider'ı temizle
                        ClearErrorProviderForRow(e.RowIndex);
                    }
                    
                    // TreeGrid'de validasyon durumunu güncelle
                    treeRow.IsValid = validationResult.IsValid;
                    treeRow.ValidationError = validationResult.ErrorMessage;
                }
            }
        }

        // Alan değerinin geçerli olup olmadığını kontrol eden metod - XSD pattern'ları ile
        private bool IsFieldValueValid(TreeGridRow treeRow, string value)
        {
            return ValidationHelper.IsFieldValueValid(treeRow, value, schemaAnalyzer);
        }

        // XSD kısıtlamalarına göre detaylı validasyon yapan metod - Sadece pattern kontrolü
        private ValidationResult ValidateFieldWithXsdConstraints(TreeGridRow treeRow, string value)
        {
            return ValidationHelper.ValidateFieldWithXsdConstraints(treeRow, value, schemaAnalyzer, headerSchemaAnalyzer);
        }

        // Yardımcı metodlar
        private int GetDecimalPlaces(string value)
        {
            return ValidationHelper.GetDecimalPlaces(value);
        }

        private int GetTotalDigits(string value)
        {
            return ValidationHelper.GetTotalDigits(value);
        }

        private bool IsValidDateFormat(string value)
        {
            return ValidationHelper.IsValidDateFormat(value);
        }

        // Alan kısıtlamalarını al
        private XsdSchemaAnalyzer.SchemaConstraints GetFieldConstraints(TreeGridRow treeRow)
        {
            return ValidationHelper.GetFieldConstraints(treeRow, schemaAnalyzer, headerSchemaAnalyzer);
        }



        // Tüm alanları yeniden validasyon yaparak renklendirme
        private void RefreshValidationColors()
        {
            try
            {
                dataGridViewXml.Refresh(); // CellFormatting event'ini tetikler
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshValidationColors hatası: {ex.Message}");
            }
        }

        // ErrorProvider ile hata ikonu göster
        private void ShowErrorProviderForRow(int rowIndex, string errorMessage)
        {
            try
            {
                if (errorProvider != null && rowIndex >= 0 && rowIndex < dataGridViewXml.Rows.Count)
                {
                    var cell = dataGridViewXml.Rows[rowIndex].Cells["Value"];
                    if (cell != null)
                    {
                        // DataGridViewCell'i Control'e dönüştüremeyiz, bu yüzden ErrorProvider kullanmıyoruz
                        // Bunun yerine hücreyi görsel olarak işaretleyelim
                        if (cell != null)
                        {
                            cell.Style.BackColor = Color.LightPink;
                            cell.Style.SelectionBackColor = Color.Red;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowErrorProviderForRow hatası: {ex.Message}");
            }
        }

        // ErrorProvider'ı temizle
        private void ClearErrorProviderForRow(int rowIndex)
        {
            try
            {
                if (errorProvider != null && rowIndex >= 0 && rowIndex < dataGridViewXml.Rows.Count)
                {
                    var cell = dataGridViewXml.Rows[rowIndex].Cells["Value"];
                    if (cell != null)
                    {
                        // Hücre stilini temizle
                        if (cell != null)
                        {
                            cell.Style.BackColor = Color.White;
                            cell.Style.SelectionBackColor = Color.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearErrorProviderForRow hatası: {ex.Message}");
            }
        }

        // Tüm ErrorProvider'ları temizle
        private void ClearAllErrorProviders()
        {
            try
            {
                if (errorProvider != null)
                {
                    errorProvider.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearAllErrorProviders hatası: {ex.Message}");
            }
        }

        // Validasyon hatalarını analiz et ve grid'deki alanları işaretle
        private void AnalyzeValidationErrors(List<string> validationErrors)
        {
            try
            {
                Console.WriteLine($" AnalyzeValidationErrors çağrıldı - {validationErrors.Count} hata");
                _validationErrors = validationErrors;

                // Tüm alanları önce geçerli olarak işaretle
                foreach (var row in _treeGridData)
                {
                    row.IsValid = true;
                    row.ValidationError = "";
                    row.IsMissing = false;
                }

                Console.WriteLine($" {_treeGridData.Count} alan sıfırlandı");

                // Validasyon hatalarını analiz et
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($" Validasyon hatası analiz ediliyor: {error}");

                    // Eksik element hatalarını kontrol et
                    if (error.Contains("AppHdr") || error.Contains("AppHdr elementi bulunamadı"))
                    {
                        Console.WriteLine($"  -> AppHdr hatası bulundu, Header alanları işaretleniyor");
                        // AppHdr elementi eksik - Header alanlarını işaretle
                        MarkHeaderFieldsAsInvalid("AppHdr elementi bulunamadı");
                    }

                    // Diğer validasyon hatalarını da kontrol et
                    if (error.Contains("elementi bulunamadı"))
                    {
                        // Genel eksik element hatası
                        var elementName = ExtractElementNameFromError(error);
                        if (!string.IsNullOrEmpty(elementName))
                        {
                            Console.WriteLine($"  -> Eksik element: {elementName}");
                            MarkElementAsInvalid(elementName, error);
                        }
                    }

                    // Pattern, length, range gibi diğer validasyon hatalarını da kontrol et
                    if (error.Contains("pattern") || error.Contains("length") || error.Contains("range"))
                    {
                        // Değer validasyon hatası - ilgili alanı bul ve işaretle
                        MarkValueValidationError(error);
                    }
                }

                Console.WriteLine($" Grid yenileniyor...");
                // Grid'i yenile
                RefreshValidationColors();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" AnalyzeValidationErrors hatası: {ex.Message}");
            }
        }

        // Header alanlarını geçersiz olarak işaretle
        private void MarkHeaderFieldsAsInvalid(string errorMessage)
        {
            Console.WriteLine($" MarkHeaderFieldsAsInvalid çağrıldı: {errorMessage}");

            foreach (var row in _treeGridData)
            {
                // Header seviyesi ve Header altındaki tüm alanları işaretle
                if (row.Level == 1 && row.FieldName == "Header")
                {
                    Console.WriteLine($"  -> Header elementi işaretleniyor: {row.FieldName}");
                    row.IsValid = false;
                    row.ValidationError = errorMessage;
                    row.IsMissing = true;

                    // Header altındaki tüm alanları da işaretle
                    MarkChildrenAsInvalid(row, errorMessage);
                }
                // AppHdr elementi eksik olduğunda, Header ile ilgili diğer alanları da işaretle
                else if (row.FieldName == "Sender" || row.FieldName == "Receiver" || row.FieldName == "MessageType")
                {
                    Console.WriteLine($"  -> Header ile ilgili alan işaretleniyor: {row.FieldName}");
                    row.IsValid = false;
                    row.ValidationError = errorMessage;
                    row.IsMissing = false; // Eksik değil, sadece invalid
                }
            }
        }

        // Alt elementleri recursive olarak invalid olarak işaretle
        private void MarkChildrenAsInvalid(TreeGridRow parent, string errorMessage)
        {
            foreach (var child in parent.Children)
            {
                Console.WriteLine($"    -> Alt element işaretleniyor: {child.FieldName}");
                child.IsValid = false;
                child.ValidationError = errorMessage;
                child.IsMissing = true;

                // Recursive olarak alt elementleri de işaretle
                MarkChildrenAsInvalid(child, errorMessage);
            }
        }

        // Belirli bir elementi geçersiz olarak işaretle
        private void MarkElementAsInvalid(string elementName, string errorMessage)
        {
            foreach (var row in _treeGridData)
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
        private void MarkValueValidationError(string errorMessage)
        {
            // Hata mesajından alan adını çıkarmaya çalış
            var fieldName = ExtractFieldNameFromError(errorMessage);
            if (!string.IsNullOrEmpty(fieldName))
            {
                foreach (var row in _treeGridData)
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
        private string ExtractElementNameFromError(string error)
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
        private string ExtractFieldNameFromError(string error)
        {
            // Pattern, length, range hatalarından alan adını çıkarmaya çalış
            // Bu kısım XSD validasyon mesajlarına göre özelleştirilebilir
            return "";
        }

        // Validasyon özeti göster
        private void ShowValidationSummary()
        {
            DataGridOperations.ShowValidationSummary(_treeGridData);
        }





        private int GetVisibleRowIndex(int displayRowIndex)
        {
            return DataGridOperations.GetVisibleRowIndex(displayRowIndex, _treeGridData);
        }

        private XsdSchemaAnalyzer.SchemaConstraints? FindConstraintsByElementName(string elementName)
        {
            try
            {
                if (_currentXmlDoc == null) return null;

                // XML'de bu isimdeki tüm elementleri bul
                var elements = _currentXmlDoc.Descendants(elementName);
                foreach (var element in elements)
                {
                    // Element'in XPath'ini oluştur
                    var path = XmlOperations.BuildXPathForElement(element);

                    // Önce Document XSD'de ara
                    if (schemaAnalyzer != null)
                    {
                        var constraints = schemaAnalyzer.GetConstraintsByPath(path);
                        if (constraints?.EnumerationValues != null && constraints.EnumerationValues.Count > 0)
                        {
                            Console.WriteLine($"Document XSD'den enum bulundu: {elementName} için {constraints.EnumerationValues.Count} değer");
                            return constraints;
                        }
                    }

                    // Eğer Document XSD'de bulunamazsa, Header XSD'de ara
                    if (headerSchemaAnalyzer != null)
                    {
                        var headerConstraints = headerSchemaAnalyzer.GetConstraintsByPath(path);
                        if (headerConstraints?.EnumerationValues != null && headerConstraints.EnumerationValues.Count > 0)
                        {
                            Console.WriteLine($"Header XSD'den enum bulundu: {elementName} için {headerConstraints.EnumerationValues.Count} değer");
                            return headerConstraints;
                        }

                        // Element adı ile de dene (Header XSD için)
                        var headerConstraintsByName = headerSchemaAnalyzer.GetConstraintsByPath(elementName);
                        if (headerConstraintsByName?.EnumerationValues != null && headerConstraintsByName.EnumerationValues.Count > 0)
                        {
                            Console.WriteLine($"Header XSD'den (element adıyla) enum bulundu: {elementName} için {headerConstraintsByName.EnumerationValues.Count} değer");
                            return headerConstraintsByName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FindConstraintsByElementName hatası: {ex.Message}");
            }
            return null;
        }

        private XsdSchemaAnalyzer.SchemaConstraints? FindConstraintsByElementPath(string elementPath)
        {
            try
            {
                Console.WriteLine($"Enum aranıyor, path: {elementPath}");

                // Önce Document XSD'de ara
                if (schemaAnalyzer != null)
                {
                    var constraints = schemaAnalyzer.GetConstraintsByPath(elementPath);
                    if (constraints?.EnumerationValues != null && constraints.EnumerationValues.Count > 0)
                    {
                        Console.WriteLine($"Document XSD'den enum bulundu: {elementPath} için {constraints.EnumerationValues.Count} değer");
                        return constraints;
                    }
                }

                // Eğer Document XSD'de bulunamazsa, Header XSD'de ara
                if (headerSchemaAnalyzer != null)
                {
                    var headerConstraints = headerSchemaAnalyzer.GetConstraintsByPath(elementPath);
                    if (headerConstraints?.EnumerationValues != null && headerConstraints.EnumerationValues.Count > 0)
                    {
                        Console.WriteLine($"Header XSD'den enum bulundu: {elementPath} için {headerConstraints.EnumerationValues.Count} değer");
                        return headerConstraints;
                    }
                }

                Console.WriteLine($"Hiçbir XSD'de enum bulunamadı: {elementPath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FindConstraintsByElementPath hatası: {ex.Message}");
                return null;
            }
        }

        private string BuildXPathForElement(XElement element)
        {
            return XmlOperations.BuildXPathForElement(element);
        }







        private void DataGridViewXml_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewXml.Columns["Value"].Index && e.RowIndex >= 0)
            {
                // Element değeri değişti, XML'i güncelle
                UpdateXmlFromGrid(e.RowIndex);

                // Gerçek zamanlı validasyon yap
                var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
                if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                {
                    var treeRow = _treeGridData[visibleRowIndex];
                    var newValue = dataGridViewXml.Rows[e.RowIndex].Cells["Value"].Value?.ToString() ?? "";

                    // XSD kısıtlamalarına göre validasyon yap
                    var validationResult = ValidateFieldWithXsdConstraints(treeRow, newValue);

                    // TreeGrid'de validasyon durumunu güncelle
                    treeRow.IsValid = validationResult.IsValid;
                    treeRow.ValidationError = validationResult.ErrorMessage;

                                    // Validasyon renklerini yenile
                RefreshValidationColors();
                
                // ErrorProvider'ı güncelle
                if (!validationResult.IsValid)
                {
                    ShowErrorProviderForRow(e.RowIndex, validationResult.ErrorMessage);
                    // Sadece hata varsa tooltip ile göster
                    ShowValidationTooltip(e.RowIndex, e.ColumnIndex, validationResult);
                }
                else
                {
                    ClearErrorProviderForRow(e.RowIndex);
                }
                }
            }
        }

        private void DataGridViewXml_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewXml.Columns["Value"].Index && e.RowIndex >= 0)
            {
                var visibleRowIndex = GetVisibleRowIndex(e.RowIndex);
                if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                {
                    var treeRow = _treeGridData[visibleRowIndex];
                    var newValue = e.FormattedValue?.ToString() ?? "";

                    // XSD kısıtlamalarına göre detaylı validasyon yap
                    var validationResult = ValidateFieldWithXsdConstraints(treeRow, newValue);

                    if (!validationResult.IsValid)
                    {
                        e.Cancel = true;

                        // Sadece hata mesajını göster
                        var errorMessage = validationResult.ErrorMessage;

                        MessageBox.Show($"'{treeRow.FieldName}' alanı için geçersiz değer:\n{errorMessage}",
                            "Validasyon Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // TreeGrid'de validasyon durumunu güncelle
                        treeRow.IsValid = false;
                        treeRow.ValidationError = validationResult.ErrorMessage;

                        // Grid'i yenile
                        RefreshValidationColors();
                        return;
                    }

                    // Geçerli değer - TreeGrid'de validasyon durumunu güncelle
                    treeRow.IsValid = true;
                    treeRow.ValidationError = "";
                }
            }
        }

        // Validasyon tooltip'i göster - Sadece hata mesajları
        private void ShowValidationTooltip(int rowIndex, int columnIndex, ValidationResult validationResult)
        {
            try
            {
                var cellRect = dataGridViewXml.GetCellDisplayRectangle(columnIndex, rowIndex, false);
                var tooltipText = $"❌ {validationResult.ErrorMessage}";

                tooltipText += $"\n\n{validationResult.ValidationType}";

                var toolTip = new ToolTip();
                toolTip.Show(tooltipText, dataGridViewXml, cellRect.Left, cellRect.Top - 20, 5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowValidationTooltip hatası: {ex.Message}");
            }
        }

        private void UpdateXmlFromGrid(int displayRowIndex)
        {
            try
            {
                Console.WriteLine($"UpdateXmlFromGrid çağrıldı: displayRowIndex={displayRowIndex}");

                if (displayRowIndex >= 0 && displayRowIndex < dataGridViewXml.Rows.Count && _currentXmlDoc != null)
                {
                    // Görünür satırlar arasından doğru TreeGrid satırını bul
                    var visibleRowIndex = GetVisibleRowIndex(displayRowIndex);
                    Console.WriteLine($"GetVisibleRowIndex sonucu: {visibleRowIndex}");

                    if (visibleRowIndex >= 0 && visibleRowIndex < _treeGridData.Count)
                    {
                        var treeRow = _treeGridData[visibleRowIndex];
                        var newValue = dataGridViewXml.Rows[displayRowIndex].Cells["Value"].Value?.ToString();

                        Console.WriteLine($"Güncellenecek satır: {treeRow.FieldName}, Eski değer: '{treeRow.Value}', Yeni değer: '{newValue}'");

                        if (!string.IsNullOrEmpty(treeRow.FieldName))
                        {
                            // Element değerini TreeGrid'de güncelle
                            treeRow.Value = newValue ?? "";

                            // XML'de de güncelle
                            UpdateXmlElementByName(treeRow.FieldName, newValue);

                            Console.WriteLine($"XML güncellendi: {treeRow.FieldName} = '{newValue}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Geçersiz visibleRowIndex: {visibleRowIndex}");
                    }
                }
                else
                {
                    Console.WriteLine($"Geçersiz parametreler: displayRowIndex={displayRowIndex}, Rows.Count={dataGridViewXml.Rows.Count}, _currentXmlDoc={_currentXmlDoc != null}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateXmlFromGrid hatası: {ex.Message}");
                MessageBox.Show($"XML güncellenirken hata oluştu: {ex.Message}");
            }
        }

        private void UpdateXmlElementByName(string elementName, string newValue)
        {
            XmlOperations.UpdateXmlElementByName(_currentXmlDoc, elementName, newValue);
        }

        // Daha spesifik güncelleme için Path kullanarak elementi bul ve güncelle
        private void UpdateXmlElementByPath(string elementPath, string newValue)
        {
            XmlOperations.UpdateXmlElementByPath(_currentXmlDoc, elementPath, newValue);
        }

        // Path'den element adını çıkar
        private string ExtractElementNameFromPath(string elementPath)
        {
            return XmlOperations.ExtractElementNameFromPath(elementPath);
        }







        private void LoadXmlContentToGrid()
        {
            // Tüm ErrorProvider'ları temizle
            ClearAllErrorProviders();
            
            FormEvents.LoadXmlContentToGrid(_currentXmlDoc, dataGridViewXml, _treeGridData, txtXsdPacsPath.Text, txtXsdHeadPath.Text);
        }

        private TreeGridRow LoadXmlToTreeGrid(XElement element, int level, string parentPath)
        {
            return FormEvents.LoadXmlToTreeGrid(element, level, parentPath, _treeGridData);
        }

        private void RefreshTreeGridDisplay()
        {
            DataGridOperations.RefreshTreeGridDisplay(dataGridViewXml, _treeGridData, schemaAnalyzer, headerSchemaAnalyzer);
            
            // Tüm ErrorProvider'ları temizle
            ClearAllErrorProviders();
            
            // Validasyon renklerini yenile
            RefreshValidationColors();
        }



        private bool IsRowVisible(TreeGridRow row)
        {
            return DataGridOperations.IsRowVisible(row, _treeGridData);
        }

        // Bir elementin belirli bir parent'a ait olup olmadığını kontrol et
        private bool IsChildOfParent(TreeGridRow child, TreeGridRow potentialParent)
        {
            return DataGridOperations.IsChildOfParent(child, potentialParent);
        }

        private void ToggleRowExpansion(int displayRowIndex)
        {
            DataGridOperations.ToggleRowExpansion(displayRowIndex, _treeGridData, dataGridViewXml);
        }

        private int GetNewVisibleRowIndex(int oldTreeGridIndex)
        {
            return DataGridOperations.GetNewVisibleRowIndex(oldTreeGridIndex, _treeGridData);
        }

        private void ExpandAllRows()
        {
            DataGridOperations.ExpandAllRows(_treeGridData);
            RefreshTreeGridDisplay();
        }

        private void CollapseAllRows()
        {
            DataGridOperations.CollapseAllRows(_treeGridData);
            RefreshTreeGridDisplay();
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        // element adını değiştir 
        private static string SanitizeName(string input)
        {
            return XmlOperations.SanitizeName(input);
        }


        // 
        private static string BuildXPathWithIndex(XElement element)
        {
            return XmlOperations.BuildXPathWithIndex(element);
        }

        // Hücre düzenleme kontrolü gösterildiğinde
        private void DataGridViewXml_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                if (dataGridViewXml.CurrentCell == null) return;
                if (dataGridViewXml.CurrentCell.OwningColumn.Name != "Value") return;

                var displayRowIndex = dataGridViewXml.CurrentCell.RowIndex;
                var treeIndex = GetVisibleRowIndex(displayRowIndex);
                if (treeIndex < 0 || treeIndex >= _treeGridData.Count) return;

                var row = _treeGridData[treeIndex];

                // TextBox için özel stil ve davranış (çoğu alan için)
                if (e.Control is TextBox textBox)
                {
                    // TextBox stilini ayarla
                    textBox.Font = new Font("Arial", 9, FontStyle.Regular);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Color.White;
                    textBox.ForeColor = Color.Black;

                    // Validasyon durumuna göre arka plan rengini ayarla
                    if (row.IsMissing || !row.IsValid)
                    {
                        textBox.BackColor = Color.FromArgb(255, 255, 200, 200); // Açık kırmızı
                        textBox.ForeColor = Color.DarkRed; // Yazı koyu kırmızı
                    }
                    else
                    {
                        textBox.BackColor = Color.White;
                        textBox.ForeColor = Color.Black;
                    }

                    // TextBox boyutunu ayarla
                    textBox.Width = 650; // Hücre genişliğine uygun
                    textBox.Height = 50; // Hücre yüksekliğine uygun

                    // TextBox'ta değer değiştiğinde validasyon yap
                    textBox.TextChanged -= OnTextBoxTextChanged;
                    textBox.TextChanged += OnTextBoxTextChanged;
                }

                // ComboBox için özel stil (sadece enum alanlar için)
                else if (e.Control is ComboBox comboBox)
                {
                    comboBox.Font = new Font("Arial", 9, FontStyle.Regular);
                    comboBox.BackColor = Color.White;
                    comboBox.ForeColor = Color.Black;
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                    // Validasyon durumuna göre arka plan rengini ayarla
                    if (row.IsMissing || !row.IsValid)
                    {
                        comboBox.BackColor = Color.FromArgb(255, 255, 200, 200); // Açık kırmızı
                        comboBox.ForeColor = Color.DarkRed; // Yazı koyu kırmızı
                    }
                    else
                    {
                        comboBox.BackColor = Color.White;
                        comboBox.ForeColor = Color.Black;
                    }

                    // ComboBox boyutunu ayarla
                    comboBox.Width = 650; // Hücre genişliğine uygun
                    comboBox.Height = 50; // Hücre yüksekliğine uygun
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditingControlShowing error: {ex.Message}");
            }
        }

        // TextBox'ta değer değiştiğinde validasyon yap
        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewXml.CurrentCell == null) return;
                if (dataGridViewXml.CurrentCell.OwningColumn.Name != "Value") return;

                var textBox = sender as TextBox;
                if (textBox == null) return;

                var displayRowIndex = dataGridViewXml.CurrentCell.RowIndex;
                var treeIndex = GetVisibleRowIndex(displayRowIndex);
                if (treeIndex < 0 || treeIndex >= _treeGridData.Count) return;

                var row = _treeGridData[treeIndex];
                var newValue = textBox.Text;

                // Gerçek zamanlı validasyon
                bool isValid = IsFieldValueValid(row, newValue);

                // Arka plan rengini güncelle
                if (row.IsMissing || !row.IsValid || !isValid)
                {
                    textBox.BackColor = Color.FromArgb(255, 255, 200, 200); // Açık kırmızı
                    textBox.ForeColor = Color.DarkRed; // Yazı koyu kırmızı
                }
                else
                {
                    textBox.BackColor = Color.White;
                    textBox.ForeColor = Color.Black;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnTextBoxTextChanged error: {ex.Message}");
            }
        }

    
        private List<string> GetManualEnumValues(string fieldName, string currentValue)
        {
            return DataGridOperations.GetManualEnumValues(fieldName, currentValue);
        }










    }
}


