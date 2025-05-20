using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface IProductBc
    {
        Product? GetProduct(int id);

        Product CreateProduct(Product product, string producerId);

        public void UpdateProduct(int id, Product product, string producerId);

        void DeleteProduct(int id, string producerId);

        (List<Product> ImportedProducts, List<string> Errors) ImportProductsFromFile(Stream fileStream, string fileName, string producerId);

        (byte[] FileContent, string FileName, string ContentType) ExportProductsToFile(string producerId, string contentRootPath);
    }
}
