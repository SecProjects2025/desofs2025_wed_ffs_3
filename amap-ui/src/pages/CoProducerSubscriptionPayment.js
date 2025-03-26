// Resumo de pagamento (simplificado)
import React from 'react';

const PaymentSummary = ({ selectedProducts, totalAmount }) => {
    return (
        <div className="container" style={{ padding: '20px' }}>
            <h1>Resumo da Compra</h1>
            <div className="row">
                <div className="col-md-6">
                    <h3>Detalhes dos Produtos</h3>
                    <ul>
                        {selectedProducts.map((product) => (
                            <li key={product.id}>
                                {product.name} - {product.price}€ x {product.quantity}
                            </li>
                        ))}
                    </ul>
                </div>
                <div className="col-md-6">
                    <h3>Resumo de Pagamento</h3>
                    <p>Total: €{totalAmount}</p>
                    <div>
                        <button className="btn btn-success">Finalizar Pagamento</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PaymentSummary;
