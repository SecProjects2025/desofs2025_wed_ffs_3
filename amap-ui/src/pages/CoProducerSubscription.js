import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // Importando o useNavigate

const SubscriptionScreen = () => {
    const [products, setProducts] = useState([]);
    const [selectedProducts, setSelectedProducts] = useState([]);
    const [subscriptionType, setSubscriptionType] = useState('');
    const [paymentType, setPaymentType] = useState('');
    const [modalVisible, setModalVisible] = useState(false);
    const navigate = useNavigate(); // Substituindo useHistory por useNavigate

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

        // Usando navigate() para redirecionar
        navigate('/subscription-details');  // Redireciona para a tela de detalhes
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
