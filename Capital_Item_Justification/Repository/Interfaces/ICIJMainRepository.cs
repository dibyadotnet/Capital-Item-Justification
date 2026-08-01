using Capital_Item_Justification.Models;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface ICIJMainRepository
    {
        Task<List<CijItemType>> GetItemType();
    }
}
