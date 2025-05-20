namespace AMAP.Entities.Models.DTO.Catalog
{
    public class CatalogDto
    {
        public string? ProducerName { get; set; }

        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
