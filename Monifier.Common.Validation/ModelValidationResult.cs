namespace Monifier.Common.Validation
{
    public class ModelValidationResult
    {
        public ModelValidationResult(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }

        public ModelValidationResult(string message)
        {
            PropertyName = string.Empty;
            Message = message;
        }
        
        public string PropertyName { get; }
        public string Message { get; }
    }
}