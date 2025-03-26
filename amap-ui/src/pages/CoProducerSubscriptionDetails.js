import React, { useState } from 'react';

const SubscriptionDetails = ({ selectedProducts, onDeleteProduct }) => {
    const [quantity, setQuantity] = useState(1);
    const [paymentMethod, setPaymentMethod] = useState('');
    const [subscriptionFrequency, setSubscriptionFrequency] = useState('monthly');

    const handleDelete = (productId) => {
        onDeleteProduct(productId);
    };

    return (
        <div className="container" style={{ padding: '20px' }}>
            <h1>As Suas Subscrições</h1>
            <div className="row">
                {selectedProducts.map((product) => (
                    <div key={product.id} className="col-12 mb-4">
                        <div className="card">
                            <div className="card-body">
                                <div className="row">
                                    <div className="col-md-3">
                                        <img src={product.photo} className="img-fluid" alt={product.name} />
                                    </div>
                                    <div className="col-md-3">
                                        <p><strong>{product.name}</strong></p>
                                        <p>{product.description}</p>
                                        <p>Produtor: {product.name}</p>
                                    </div>
                                    <div className="col-md-2">
                                        <select
                                            value={subscriptionFrequency}
                                            onChange={(e) => setSubscriptionFrequency(e.target.value)}
                                        >
                                            <option value="monthly">Mensal</option>
                                            <option value="weekly">Semanal</option>
                                            <option value="bi-weekly">Quinzenal</option>
                                        </select>
                                        <div>
                                            {/* Adicionar datas disponíveis */}
                                            <button className="btn btn-secondary">Selecionar Data</button>
                                        </div>
                                    </div>
                                    <div className="col-md-2">
                                        <input
                                            type="number"
                                            value={quantity}
                                            onChange={(e) => setQuantity(e.target.value)}
                                            min="1"
                                        />
                                    </div>
                                    <div className="col-md-2">
                                        <select
                                            value={paymentMethod}
                                            onChange={(e) => setPaymentMethod(e.target.value)}
                                        >
                                            <option value="credit-card">Cartão de Crédito</option>
                                            <option value="paypal">PayPal</option>
                                            <option value="bank-transfer">Transferência Bancária</option>
                                        </select>
                                        <p>Preço: €{product.price}</p>
                                    </div>
                                </div>
                                <button onClick={() => handleDelete(product.id)} className="btn btn-danger">
                                    Apagar
                                </button>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
            <div style={{ textAlign: 'center' }}>
                <button className="btn btn-success">Pagar</button>
            </div>
        </div>
    );
};

export default SubscriptionDetails;
