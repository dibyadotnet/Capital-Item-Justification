using System.ComponentModel.DataAnnotations.Schema;

namespace Capital_Item_Justification.ViewModels
{
    public class CommitteeCommentViewModel
    {
        public int CommentId { get; set; }
        public int Cijid { get; set; }
        public string Comments { get; set; } = null!;

        public DateTime CommentDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; }
    }
}
