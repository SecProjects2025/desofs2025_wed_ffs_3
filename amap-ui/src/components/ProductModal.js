import React, { useState, useEffect } from 'react';
import { Modal, Button, Form, ListGroup, InputGroup } from 'react-bootstrap';

const ProductModal = ({ show, onHide, onSave, product, productsList }) => {
  const [isComposite, setIsComposite] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    photo: '',
    referencePrice: '',
    deliveryUnits: '',
    compositeItems: [],
  });

  useEffect(() => {
    if (product) {
      setFormData({
        ...product,
        compositeItems: product.components || [],
        referencePrice: product.referencePrice ? product.referencePrice.replace('€', '').trim() : '',
      });
      setIsComposite(product.type === 'Composite');
    } else {
      setFormData({
        name: '',
        description: '',
        photo: '',
        referencePrice: '',
        deliveryUnits: '',
        compositeItems: [],
      });
      setIsComposite(false);
    }
  }, [product]);

  const handleTypeChange = (e) => {
    setIsComposite(e.target.value === 'Composite');
    setFormData((prev) => ({
      ...prev,
      compositeItems: [],
    }));
  };

  const handleProductSelect = (productId) => {
    if (!formData.compositeItems.some((item) => item.subProductId === productId)) {
      setFormData((prev) => ({
        ...prev,
        compositeItems: [...prev.compositeItems, { subProductId: productId, quantity: 1 }],
      }));
    }
  };

  const handleQuantityChange = (productId, quantity) => {
    setFormData((prev) => ({
      ...prev,
      compositeItems: prev.compositeItems.map((item) =>
        item.subProductId === productId ? { ...item, quantity: Number(quantity) } : item
      ),
    }));
  };

  const handleRemoveProduct = (productId) => {
    setFormData((prev) => ({
      ...prev,
      compositeItems: prev.compositeItems.filter((item) => item.subProductId !== productId),
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const referencePrice = parseFloat(formData.referencePrice);
    const deliveryUnits = parseInt(formData.deliveryUnits, 10);

    if (!formData.name || !formData.photo || isNaN(referencePrice) || isNaN(deliveryUnits)) {
      alert('Por favor, preencha todos os campos obrigatórios.');
      return;
    }

    if (referencePrice <= 0) {
      alert('O preço deve ser maior que 0.');
      return;
    }

    if (deliveryUnits <= 0) {
      alert('O número de unidades de entrega deve ser maior que 0.');
      return;
    }

    if (isComposite && formData.compositeItems.length === 0) {
      alert('Adicione pelo menos um produto ao cabaz.');
      return;
    }

    onSave({ ...formData, referencePrice: formData.referencePrice.replace('€', '').trim() });
    onHide();
  };

  return (
    <Modal show={show} onHide={onHide}>
      <Modal.Header closeButton>
        <Modal.Title>{product ? 'Editar Produto' : 'Adicionar Produto'}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form onSubmit={handleSubmit}>
          {!product && (
            <Form.Group>
              <Form.Label>Tipo de Produto</Form.Label>
              <Form.Control
                as="select"
                onChange={handleTypeChange}
                value={isComposite ? 'Composite' : 'Simple'}
              >
                <option value="Simple">Simples</option>
                <option value="Composite">Cabaz</option>
              </Form.Control>
            </Form.Group>
          )}
          <Form.Group>
            <Form.Label>Nome</Form.Label>
            <Form.Control
              type="text"
              name="name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </Form.Group>
          <Form.Group>
            <Form.Label>Descrição</Form.Label>
            <Form.Control
              as="textarea"
              name="description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            />
          </Form.Group>
          <Form.Group>
            <Form.Label>Foto URL</Form.Label>
            <Form.Control
              type="text"
              name="photo"
              value={formData.photo}
              onChange={(e) => setFormData({ ...formData, photo: e.target.value })}
              required
            />
          </Form.Group>
          <Form.Group>
            <Form.Label>Preço</Form.Label>
            <Form.Control
              type="number"
              name="referencePrice"
              min="0.01"
              step="0.01"
              value={formData.referencePrice}
              onChange={(e) => setFormData({ ...formData, referencePrice: e.target.value })}
              required
            />
          </Form.Group>
          <Form.Group>
            <Form.Label>Unidades de Entrega</Form.Label>
            <Form.Control
              type="number"
              name="deliveryUnits"
              min="1"
              value={formData.deliveryUnits}
              onChange={(e) => setFormData({ ...formData, deliveryUnits: e.target.value })}
              required
            />
          </Form.Group>
          {isComposite && (
            <div>
              <Form.Label>Adicionar Produtos ao Cabaz</Form.Label>
              <ListGroup>
                {productsList.map((p) => (
                  <ListGroup.Item key={p.id}>
                    <div className="d-flex justify-content-between align-items-center">
                      <span>{p.name}</span>
                      {formData.compositeItems.some((item) => item.subProductId === p.id) ? (
                        <InputGroup style={{ width: '100px' }}>
                          <Form.Control
                            type="number"
                            min="1"
                            value={
                              formData.compositeItems.find((item) => item.subProductId === p.id)?.quantity || 1
                            }
                            onChange={(e) => handleQuantityChange(p.id, e.target.value)}
                          />
                          <Button variant="danger" onClick={() => handleRemoveProduct(p.id)}>
                            -
                          </Button>
                        </InputGroup>
                      ) : (
                        <Button variant="primary" onClick={() => handleProductSelect(p.id)}>
                          +
                        </Button>
                      )}
                    </div>
                  </ListGroup.Item>
                ))}
              </ListGroup>
            </div>
          )}
          <Button variant="secondary" onClick={onHide}>
            Cancelar
          </Button>
          <Button variant="primary" type="submit">
            Guardar
          </Button>
        </Form>
      </Modal.Body>
    </Modal>
  );
};

export default ProductModal;
