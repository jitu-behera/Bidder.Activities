namespace Bidder.Activities.Domain.Exceptions
{
    public class ValidationError
    {
        public ValidationError()
        {
        }

        public ValidationError(int code, string value, string description, string path = "")
        {
            Code = code;
            Value = value;
            Description = description;
            Path = path;
        }

        public int Code { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }
        public string Path { get; set; }
    }

    public class TransformationError : ValidationError
    {
        public object NewValue { get; set; }
        public TransformationError()
        {

        }

        public TransformationError(int code, string value, string description, string path, object newValue)
        {
            Code = code;
            Value = value;
            Description = description;
            Path = path;
            NewValue = newValue;
        }
    }
}
