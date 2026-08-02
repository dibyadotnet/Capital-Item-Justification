using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface ICIJMainRepository
    {
        Task<List<CijItemType>> GetItemType();
        Task<List<CijPurchasePurpose>> GetPurchasePurpose();
        Task<List<CijOldEquipmemtTreatment>> GetOldEquipmentTreatment();
    }
}
