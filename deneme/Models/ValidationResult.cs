namespace validator.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string ValidationType { get; set; }
    }
}

