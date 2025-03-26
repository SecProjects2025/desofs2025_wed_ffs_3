/*import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, ListGroup, Button } from 'react-bootstrap';
import axios from 'axios';

const CoProducerMainScreen = () => {
    const [isPurchaseSeasonOpen, setIsPurchaseSeasonOpen] = useState(false);
    const [products, setProducts] = useState([]);
    const [offers, setOffers] = useState([]);
    const [purchaseDates, setPurchaseDates] = useState({ startDate: '', endDate: '' });

    useEffect(() => {
        // Função para carregar a época de compras
        const fetchPurchaseSeason = async () => {
            try {
                const response = await axios.get('/api/SubscriptionPeriod/1'); // Atualizar o ID conforme necessário
                const { startDate, endDate, products: productList } = response.data;

                const currentDate = new Date();
                const start = new Date(startDate);
                const end = new Date(endDate);

                // Verificar se a época está aberta
                const isOpen = currentDate >= start && currentDate <= end;
                setIsPurchaseSeasonOpen(isOpen);

                setPurchaseDates({ startDate, endDate });
                setProducts(productList || []);
            } catch (error) {
                console.error('Erro ao carregar os dados da época de compras:', error);
            }
        };

        // Função para carregar ofertas de produtos
        const fetchProductOffers = async () => {
            try {
                const response = await axios.get('/api/ProductOffer');
                setOffers(response.data || []);
            } catch (error) {
                console.error('Erro ao carregar as ofertas de produtos:', error);
            }
        };

        fetchPurchaseSeason();
        fetchProductOffers();
    }, []);

    const renderProductCard = (product) => {
        const productOffers = offers.filter((offer) => offer.productId === product.id);

        return (
            <Col md={4} key={product.id} className="mb-4">
                <Card className="shadow-sm">
                    <Card.Img
                        variant="top"
                        src={product.image || 'default-product.jpg'}
                        alt={product.name}
                        className="img-fluid"
                        style={{ height: '200px', objectFit: 'cover' }}
                    />
                    <Card.Body>
                        <Card.Title>{product.name}</Card.Title>
                        <Card.Text>{product.description}</Card.Text>
                        <Card.Text>
                            <strong>Preço Base:</strong> {product.price} €
                        </Card.Text>
                        {productOffers.length > 0 ? (
                            <ListGroup variant="flush">
                                {productOffers.map((offer) => (
                                    <ListGroup.Item key={offer.id}>
                                        <strong>Oferta:</strong> {offer.price} € ({offer.deliveryDate})
                                    </ListGroup.Item>
                                ))}
                            </ListGroup>
                        ) : (
                            <p className="text-muted">Sem ofertas disponíveis</p>
                        )}
                        <Button variant="primary" block>
                            Comprar
                        </Button>
                    </Card.Body>
                </Card>
            </Col>
        );
    };

    return (
        <Container className="mt-5">
            {isPurchaseSeasonOpen ? (
                <div className="text-center mb-5">
                    <h1 className="text-success">Época de compra de produtos ABERTA</h1>
                    <p>
                        De <strong>{purchaseDates.startDate}</strong> até <strong>{purchaseDates.endDate}</strong>.
                    </p>
                    <Row>{products.map((product) => renderProductCard(product))}</Row>
                </div>
            ) : (
                <div className="text-center">
                    <h1 className="text-danger">Época de compras fechada</h1>
                    <p>
                        De momento não está nenhuma época de compras aberta. Os produtos ainda estão a ser cultivados.
                    </p>
                    <img
                        src="/closed-season.jpg"
                        alt="Época fechada"
                        className="img-fluid rounded"
                        style={{ maxWidth: '60%' }}
                    />
                </div>
            )}
        </Container>
    );
};

export default CoProducerMainScreen;
*/
import React, { useState, useEffect } from 'react';

const SubscriptionScreen = () => {
    const [products, setProducts] = useState([]);
    const [selectedProducts, setSelectedProducts] = useState([]);
    const [subscriptionType, setSubscriptionType] = useState('');
    const [paymentType, setPaymentType] = useState('');
    const [modalVisible, setModalVisible] = useState(false);

    useEffect(() => {
        const mockProducts = [
            { id: 1, name: 'Produto A', description: 'Descrição do Produto A', price: 20, photo: 'https://via.placeholder.com/100' },
            { id: 2, name: 'Produto B', description: 'Descrição do Produto B', price: 30, photo: 'https://via.placeholder.com/100' },
            { id: 3, name: 'Produto C', description: 'Descrição do Produto C', price: 25, photo: 'https://via.placeholder.com/100' },
            // Mais produtos
        ];

        setProducts(mockProducts);
    }, []);

    const toggleProductSelection = (product) => {
        setSelectedProducts((prevSelected) => {
            if (prevSelected.find((p) => p.id === product.id)) {
                return prevSelected.filter((p) => p.id !== product.id);
            } else {
                return [...prevSelected, product];
            }
        });
    };

    const handleSubmit = () => {
        if (!subscriptionType || !paymentType) {
            alert('Por favor, selecione o tipo de subscrição e método de pagamento.');
            return;
        }

        console.log('Dados da subscrição:', selectedProducts);
        alert('Subscrição enviada com sucesso!');
        setModalVisible(false);
        setSelectedProducts([]);
        setSubscriptionType('');
        setPaymentType('');
    };

    const totalPrice = selectedProducts.reduce((total, product) => total + product.price, 0);

    return (
        <div className="container" style={{ padding: '20px' }}>
            <h1>Produtos Disponíveis</h1>
            <div className="row">
                {products.map((product) => (
                    <div key={product.id} className="col-md-4 mb-4">
                        <div className="card" style={{ width: '100%' }}>
                            <img src={product.photo} className="card-img-top" alt={product.name} />
                            <div className="card-body">
                                <h5 className="card-title">{product.name}</h5>
                                <p className="card-text">{product.description}</p>
                                <button
                                    onClick={() => toggleProductSelection(product)}
                                    className={`btn ${selectedProducts.find((p) => p.id === product.id) ? 'btn-success' : 'btn-secondary'}`}
                                >
                                    {selectedProducts.find((p) => p.id === product.id) ? 'Selecionado' : 'Selecionar'}
                                </button>
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Carrinho de compras */}
            {selectedProducts.length > 0 && (
                <div style={{ position: 'fixed', bottom: '20px', right: '20px' }}>
                    <button
                        onClick={() => setModalVisible(true)}
                        className="btn btn-primary"
                        style={{ position: 'relative' }}
                    >
                        <i className="bi bi-cart"></i> Carrinho (
                        <span style={{ color: 'red' }}>{selectedProducts.length}</span>)
                        <div>Total: €{totalPrice}</div>
                    </button>
                </div>
            )}

            {/* Modal para revisar a subscrição */}
            {modalVisible && (
                <div
                    style={{
                        position: 'fixed',
                        top: '50%',
                        left: '50%',
                        transform: 'translate(-50%, -50%)',
                        background: 'white',
                        padding: '20px',
                        boxShadow: '0 0 10px rgba(0,0,0,0.5)',
                        width: '400px',
                    }}
                >
                    <h2>Confirmação da Subscrição</h2>
                    <h4>Produtos Selecionados:</h4>
                    <ul>
                        {selectedProducts.map((product) => (
                            <li key={product.id}>{product.name}</li>
                        ))}
                    </ul>
                    {/* Picker para Tipo de Subscrição */}
                    <label>
                        Tipo de Subscrição:
                        <select
                            value={subscriptionType}
                            onChange={(e) => setSubscriptionType(e.target.value)}
                            style={{ display: 'block', marginBottom: '10px' }}
                        >
                            <option value="">Selecione</option>
                            <option value="monthly">Mensal</option>
                            <option value="seasonal">Sazonal</option>
                        </select>
                    </label>

                    {/* Picker para Tipo de Pagamento */}
                    <label>
                        Método de Pagamento:
                        <select
                            value={paymentType}
                            onChange={(e) => setPaymentType(e.target.value)}
                            style={{ display: 'block', marginBottom: '10px' }}
                        >
                            <option value="">Selecione</option>
                            <option value="credit-card">Cartão de Crédito</option>
                            <option value="paypal">PayPal</option>
                            <option value="bank-transfer">Transferência Bancária</option>
                        </select>
                    </label>

                    <button
                        onClick={handleSubmit}
                        className="btn btn-success"
                    >
                        Confirmar Subscrição
                    </button>
                    <button
                        onClick={() => setModalVisible(false)}
                        className="btn btn-danger"
                    >
                        Fechar
                    </button>
                </div>
            )}
        </div>
    );
};

export default SubscriptionScreen;
