using sjam.Dal.Enum;
using System.ComponentModel.DataAnnotations;

namespace sjam.Models
{
    public class ServiceResponse<T>
    {
        public T? Result { get; set; }
        public APIResponseStatus ResponseStatus { get; set; }
        public string Message { get; set; }
        public ICollection<ValidationResult> ValidationResults { get; set; }
    }
}
