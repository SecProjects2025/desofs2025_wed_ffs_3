import React, { useState } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

const initialProducts = [
    { id: 1, name: "Queijo de Cabra Curado", price: 10, unit: "kg", image: "https://media.istockphoto.com/id/155442529/pt/foto/machego-semi-curada-queijo-de-cabra.jpg?s=1024x1024&w=is&k=20&c=4Ntl41AitW4rir0cJnWM9x_RmlsSQW_RwSmW_GR8X4g="},
    { id: 2, name: "Cabaz de Legumes", price: 20, unit: "unidade", image: "https://media.istockphoto.com/id/168400647/pt/foto/cesta-de-frutas-e-produtos-hortícolas.jpg?s=1024x1024&w=is&k=20&c=SaHxlNbZU57ovR_mYKDZswyDUpbly9k0S2ildGJUuqs="},
    { id: 3, name: "Tomate Biológico", price: 3, unit: "kg", image: "https://media.istockphoto.com/id/1450576005/pt/foto/tomato-isolated-tomato-on-white-background-perfect-retouched-tomatoe-side-view-with-clipping.jpg?s=612x612&w=is&k=20&c=wrJqybWt5Qdr9ICeTsOtQavKI6JDwbV5YWKHm9ic8bs="},
    { id: 4, name: "Alface Orgânica", price: 2, unit: "unidade", image: "https://media.istockphoto.com/id/469159458/pt/foto/alface-isolado-em-fundo-branco.jpg?s=612x612&w=is&k=20&c=xeivyrZzE2CqLmZ-XaBZTlhz5TETNOAui29li6SjQYY=" },
    { id: 5, name: "Maçã Verde", price: 4, unit: "kg", image: "https://media.istockphoto.com/id/95761550/pt/foto/green-maçã.jpg?s=612x612&w=is&k=20&c=T-_AkBAhwJbqtQ4MkmI_S24fgFU3rc_uZ8nUieICJfQ=" },
];

function OfferPage() {
    const [selectedDates, setSelectedDates] = useState([]);
    const [products, setProducts] = useState(
        initialProducts.map((product) => ({
            ...product,
            stock: 0,
            updatedPrice: product.price,
            deliveryDates: [],
        }))
    );
    const [paymentOptions, setPaymentOptions] = useState({
        integral: true,
        parcelado: false,
    }); // Controle das formas de pagamento
    const [step, setStep] = useState(1);
    const [summary, setSummary] = useState(null);

    const handleDateSelection = (date) => {
        setSelectedDates((prevDates) =>
            prevDates.includes(date)
                ? prevDates.filter((d) => d !== date)
                : [...prevDates, date]
        );
    };

    const handleProductChange = (id, field, value) => {
        setProducts((prevProducts) =>
            prevProducts.map((product) =>
                product.id === id ? { ...product, [field]: value } : product
            )
        );
    };

    const togglePaymentOption = (option) => {
        // Prevenir que ambas as opções sejam desmarcadas
        if (paymentOptions[option] && Object.values(paymentOptions).filter(Boolean).length === 1) {
            alert("Pelo menos uma forma de pagamento deve estar selecionada.");
            return;
        }
        setPaymentOptions((prev) => ({
            ...prev,
            [option]: !prev[option],
        }));
    };

    const proceedToStep2 = () => {
        if (selectedDates.length === 0) {
            alert("Por favor, selecione pelo menos uma data de entrega.");
            return;
        }
        setStep(2);
    };

    const finalizeConfiguration = () => {
        if (!paymentOptions.integral && !paymentOptions.parcelado) {
            alert("Por favor, selecione pelo menos uma forma de pagamento.");
            return;
        }
        const configuredProducts = products.filter(
            (product) => product.stock > 0 && product.deliveryDates.length > 0
        );
        setSummary({
            selectedDates,
            configuredProducts,
            paymentOptions,
        });
    };

    return (
        <div className="container mt-4">
            {step === 1 && (
                <div>
                    <h1 className="mb-4">Seleção de Datas de Entrega</h1>
                    <p>Por favor, selecione as datas disponíveis em que deseja realizar entregas:</p>
                    <div className="mb-4">
                        {["2024-12-10", "2024-12-17", "2024-12-24"].map((date) => (
                            <div className="form-check" key={date}>
                                <input
                                    className="form-check-input"
                                    type="checkbox"
                                    value={date}
                                    checked={selectedDates.includes(date)}
                                    onChange={() => handleDateSelection(date)}
                                    id={date}
                                />
                                <label className="form-check-label" htmlFor={date}>
                                    {date}
                                </label>
                            </div>
                        ))}
                    </div>
                    <button className="btn btn-success" onClick={proceedToStep2}>
                        Continuar
                    </button>
                </div>
            )}

            {step === 2 && (
                <div>
                    <h1 className="mb-4">Configuração dos Produtos</h1>
                    <p>Defina as configurações gerais e específicas para seus produtos:</p>

                    <div className="mb-4">
                        <h5>Forma de pagamento</h5>
                        <div className="form-check">
                            <input
                                type="checkbox"
                                className="form-check-input"
                                id="payment-integral"
                                checked={paymentOptions.integral}
                                onChange={() => togglePaymentOption("integral")}
                            />
                            <label className="form-check-label" htmlFor="payment-integral">
                                Integral
                            </label>
                        </div>
                        <div className="form-check">
                            <input
                                type="checkbox"
                                className="form-check-input"
                                id="payment-parcelado"
                                checked={paymentOptions.parcelado}
                                onChange={() => togglePaymentOption("parcelado")}
                            />
                            <label className="form-check-label" htmlFor="payment-parcelado">
                                Parcelado
                            </label>
                        </div>
                    </div>

                    <div className="row">
                        {products.map((product) => (
                            <div key={product.id} className="col-md-6 mb-4">
                                <div className="card">
                                    <img
                                        src={product.image}
                                        alt={product.name}
                                        className="card-img-top"
                                        style={{ height: "300px", objectFit: "cover" }}
                                    />
                                    <div className="card-body">
                                        <h5 className="card-title">{product.name}</h5>
                                        <p>Preço original: {product.price}€/{product.unit}</p>
                                        <div className="mb-3">
                                            <label className="form-label">Novo preço:</label>
                                            <input
                                                type="number"
                                                className="form-control"
                                                value={product.updatedPrice}
                                                onChange={(e) =>
                                                    handleProductChange(product.id, "updatedPrice", parseFloat(e.target.value))
                                                }
                                            />
                                        </div>
                                        <div className="mb-3">
                                            <label className="form-label">Stock disponível:</label>
                                            <input
                                                type="number"
                                                className="form-control"
                                                value={product.stock}
                                                onChange={(e) =>
                                                    handleProductChange(product.id, "stock", parseInt(e.target.value) || 0)
                                                }
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                    <button className="btn btn-success" onClick={finalizeConfiguration}>
                        Finalizar Configuração
                    </button>
                </div>
            )}

            {summary && (
                <div className="mt-4">
                    <h2>Resumo da Configuração</h2>
                    <div className="card p-4">
                        <h5>Formas de Pagamento Selecionadas:</h5>
                        <ul>
                            {paymentOptions.integral && <li>Pagamento Integral</li>}
                            {paymentOptions.parcelado && <li>Pagamento Parcelado</li>}
                        </ul>
                        <h5>Produtos Configurados:</h5>
                        {summary.configuredProducts.map((product) => (
                            <p key={product.id}>
                                {product.name} - {product.stock} unidades a {product.updatedPrice}€.
                            </p>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}

export default OfferPage;
