import React, { useState } from 'react';
import { Container, Button, Row, Col } from 'react-bootstrap';
import ProductCard from '../components/ProductCard';
import ProductModal from '../components/ProductModal';

// Dados para teste no frontend (substitua 'products' se necessário)
const mockProducts = [
  {
    id: 1,
    name: "Queijo de Cabra Curado",
    description: "Queijo artesanal curado, feito com leite de cabra da região.",
    photo: "https://media.istockphoto.com/id/155442529/pt/foto/machego-semi-curada-queijo-de-cabra.jpg?s=1024x1024&w=is&k=20&c=4Ntl41AitW4rir0cJnWM9x_RmlsSQW_RwSmW_GR8X4g=", // Placeholder confiável
    referencePrice: 12.5,
    deliveryUnits: "kg",
    type: "Simple",
  },
  {
    id: 2,
    name: "Cabaz de Legumes Variados",
    description: "Cabaz com uma seleção de legumes frescos da estação.",
    photo: "https://media.istockphoto.com/id/168400647/pt/foto/cesta-de-frutas-e-produtos-hortícolas.jpg?s=1024x1024&w=is&k=20&c=SaHxlNbZU57ovR_mYKDZswyDUpbly9k0S2ildGJUuqs=",
    referencePrice: 20.0,
    deliveryUnits: "unidades",
    type: "Composite",
    components: [
      { subProductId: 3, subProductName: "Cenoura", quantity: 1 },
      { subProductId: 4, subProductName: "Batata", quantity: 2 },
    ],
  },
  {
    id: 3,
    name: "Cenoura",
    description: "Cenouras orgânicas frescas, colhidas na hora.",
    photo: "https://media.istockphoto.com/id/619252960/pt/foto/cenoura.jpg?s=1024x1024&w=is&k=20&c=q-njyXwNqnWljfZySPRgAToRoP_x6-6DwlGzCPjVRqI=",
    referencePrice: 3.0,
    deliveryUnits: "kg",
    type: "Simple",
  },
  {
    id: 4,
    name: "Batata",
    description: "Batatas da melhor qualidade, perfeitas para assados.",
    photo: "https://media.istockphoto.com/id/1475113258/pt/foto/raw-potatoes-freshly-cut-in-half-isolated-on-white-background.jpg?s=1024x1024&w=is&k=20&c=Ph6brEuFb4Y0FrpsqicmcHvpPn8SoKezhCH3fOWS5y0=",
    referencePrice: 2.5,
    deliveryUnits: "kg",
    type: "Simple",
  },
];

const CatalogPage = () => {
  const [products, setProducts] = useState(mockProducts); // Usando dados mock para teste
  const [showModal, setShowModal] = useState(false);
  const [currentProduct, setCurrentProduct] = useState(null);

  const handleAddProduct = () => {
    setCurrentProduct(null); // Para adicionar um novo produto
    setShowModal(true);
  };

  const handleEditProduct = (product) => {
    setCurrentProduct(product); // Para editar um produto existente
    setShowModal(true);
  };

  const handleSaveProduct = (productData) => {
    if (currentProduct) {
      // Atualizar produto existente
      setProducts((prev) =>
          prev.map((p) => (p.id === currentProduct.id ? { ...currentProduct, ...productData } : p))
      );
    } else {
      // Criar novo produto
      const newProduct = { ...productData, id: Date.now() }; // Gera um ID para teste
      setProducts((prev) => [...prev, newProduct]);
    }
    setShowModal(false);
  };

  const handleDeleteProduct = (id) => {
    setProducts((prev) => prev.filter((product) => product.id !== id));
  };

  return (
      <Container>
        <h1 style={{ color: '#2E8B57' }}>Catálogo de Produtos</h1>
        <Button variant="success" onClick={handleAddProduct} className="my-3">
          Adicionar Produto
        </Button>
        <Row>
          {products.map((product) => (
              <Col key={product.id} sm={12} md={6} lg={4}>
                <ProductCard
                    product={product}
                    onEdit={handleEditProduct}
                    onDelete={() => handleDeleteProduct(product.id)}
                />
              </Col>
          ))}
        </Row>
        <ProductModal
            show={showModal}
            onHide={() => setShowModal(false)}
            onSave={handleSaveProduct}
            product={currentProduct}
            allProducts={products} // Passando todos os produtos para o modal
        />
      </Container>
  );
};

export default CatalogPage;