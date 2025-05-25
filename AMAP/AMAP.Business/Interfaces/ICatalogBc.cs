using AMAP.Entities.Models.DTO.Catalog;

namespace AMAP.Business.Interfaces
{
    public interface ICatalogBc
    {
        Task<List<CatalogDto>> GetAllCatalogs();

        Task<List<CatalogDto>> GetCatalogByProducer(string producerId);
    }
}
