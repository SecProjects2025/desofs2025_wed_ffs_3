
import React, { useState } from 'react';
import { Container, Row, Col, Button, Form, Card, Badge } from 'react-bootstrap';
import { FaCheckCircle, FaTimesCircle } from 'react-icons/fa';
import '../App.css';

const mockAMAPData = {
    name: "Época de Verão",
    startDate: "2024-06-01",
    endDate: "2024-08-31",
    deliveryDates: ["2024-06-15", "2024-06-29", "2024-07-13", "2024-07-27", "2024-08-10"],
};

const mockCatalog = [
    {
        id: 1,
        name: "Tomate",
        description: "Tomates frescos e maduros, ideais para saladas e molhos.",
        price: 3,
        units: "kg",
        photo: "https://media.istockphoto.com/id/466175630/pt/foto/tomate-isolado-em-fundo-branco.jpg?s=1024x1024&w=is&k=20&c=kRzZ6kkAVMWHyIgeqKg2ejv6og1Es1U54IrSGyPGYm8=",
    },
    {
        id: 2,
        name: "Alface",
        description: "Alface orgânica e crocante, cultivada localmente.",
        price: 2,
        units: "unidade",
        photo: "https://media.istockphoto.com/id/181072765/pt/foto/alface-isolado.jpg?s=1024x1024&w=is&k=20&c=_3uPFtA6twDkSowaTjN317A-obMgDM0lPJ_a6JlFCzo=",
    },
    {
        id: 3,
        name: "Cenoura",
        description: "Cenouras frescas, ricas em vitaminas e sabor.",
        price: 2.5,
        units: "kg",
        photo: "https://media.istockphoto.com/id/619252960/pt/foto/cenoura.jpg?s=1024x1024&w=is&k=20&c=q-njyXwNqnWljfZySPRgAToRoP_x6-6DwlGzCPjVRqI=",
    },
    {
        id: 4,
        name: "Batata",
        description: "Batatas de alta qualidade, perfeitas para qualquer prato.",
        price: 1.5,
        units: "kg",
        photo: "https://media.istockphoto.com/id/1475113258/pt/foto/raw-potatoes-freshly-cut-in-half-isolated-on-white-background.jpg?s=1024x1024&w=is&k=20&c=Ph6brEuFb4Y0FrpsqicmcHvpPn8SoKezhCH3fOWS5y0=",
    },
    {
        id: 5,
        name: "Cebola",
        description: "Cebolas frescas e saborosas, ótimas para temperar pratos.",
        price: 1.8,
        units: "kg",
        photo: "https://media.istockphoto.com/id/513920379/pt/foto/lâmpadas-de-cebola.jpg?s=1024x1024&w=is&k=20&c=Y-iNS-3sJwPpDVrm7SjwcZzBzz0ippyByICxEBzJW94=",
    },
    {
        id: 6,
        name: "Queijo de Cabra",
        description: "Queijo artesanal feito com leite de cabra local.",
        price: 12.5,
        units: "unidade",
        photo: "https://media.istockphoto.com/id/155442529/pt/foto/machego-semi-curada-queijo-de-cabra.jpg?s=1024x1024&w=is&k=20&c=4Ntl41AitW4rir0cJnWM9x_RmlsSQW_RwSmW_GR8X4g=",
    },
    {
        id: 7,
        name: "Cabaz de Legumes",
        description: "Cabaz com uma variedade de legumes da estação.",
        price: 20,
        units: "cabaz",
        photo: "https://media.istockphoto.com/id/168400647/pt/foto/cesta-de-frutas-e-produtos-hort%C3%ADcolas.jpg?s=1024x1024&w=is&k=20&c=SaHxlNbZU57ovR_mYKDZswyDUpbly9k0S2ildGJUuqs=",
    },
    {
        id: 8,
        name: "Abóbora",
        description: "Abóboras frescas e saborosas, ideais para sopas e assados.",
        price: 3.2,
        units: "kg",
        photo: "https://media.istockphoto.com/id/184279619/pt/foto/abóbora-menina.jpg?s=1024x1024&w=is&k=20&c=AE7O6jPJyUKGXZ9TtMVNYpiNkS1R7ZE2OrwxIHxkuHs=",
    },
    {
        id: 9,
        name: "Mel",
        description: "Mel natural, produzido por abelhas locais.",
        price: 7.5,
        units: "frasco (500g)",
        photo: "https://media.istockphoto.com/id/950996184/pt/foto/honey.jpg?s=1024x1024&w=is&k=20&c=qA4cPW1z8rJOjsWCS8SWEGSYR_3TpvMtR3RK4PfsBx8=",
    },
];

const ProducerOfferScreen = () => {
    const [selectedProducts, setSelectedProducts] = useState([]);
    const [selectedDates, setSelectedDates] = useState([]);
    const [paymentOption, setPaymentOption] = useState("integral");

    const handleProductSelect = (product) => {
        setSelectedProducts((prev) =>
            prev.find((p) => p.id === product.id)
                ? prev.filter((p) => p.id !== product.id) // Deselect if already selected
                : [...prev, product]
        );
    };

    const handleDateSelect = (date) => {
        setSelectedDates((prev) =>
            prev.includes(date) ? prev.filter((d) => d !== date) : [...prev, date]
        );
    };

    const handleSubmit = () => {
        console.log({
            selectedProducts,
            selectedDates,
            paymentOption,
        });
        alert("Oferta criada com sucesso!");
    };

    return (
        <Container>
            <h1 className="text-center my-4" style={{ color: '#2E8B57' }}>
                Criar Oferta de Produtos
            </h1>

            {/* Informações da AMAP */}
            <Card className="mb-4 shadow">
                <Card.Body>
                    <h5 className="text-primary">Informações da AMAP</h5>
                    <p>
                        <strong>Época:</strong> {mockAMAPData.name} <br />
                        <strong>Início:</strong> {mockAMAPData.startDate} <br />
                        <strong>Fim:</strong> {mockAMAPData.endDate}
                    </p>
                    <p><strong>Datas disponíveis para entrega:</strong></p>
                    <ul className="list-inline">
                        {mockAMAPData.deliveryDates.map((date) => (
                            <li className="list-inline-item me-2">
                                <Badge bg="info" key={date}>{date}</Badge>
                            </li>
                        ))}
                    </ul>
                </Card.Body>
            </Card>

            {/* Seleção de Produtos */}
            <Card className="mb-4 shadow">
                <Card.Body>
                    <h5 className="text-primary">Selecione Produtos do Catálogo</h5>
                    <Row>
                        {mockCatalog.map((product) => (
                            <Col key={product.id} sm={12} md={6} lg={4} className="my-2">
                                <Card
                                    className={`h-100 ${selectedProducts.some((p) => p.id === product.id) ? 'border-success' : ''}`}
                                >
                                    <Card.Img
                                        variant="top"
                                        src={product.photo}
                                        alt={product.name}
                                        style={{ height: '200px', objectFit: 'cover' }}
                                    />
                                    <Card.Body>
                                        <Card.Title>{product.name}</Card.Title>
                                        <Card.Text>{product.description}</Card.Text>
                                        <Card.Text>
                                            <strong>Preço:</strong> €{product.price} por {product.units}
                                        </Card.Text>
                                        <Button
                                            variant={selectedProducts.some((p) => p.id === product.id) ? 'success' : 'outline-secondary'}
                                            onClick={() => handleProductSelect(product)}
                                        >
                                            {selectedProducts.some((p) => p.id === product.id)
                                                ? 'Selecionado'
                                                : 'Selecionar'}
                                        </Button>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                </Card.Body>
            </Card>

            {/* Escolha de Datas */}
            <Card className="mb-4 shadow">
                <Card.Body>
                    <h5 className="text-primary">Escolha as Datas de Entrega</h5>
                    <Row>
                        {mockAMAPData.deliveryDates.map((date) => (
                            <Col sm={6} lg={4} key={date}>
                                <Card className={`mb-2 ${selectedDates.includes(date) ? 'border-success' : ''}`}>
                                    <Card.Body className="d-flex justify-content-between align-items-center">
                                        <span>{date}</span>
                                        <Button
                                            variant={selectedDates.includes(date) ? 'success' : 'outline-secondary'}
                                            onClick={() => handleDateSelect(date)}
                                        >
                                            {selectedDates.includes(date) ? <FaCheckCircle /> : <FaTimesCircle />}
                                        </Button>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                </Card.Body>
            </Card>

            {/* Opções de Fracionamento */}
            <Card className="mb-4 shadow">
                <Card.Body>
                    <h5 className="text-primary">Definir Fracionamento do Pagamento</h5>
                    <Form>
                        <Row>
                            {['integral', 'parcelado', 'fracionado'].map((option) => (
                                <Col sm={4} key={option}>
                                    <Form.Check
                                        type="radio"
                                        label={option.charAt(0).toUpperCase() + option.slice(1)}
                                        name="paymentOption"
                                        value={option}
                                        onChange={(e) => setPaymentOption(e.target.value)}
                                        checked={paymentOption === option}
                                        className="mb-3"
                                    />
                                </Col>
                            ))}
                        </Row>
                    </Form>
                </Card.Body>
            </Card>

            {/* Botão de Confirmação */}
            <div className="text-center">
                <Button variant="success" size="lg" onClick={handleSubmit}>
                    Confirmar Oferta
                </Button>
            </div>
        </Container>
    );
};

export default ProducerOfferScreen;
