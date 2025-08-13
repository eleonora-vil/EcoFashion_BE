using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class RejectApplicationRequest
    {
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do từ chối không được quá 500 ký tự")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
