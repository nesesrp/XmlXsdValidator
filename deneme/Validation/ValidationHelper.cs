using System.Text.RegularExpressions;
using validator.Models;

namespace validator.Validation
{
    public static class ValidationHelper
    {
        // Alan değerinin geçerli olup olmadığını kontrol eden metod - XSD pattern'ları ile
        public static bool IsFieldValueValid(TreeGridRow treeRow, string value, XsdSchemaAnalyzer schemaAnalyzer)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    // Boş değer her zaman geçerli (zorunlu alan kontrolü yapılmıyor)
                    return true;
                }

                // Veri tipine göre validasyon
                switch (treeRow.DataType)
                {
                    case "boolean":
                        var validBooleans = new[] { "true", "false", "yes", "no" };
                        return validBooleans.Contains(value.ToLower());

                    case "number":
                        return decimal.TryParse(value, out _) ||
                               int.TryParse(value, out _) ||
                               double.TryParse(value, out _);

                    case "enum":
                        if (treeRow.EnumValues != null && treeRow.EnumValues.Count > 0)
                        {
                            return treeRow.EnumValues.Contains(value);
                        }
                        return true; // Enum değerleri yoksa her değer geçerli

                    case "string":
                        // String için XSD pattern validasyonu
                        if (schemaAnalyzer != null && !string.IsNullOrEmpty(treeRow.Path))
                        {
                            var constraints = schemaAnalyzer.GetConstraintsByPath(treeRow.Path);
                            if (constraints != null)
                            {
                                // Pattern (regex) validasyonu
                                if (!string.IsNullOrEmpty(constraints.Pattern))
                                {
                                    try
                                    {
                                        var regex = new Regex(constraints.Pattern);
                                        if (!regex.IsMatch(value))
                                        {
                                            treeRow.ValidationError = $"Invalid format - Pattern: {constraints.Pattern}";
                                            return false;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Regex hatası: {ex.Message}");
                                        treeRow.ValidationError = $"Regex error: {constraints.Pattern}";
                                        return false;
                                    }
                                }

                                // Length validasyonu
                                if (constraints.MinLength.HasValue && value.Length < constraints.MinLength.Value)
                                {
                                    treeRow.ValidationError = $"Min {constraints.MinLength} characters required";
                                    return false;
                                }

                                if (constraints.MaxLength.HasValue && value.Length > constraints.MaxLength.Value)
                                {
                                    treeRow.ValidationError = $"Max {constraints.MaxLength} characters allowed";
                                    return false;
                                }
                            }
                        }
                        return true;

                    case "date":
                        // Tarih formatı validasyonu
                        if (!DateTime.TryParse(value, out _))
                        {
                            treeRow.ValidationError = "Invalid date format";
                            return false;
                        }
                        return true;

                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsFieldValueValid hatası: {ex.Message}");
                treeRow.ValidationError = $"Validation error: {ex.Message}";
                return false;
            }
        }

        // XSD kısıtlamalarına göre detaylı validasyon yapan metod - Sadece pattern kontrolü
        public static ValidationResult ValidateFieldWithXsdConstraints(TreeGridRow treeRow, string value, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer)
        {
            var result = new ValidationResult
            {
                IsValid = true,
                ErrorMessage = "",
                WarningMessage = "",
                ValidationType = "auto-validated from XSD patterns"
            };

            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return result; // Boş değer geçerli
                }

                // XSD kısıtlamalarını al
                var constraints = GetFieldConstraints(treeRow, schemaAnalyzer, headerSchemaAnalyzer);
                if (constraints == null)
                {
                    return result; // Kısıtlama yoksa geçerli
                }

                // SADECE Pattern (regex) validasyonu - resimdeki gibi
                if (!string.IsNullOrEmpty(constraints.Pattern))
                {
                    try
                    {
                        var regex = new Regex(constraints.Pattern);
                        if (!regex.IsMatch(value))
                        {
                            result.IsValid = false;
                            result.ErrorMessage = $"Invalid format - Pattern: {constraints.Pattern}";
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Regex error: {constraints.Pattern}";
                        return result;
                    }
                }

                // Length kısıtlamaları - sadece hata olarak
                if (constraints.MinLength.HasValue && value.Length < constraints.MinLength.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Min {constraints.MinLength} characters required";
                    return result;
                }

                if (constraints.MaxLength.HasValue && value.Length > constraints.MaxLength.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Max {constraints.MaxLength} characters allowed";
                    return result;
                }

                // Sayısal kısıtlamalar - sadece hata olarak
                if (treeRow.DataType == "number" && decimal.TryParse(value, out decimal numValue))
                {
                    if (constraints.MinInclusive.HasValue && numValue < constraints.MinInclusive.Value)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Value must be ≥ {constraints.MinInclusive}";
                        return result;
                    }

                    if (constraints.MaxInclusive.HasValue && numValue > constraints.MaxInclusive.Value)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Value must be ≤ {constraints.MaxInclusive}";
                        return result;
                    }

                    if (constraints.MinExclusive.HasValue && numValue <= constraints.MinExclusive.Value)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Value must be > {constraints.MinExclusive}";
                        return result;
                    }

                    if (constraints.MaxExclusive.HasValue && numValue >= constraints.MaxExclusive.Value)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Value must be < {constraints.MaxExclusive}";
                        return result;
                    }

                    // Decimal hassasiyet kontrolü - sadece hata olarak
                    if (constraints.FractionDigits.HasValue)
                    {
                        var decimalPlaces = GetDecimalPlaces(value);
                        if (decimalPlaces > constraints.FractionDigits.Value)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = $"≤ {constraints.FractionDigits} decimals allowed";
                            return result;
                        }
                    }

                    // Toplam basamak kontrolü - sadece hata olarak
                    if (constraints.TotalDigits.HasValue)
                    {
                        var totalDigits = GetTotalDigits(value);
                        if (totalDigits > constraints.TotalDigits.Value)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = $"Max {constraints.TotalDigits} total digits";
                            return result;
                        }
                    }
                }

                // Tarih formatı kontrolü - sadece hata olarak
                if (treeRow.DataType == "date" || constraints?.DataType == "date")
                {
                    if (!IsValidDateFormat(value))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Use YYYY-MM-DD format";
                        return result;
                    }
                }

                // Enum validasyonu - sadece hata olarak
                if (treeRow.DataType == "enum" && treeRow.EnumValues != null && treeRow.EnumValues.Count > 0)
                {
                    if (!treeRow.EnumValues.Contains(value))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Must be one of: {string.Join(", ", treeRow.EnumValues)}";
                        return result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
                return result;
            }
        }

        // Yardımcı metodlar
        public static int GetDecimalPlaces(string value)
        {
            var parts = value.Split('.');
            return parts.Length > 1 ? parts[1].Length : 0;
        }

        public static int GetTotalDigits(string value)
        {
            return value.Replace(".", "").Replace("-", "").Length;
        }

        public static bool IsValidDateFormat(string value)
        {
            // Boş değer kontrolü
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // YYYY-MM-DD formatını kontrol et
            if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return true;
            }

            // YYYY/MM/DD formatını da kabul et
            if (DateTime.TryParseExact(value, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return true;
            }

            // DD.MM.YYYY formatını da kabul et
            if (DateTime.TryParseExact(value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return true;
            }

            // DD/MM/YYYY formatını da kabul et
            if (DateTime.TryParseExact(value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return true;
            }

            // Genel DateTime.TryParse ile de dene
            if (DateTime.TryParse(value, out _))
            {
                return true;
            }

            return false;
        }

        // Alan kısıtlamalarını al
        public static XsdSchemaAnalyzer.SchemaConstraints GetFieldConstraints(TreeGridRow treeRow, XsdSchemaAnalyzer schemaAnalyzer, XsdSchemaAnalyzer headerSchemaAnalyzer)
        {
            if (schemaAnalyzer == null && headerSchemaAnalyzer == null) return null;

            // Önce Document XSD'de ara
            if (schemaAnalyzer != null && !string.IsNullOrEmpty(treeRow.Path))
            {
                var constraints = schemaAnalyzer.GetConstraintsByPath(treeRow.Path);
                if (constraints != null) return constraints;
            }

            // Eğer bulunamazsa, Header XSD'de ara
            if (headerSchemaAnalyzer != null && !string.IsNullOrEmpty(treeRow.Path))
            {
                var constraints = headerSchemaAnalyzer.GetConstraintsByPath(treeRow.Path);
                if (constraints != null) return constraints;
            }

            // Element adı ile de dene
            if (schemaAnalyzer != null)
            {
                var constraints = schemaAnalyzer.GetConstraintsByPath(treeRow.FieldName);
                if (constraints != null) return constraints;
            }

            if (headerSchemaAnalyzer != null)
            {
                var constraints = headerSchemaAnalyzer.GetConstraintsByPath(treeRow.FieldName);
                if (constraints != null) return constraints;
            }

            return null;
        }
    }
}

