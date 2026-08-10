using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class AttachmentViewModel
    {
        public int AttachmentId { get; set; }
        public int Cijid { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? ModuleName { get; set; } = null!;
    }
}
