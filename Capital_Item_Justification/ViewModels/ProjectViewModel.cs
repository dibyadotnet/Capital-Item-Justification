using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
