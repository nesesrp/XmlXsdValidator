using System.Drawing;
using System.Windows.Forms;
using validator.Models;
using validator.Validation;
using validator;

namespace validator.Operations
{
    public static class DataGridOperations
    {
        public static void SetupDataGridView(DataGridView dataGridViewXml)
        {
            dataGridViewXml.Columns.Clear();

           
            dataGridViewXml.Columns.Add("FieldName", ""); 

            // Value sütunu (normal TextBox olarak)
            dataGridViewXml.Columns.Add("Value", "");

            // Tooltip sistemi ekle
            var toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            // Validasyon tooltip'i ekle
            toolTip.SetToolTip(dataGridViewXml,
                "XML Editör\n\n" +
                "• Sağ tık: Menü\n" +
                "• Ctrl+V: Validasyon özeti\n" +
                "• Geçersiz alanlar pembe renkte gösterilir\n" +
                "• Ctrl+Plus: Tümünü genişlet\n" +
                "• Ctrl+Minus: Tümünü daralt");

            // Sütun genişlikleri
            dataGridViewXml.Columns["FieldName"].Width = 700; 
            dataGridViewXml.Columns["Value"].Width = 700;  

            // Field Name sütununu salt okunur yap
            dataGridViewXml.Columns["FieldName"].ReadOnly = true;

          
            dataGridViewXml.Columns["Value"].ReadOnly = false;
            dataGridViewXml.Columns["Value"].DefaultCellStyle.SelectionBackColor = Color.LightGray;
            dataGridViewXml.Columns["Value"].DefaultCellStyle.SelectionForeColor = Color.Black;

            // Grid stili
            dataGridViewXml.AllowUserToAddRows = false;
            dataGridViewXml.AllowUserToDeleteRows = false;
            dataGridViewXml.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewXml.RowHeadersVisible = false;
            dataGridViewXml.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridViewXml.RowTemplate.Height = 65; 

          
            dataGridViewXml.DefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Regular);
            dataGridViewXml.DefaultCellStyle.BackColor = Color.White;
            dataGridViewXml.BackgroundColor = Color.White;

          
            dataGridViewXml.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            dataGridViewXml.DefaultCellStyle.SelectionForeColor = Color.Black;

           
            dataGridViewXml.EnableHeadersVisualStyles = false;
            dataGridViewXml.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Regular); 
            dataGridViewXml.ColumnHeadersDefaultCellStyle.BackColor = Color.White; 
            dataGridViewXml.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewXml.ColumnHeadersHeight = 50;
            dataGridViewXml.ColumnHeadersVisible = false; 

            // Hücre padding
            dataGridViewXml.DefaultCellStyle.Padding = new Padding(16, 14, 16, 14); 

            // Field Name sütunu için özel ayarlar
            dataGridViewXml.Columns["FieldName"].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold); 
            dataGridViewXml.Columns["FieldName"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewXml.Columns["FieldName"].DefaultCellStyle.Padding = new Padding(20, 16, 20, 16); 

            // Value sütunu için özel ayarlar
            dataGridViewXml.Columns["Value"].DefaultCellStyle.Padding = new Padding(18, 14, 18, 14); 
            dataGridViewXml.Columns["Value"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewXml.Columns["Value"].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular); 

            // TextBox'ların düzenlenmesi için EditMode'u ayarla
            dataGridViewXml.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        public static void RefreshTreeGridDisplay(DataGridView dataGridViewXml, List<TreeGridRow> treeGridData, XsdSchemaAnalyzer schemaAnalyzer = null, XsdSchemaAnalyzer headerSchemaAnalyzer = null)
        {
            try
            {
            
                LoadXsdConstraintsToTreeGrid(treeGridData, schemaAnalyzer, headerSchemaAnalyzer);
                
                dataGridViewXml.Rows.Clear();

                int visibleCount = 0;
                foreach (var row in treeGridData)
                {
                    if (IsRowVisible(row, treeGridData))
                    {
                        // Yeni yapıya uygun satır ekle
                        dataGridViewXml.Rows.Add(
                            "", 
                            row.Value ?? "" // Value
                        );

                        // Her alan için uygun kontrol ekle (Header olmayan tüm alanlar)
                        Console.WriteLine($"Satır {visibleCount}: {row.FieldName} - DataType: {row.DataType}, EnumValues: {row.EnumValues?.Count ?? 0}");

                        if (!row.IsHeader) // Sadece leaf elementler için kontrol ekle
                        {
                            if ((row.DataType == "enum" || row.DataType == "boolean") && row.EnumValues != null && row.EnumValues.Count > 1)
                            {

                                try
                                {
                                    var comboCell = new DataGridViewComboBoxCell();

                                    // Enum değerlerini güvenli şekilde ekle
                                    foreach (var enumValue in row.EnumValues)
                                    {
                                        if (!string.IsNullOrEmpty(enumValue))
                                        {
                                            comboCell.Items.Add(enumValue);
                                        }
                                    }

                                    // Mevcut değeri güvenli şekilde ayarla
                                    if (!string.IsNullOrEmpty(row.Value) && row.EnumValues.Contains(row.Value))
                                    {
                                        comboCell.Value = row.Value;
                                    }
                                    else if (row.EnumValues.Count > 0)
                                    {
                                        // Mevcut değer geçerli değilse ilk enum değerini kullan
                                        comboCell.Value = row.EnumValues[0];
                                        row.Value = row.EnumValues[0]; // TreeGrid'i de güncelle
                                    }

                                    comboCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton; // Dropdown okunu göster
                                    dataGridViewXml.Rows[visibleCount].Cells["Value"] = comboCell;
                                    Console.WriteLine($"  -> {row.DataType.ToUpper()} ComboBox: {row.FieldName} = '{comboCell.Value}' ({row.EnumValues.Count} değer) - {string.Join(", ", row.EnumValues)}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"ComboBox oluşturma hatası: {ex.Message}, TextBox kullanılıyor");
                                 
                                    var textCell = new DataGridViewTextBoxCell();
                                    textCell.Value = row.Value ?? "";
                                    dataGridViewXml.Rows[visibleCount].Cells["Value"] = textCell;
                                }
                            }
                            else
                            {
                              
                                var textCell = new DataGridViewTextBoxCell();
                                textCell.Value = row.Value ?? "";
                                // TextBox hücreleri varsayılan olarak sürekli görünür
                                dataGridViewXml.Rows[visibleCount].Cells["Value"] = textCell;
                                Console.WriteLine($"  TextBox: {row.FieldName} = '{textCell.Value}' (Tip: {row.DataType}) - Sürekli görünür");
                            }
                        }
                        else
                        {
                            // Header elementler için TextBox
                            var headerCell = new DataGridViewTextBoxCell();
                            headerCell.Value = row.Value ?? "";
                      
                            dataGridViewXml.Rows[visibleCount].Cells["Value"] = headerCell;
                      
                            dataGridViewXml.Rows[visibleCount].Cells["Value"].ReadOnly = true;
                            Console.WriteLine($"  -> HEADER TextBox (ReadOnly): {row.FieldName} - Sürekli görünür");
                        }

                        visibleCount++;
                    }
                }

                Console.WriteLine($"TreeGrid refreshed: {visibleCount} visible rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TreeGrid refresh error: {ex.Message}");
                MessageBox.Show($"TreeGrid yenileme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool IsRowVisible(TreeGridRow row, List<TreeGridRow> treeGridData)
        {
            try
            {
               
                if (row.Level == 0) return true;

                // Üst elementlerin hepsi expanded olmalı
                var current = row;
                var parentLevel = row.Level - 1;

                // Geriye doğru git ve üst elementleri kontrol et
                for (int i = treeGridData.Count - 1; i >= 0; i--)
                {
                    var potentialParent = treeGridData[i];
                    if (potentialParent.Level == parentLevel)
                    {
                        // Parent-child ilişkisini FieldName ve Level ile kontrol et
                        // Aynı seviyede ve aynı parent'a ait olan elementleri bul
                        if (IsChildOfParent(current, potentialParent))
                        {
                            return potentialParent.IsExpanded && IsRowVisible(potentialParent, treeGridData);
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsRowVisible error: {ex.Message}");
                return false; // Hata durumunda gizli olarak kabul et
            }
        }

        // Bir elementin belirli bir parent'a ait olup olmadığını kontrol et
        public static bool IsChildOfParent(TreeGridRow child, TreeGridRow potentialParent)
        {
            try
            {
                // Level kontrolü
                if (child.Level != potentialParent.Level + 1) return false;

                // Path kontrolü - child'ın path'i parent'ın path'ini içermeli
                if (string.IsNullOrEmpty(child.Path) || string.IsNullOrEmpty(potentialParent.Path)) return false;

                // Child'ın path'i parent'ın path'ini prefix olarak içermeli
                // Örnek: parent path: "Message/Header", child path: "Message/Header/Sender"
                if (child.Path.StartsWith(potentialParent.Path + "/"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsChildOfParent error: {ex.Message}");
                return false;
            }
        }

        public static int GetVisibleRowIndex(int displayRowIndex, List<TreeGridRow> treeGridData)
        {
            try
            {
                int visibleCount = 0;
                for (int i = 0; i < treeGridData.Count; i++)
                {
                    if (IsRowVisible(treeGridData[i], treeGridData))
                    {
                        if (visibleCount == displayRowIndex)
                            return i;
                        visibleCount++;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVisibleRowIndex error: {ex.Message}");
                return -1; // Hata durumunda -1 döndür
            }
        }

        public static void ToggleRowExpansion(int displayRowIndex, List<TreeGridRow> treeGridData, DataGridView dataGridViewXml)
        {
            try
            {
                var visibleRowIndex = GetVisibleRowIndex(displayRowIndex, treeGridData);
                if (visibleRowIndex >= 0 && visibleRowIndex < treeGridData.Count)
                {
                    var treeRow = treeGridData[visibleRowIndex];
                    if (treeRow.Children.Count > 0)
                    {
                        // Mevcut durumu tersine çevir (expand ise collapse, collapse ise expand)
                        treeRow.IsExpanded = !treeRow.IsExpanded;
                        RefreshTreeGridDisplay(dataGridViewXml, treeGridData);

                        string action = treeRow.IsExpanded ? "Expanded" : "Collapsed";
                        Console.WriteLine($"{action} '{treeRow.FieldName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleRowExpansion hatası: {ex.Message}");
            }
        }

        public static void ExpandAllRows(List<TreeGridRow> treeGridData)
        {
            foreach (var row in treeGridData)
            {
                row.IsExpanded = true;
            }
            Console.WriteLine("All rows expanded");
        }

        public static void CollapseAllRows(List<TreeGridRow> treeGridData)
        {
            foreach (var row in treeGridData)
            {
                row.IsExpanded = false;
            }
            Console.WriteLine("All rows collapsed");
        }

        public static void ShowDataTypeSummary(List<TreeGridRow> treeGridData)
        {
            try
            {
                var typeCounts = new Dictionary<string, int>();
                var enumFields = new List<string>();

                foreach (var row in treeGridData)
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

        public static void FilterByDataType(List<TreeGridRow> treeGridData, string dataType)
        {
            try
            {
                foreach (var row in treeGridData)
                {
                    if (row.IsHeader)
                    {
                        // Header elementler için üst elementlerin expanded durumunu koru
                        continue;
                    }

                    // Sadece belirtilen veri tipindeki alanları göster
                    if (row.DataType == dataType)
                    {
                        // Üst elementleri expand et
                        var parent = GetParentRow(treeGridData, row);
                        while (parent != null)
                        {
                            parent.IsExpanded = true;
                            parent = GetParentRow(treeGridData, parent);
                        }
                    }
                }

                // Filtreleme sonrası görünümü güncelle
                // Bu metod çağrıldıktan sonra RefreshTreeGridDisplay çağrılmalı
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FilterByDataType hatası: {ex.Message}");
            }
        }

        private static TreeGridRow GetParentRow(List<TreeGridRow> treeGridData, TreeGridRow childRow)
        {
            foreach (var row in treeGridData)
            {
                if (row.Children.Contains(childRow))
                {
                    return row;
                }
            }
            return null;
        }

        public static void ShowValidationSummary(List<TreeGridRow> treeGridData)
        {
            try
            {
                var invalidFields = new List<string>();
                var missingFields = new List<string>();
                var totalFields = 0;

                foreach (var row in treeGridData)
                {
                    if (!row.IsHeader) // Sadece leaf elementler
                    {
                        totalFields++;

                        // Eksik element kontrolü
                        if (row.IsMissing)
                        {
                            missingFields.Add($"{row.FieldName}: Eksik element - {row.ValidationError}");
                        }
                        // Değer validasyon kontrolü
                        else if (!row.IsValid)
                        {
                            invalidFields.Add($"{row.FieldName}: '{row.Value}' - {row.ValidationError}");
                        }
                    }
                }

                var summary = $"Validasyon Özeti:\n\n";
                summary += $"Toplam Alan: {totalFields}\n";
                summary += $"Geçerli Alan: {totalFields - invalidFields.Count - missingFields.Count}\n";
                summary += $"Geçersiz Alan: {invalidFields.Count}\n";
                summary += $"Eksik Element: {missingFields.Count}\n";

                if (missingFields.Count > 0)
                {
                    summary += $"\nEksik Elementler:\n";
                    summary += string.Join("\n", missingFields);
                }

                if (invalidFields.Count > 0)
                {
                    summary += $"\nGeçersiz Alanlar:\n";
                    summary += string.Join("\n", invalidFields);
                }

                // Resimdeki gibi özet formatı - SADECE HATA SAYISI
                var errorCount = invalidFields.Count + missingFields.Count;
                var summaryLine = $"{errorCount} errors • auto-validated from XSD patterns";

                summary += $"\n\n{summaryLine}";

                if (errorCount > 0)
                {
                    summary += $"\n\nRenk Kodları:\n";
                    summary += $" Kırmızı: Hatalı alanlar\n";
                    summary += $" Beyaz: Geçerli alanlar";
                }
                else
                {
                    summary += $"\n\n Tüm alanlar geçerli!";
                }

                MessageBox.Show(summary, "Validasyon Özeti", MessageBoxButtons.OK,
                    (errorCount > 0) ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowValidationSummary hatası: {ex.Message}");
                MessageBox.Show($"Validasyon özeti gösterilirken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static int GetNewVisibleRowIndex(int oldTreeGridIndex, List<TreeGridRow> treeGridData)
        {
            try
            {
                // Eski satırın yeni görünür indeksini bul
                int visibleCount = 0;
                for (int i = 0; i < treeGridData.Count; i++)
                {
                    if (IsRowVisible(treeGridData[i], treeGridData))
                    {
                        if (i == oldTreeGridIndex)
                            return visibleCount;
                        visibleCount++;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNewVisibleRowIndex error: {ex.Message}");
                return -1;
            }
        }

        
        public static List<string> GetManualEnumValues(string fieldName, string currentValue)
        {
            try
            {
                var enumValues = new List<string>();

               
                switch (fieldName.ToLower())
                {
                    case "format":
                        // Format enum değerleri - sadece XSD'de tanımlı değilse
                        enumValues.AddRange(new[] { "MX", "MT", "FIN", "GPA", "ISO", "SWIFT", "XML", "JSON" });
                        break;

                    case "currency":
                        // Currency enum değerleri - sadece XSD'de tanımlı değilse
                        enumValues.AddRange(new[] { "TRY", "USD", "EUR", "GBP", "JPY", "CHF" });
                        break;

                    case "messagetype":
                        // MessageType enum değerleri - SWIFT/ISO20022 standart mesaj tipleri
                        enumValues.AddRange(new[] { "PACS.008", "PACS.009", "PACS.002", "CAMT.054", "PAIN.001" });
                        break;

                    // Sadece XSD'den gelen gerçek enum değerleri kullanılır

                    default:
                        // Eğer mevcut değer varsa, onu da ekle
                        if (!string.IsNullOrEmpty(currentValue) && !enumValues.Contains(currentValue))
                        {
                            enumValues.Add(currentValue);
                        }
                        break;
                }

                // Mevcut değer varsa ve listede yoksa ekle
                if (!string.IsNullOrEmpty(currentValue) && !enumValues.Contains(currentValue))
                {
                    enumValues.Insert(0, currentValue); // Mevcut değeri başa ekle
                }

                return enumValues.Count > 0 ? enumValues : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetManualEnumValues hatası: {ex.Message}");
                return null;
            }
        }

        // Load XSD constraints into TreeGridRow objects to populate EnumValues
        public static void LoadXsdConstraintsToTreeGrid(List<TreeGridRow> treeGridData, XsdSchemaAnalyzer schemaAnalyzer = null, XsdSchemaAnalyzer headerSchemaAnalyzer = null)
        {
            try
            {
                Console.WriteLine("Loading XSD constraints to TreeGrid...");
                
                foreach (var row in treeGridData)
                {
                    if (!row.IsHeader)
                    {
                       
                        LoadConstraintsForRow(row, schemaAnalyzer, headerSchemaAnalyzer);
                    }
                }
                
                Console.WriteLine("XSD constraints loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading XSD constraints: {ex.Message}");
            }
        }

        private static void LoadConstraintsForRow(TreeGridRow row, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer)
        {
            try
            {
                
                if (schemaAnalyzer != null || headerSchemaAnalyzer != null)
                {
                   
                    if (!string.IsNullOrEmpty(row.Path))
                    {
                        var constraints = GetConstraintsByPath(row.Path, schemaAnalyzer, headerSchemaAnalyzer);
                        if (constraints != null && constraints.EnumerationValues != null && constraints.EnumerationValues.Count > 0)
                        {
                            row.EnumValues = constraints.EnumerationValues;
                            row.DataType = "enum";
                            Console.WriteLine($"XSD enum values loaded for {row.FieldName}: {string.Join(", ", constraints.EnumerationValues)}");
                            return;
                        }
                    }

                    // Try to get constraints by element name
                    var constraintsByName = GetConstraintsByName(row.FieldName, schemaAnalyzer, headerSchemaAnalyzer);
                    if (constraintsByName != null && constraintsByName.EnumerationValues != null && constraintsByName.EnumerationValues.Count > 0)
                    {
                        row.EnumValues = constraintsByName.EnumerationValues;
                        row.DataType = "enum";
                        Console.WriteLine($"XSD enum values loaded for {row.FieldName} (by name): {string.Join(", ", constraintsByName.EnumerationValues)}");
                        return;
                    }
                }

                if (row.EnumValues == null || row.EnumValues.Count == 0)
                {
                    var manualValues = GetManualEnumValues(row.FieldName, row.Value);
                    if (manualValues != null)
                    {
                        row.EnumValues = manualValues;
                        row.DataType = "enum";
                        Console.WriteLine($"Manual enum values loaded for {row.FieldName}: {string.Join(", ", manualValues)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading constraints for row {row.FieldName}: {ex.Message}");
            }
        }

        private static XsdSchemaAnalyzer.SchemaConstraints GetConstraintsByPath(string path, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer)
        {
            try
            {
                // Try Document XSD first
                if (schemaAnalyzer != null)
                {
                    var constraints = schemaAnalyzer.GetConstraintsByPath(path);
                    if (constraints?.EnumerationValues != null && constraints.EnumerationValues.Count > 0)
                    {
                        return constraints;
                    }
                }

                // Try Header XSD if Document XSD doesn't have constraints
                if (headerSchemaAnalyzer != null)
                {
                    var headerConstraints = headerSchemaAnalyzer.GetConstraintsByPath(path);
                    if (headerConstraints?.EnumerationValues != null && headerConstraints.EnumerationValues.Count > 0)
                    {
                        return headerConstraints;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting constraints by path {path}: {ex.Message}");
                return null;
            }
        }

        private static XsdSchemaAnalyzer.SchemaConstraints GetConstraintsByName(string elementName, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer)
        {
            try
            {
                
                if (schemaAnalyzer != null)
                {
                    var constraints = schemaAnalyzer.GetConstraintsByPath(elementName);
                    if (constraints?.EnumerationValues != null && constraints.EnumerationValues.Count > 0)
                    {
                        return constraints;
                    }
                }

              
                if (headerSchemaAnalyzer != null)
                {
                    var headerConstraints = headerSchemaAnalyzer.GetConstraintsByPath(elementName);
                    if (headerConstraints?.EnumerationValues != null && headerConstraints.EnumerationValues.Count > 0)
                    {
                        return headerConstraints;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting constraints by name {elementName}: {ex.Message}");
                return null;
            }
        }
    }
}
