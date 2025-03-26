import React, { useState } from 'react';
import { Container, Button, Row, Col } from 'react-bootstrap';
import ProductCard from '../components/ProductCard';
import ProductModal from '../components/ProductModal';
import useProducts from '../hooks/useProducts';

const CatalogPageBE = () => {
  const { products, loading, error, handleCreate, handleUpdate, handleDelete } = useProducts();
  const [showModal, setShowModal] = useState(false);
  const [currentProduct, setCurrentProduct] = useState(null);

  const handleAddProduct = () => {
    setCurrentProduct(null);
    setShowModal(true);
  };

  const handleEditProduct = (product) => {
    setCurrentProduct(product);
    setShowModal(true);
  };

  const handleSaveProduct = (productData) => {
    if (currentProduct) {
      handleUpdate(currentProduct.id, productData);
    } else {
      handleCreate(productData);
    }
    setShowModal(false);
  };

  return (
    <Container>
      <h1 style={{ color: '#2E8B57' }}>Catálogo de Produtos</h1>
      {error && <p>{error}</p>}
      {loading ? (
        <p>Carregando produtos...</p>
      ) : (
        <>
          <Button variant="success" onClick={handleAddProduct} className="my-3">
            Adicionar Produto
          </Button>
          <Row>
            {products.map((product) => (
              <Col key={product.id} sm={12} md={6} lg={4}>
                <ProductCard
                  product={product}
                  onEdit={handleEditProduct}
                  onDelete={handleDelete}
                />
              </Col>
            ))}
          </Row>
        </>
      )}
      <ProductModal
        show={showModal}
        onHide={() => setShowModal(false)}
        onSave={handleSaveProduct}
        product={currentProduct}
        productsList={products}
      />

    </Container>
  );
};

export default CatalogPageBE;
