import React from "react";
import { Card, Button, ListGroup, Badge } from "react-bootstrap";
import { FaEdit, FaTrash } from "react-icons/fa";

const ProductCard = ({ product, onEdit, onDelete }) => {
  // Verifica se o produto tem subprodutos no campo `components`
  const isComposite = product.type === "Composite" && product.components && product.components.length > 0;

  return (
    <Card className="my-3">
      <Card.Img
        variant="top"
        src={product.photo}
        alt={product.name}
        style={{
          width: "310px",
          height: "250px",
          objectFit: "cover",
          borderRadius: "8px",
          margin: "0 auto",
        }}
      />
      <Card.Body>
        <Card.Title>
          {product.name} {isComposite && <Badge bg="primary">Cabaz</Badge>}
        </Card.Title>
        <Card.Text>{product.description}</Card.Text>
        <Card.Text>
          <strong>Preço:</strong> {product.referencePrice}
        </Card.Text>
        <Card.Text>
          <strong>Unidades de entrega:</strong> {product.deliveryUnits}
        </Card.Text>

        {isComposite && (
          <>
            <Card.Text>
              <strong>Produtos no Cabaz:</strong>
            </Card.Text>
            <ListGroup variant="flush" className="mb-3">
              {product.components.map((component, index) => (
                <ListGroup.Item key={index} className="d-flex justify-content-between">
                  <span>{component.subProductName}</span>
                  <span>Quantidade: {component.quantity}</span>
                </ListGroup.Item>
              ))}
            </ListGroup>
          </>
        )}

        <div className="d-flex justify-content-between mt-3">
          <Button
            variant="success"
            onClick={() => onEdit(product)}
            style={{ width: "48%" }}
          >
            <FaEdit /> Editar
          </Button>
          <Button
            variant="danger"
            onClick={() => onDelete(product.id)}
            style={{ width: "48%" }}
          >
            <FaTrash /> Excluir
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
};

export default ProductCard;
