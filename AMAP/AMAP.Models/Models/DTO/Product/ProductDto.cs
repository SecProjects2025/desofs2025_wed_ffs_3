public class ProductDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Photo { get; set; }

    public double ReferencePrice { get; set; }

    public string? DeliveryUnits { get; set; }

    public string? Type { get; set; }

    public IEnumerable<ProductComposeDto>? Composites { get; set; }
}
