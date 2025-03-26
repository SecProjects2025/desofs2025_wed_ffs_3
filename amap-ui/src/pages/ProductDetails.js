import React from 'react';
import { useParams } from 'react-router-dom';
import { Container, Card } from 'react-bootstrap';

const ProductDetails = ({ products }) => {
    const { id } = useParams();
    const product = products.find((item) => item.id === id);

    return (
        <Container className="mt-5">
            {product ? (
                <Card>
                    <Card.Img variant="top" src={product.image} />
                    <Card.Body>
                        <Card.Title>{product.name}</Card.Title>
                        <Card.Text>{product.description}</Card.Text>
                        <Card.Text><strong>Preço:</strong> {product.price} €</Card.Text>
                    </Card.Body>
                </Card>
            ) : (
                <p>Produto não encontrado.</p>
            )}
        </Container>
    );
};

export default ProductDetails;
