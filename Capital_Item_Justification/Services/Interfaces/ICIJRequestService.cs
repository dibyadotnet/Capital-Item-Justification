using Capital_Item_Justification.Models;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface ICIJRequestService
    {
       Task<List<CijItemType>> GetItemType();
    }
}
