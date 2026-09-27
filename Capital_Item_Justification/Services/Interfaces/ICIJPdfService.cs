namespace Capital_Item_Justification.Services.Interfaces
{
    public interface ICIJPdfService
    {
        Task<byte[]> GeneratePdfAsync(int cijId);
    }
}
