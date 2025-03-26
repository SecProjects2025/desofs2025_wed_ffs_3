import React, { useState, useEffect } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';
import { getDeliveryDates } from '../service/offerService'; // Assumindo que a função esteja em um ficheiro separado

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
  const [step, setStep] = useState(1); // Para controlar o fluxo (passo 1 ou 2)
  const [summary, setSummary] = useState(null); // Para armazenar os dados configurados
  const [deliveryDates, setDeliveryDates] = useState([]); // Estado para armazenar as datas de entrega

  // Buscar as datas de entrega ao backend
  useEffect(() => {
    const fetchDeliveryDates = async () => {
      try {
        const fetchedDates = await getDeliveryDates();
        setDeliveryDates(fetchedDates); // Atualiza o estado com as datas de entrega obtidas
      } catch (error) {
        console.error('Erro ao carregar datas de entrega:', error);
      }
    };
    fetchDeliveryDates();
  }, []);

  // Função para selecionar as datas disponíveis
  const handleDateSelection = (date) => {
    setSelectedDates((prevDates) =>
        prevDates.includes(date)
            ? prevDates.filter((d) => d !== date)
            : [...prevDates, date]
    );
  };

  // Função para alterar o stock e preço de um produto
  const handleProductChange = (id, field, value) => {
    setProducts((prevProducts) =>
        prevProducts.map((product) =>
            product.id === id ? { ...product, [field]: value } : product
        )
    );
  };

  // Função para alterar as datas de entrega de um produto
  const handleProductDateSelection = (id, date) => {
    setProducts((prevProducts) =>
        prevProducts.map((product) =>
            product.id === id
                ? {
                  ...product,
                  deliveryDates: product.deliveryDates.includes(date)
                      ? product.deliveryDates.filter((d) => d !== date)
                      : [...product.deliveryDates, date],
                }
                : product
        )
    );
  };

  // Avançar para o próximo passo
  const proceedToStep2 = () => {
    if (selectedDates.length === 0) {
      alert("Por favor, selecione pelo menos uma data de entrega.");
      return;
    }
    setStep(2);
  };

  // Finalizar configuração
  const finalizeConfiguration = () => {
    const configuredProducts = products.filter(
        (product) => product.stock > 0 && product.deliveryDates.length > 0
    );
    setSummary({
      selectedDates,
      configuredProducts,
    });
  };

  return (
      <div className="container mt-4">
        {step === 1 && (
            <div>
              <h1 className="mb-4">Seleção de Datas de Entrega</h1>
              <p>Por favor, selecione as datas disponíveis em que deseja realizar entregas:</p>
              <div className="mb-4">
                {deliveryDates.map((date) => (
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
              <p>Defina o stock, altere o preço (se necessário) e escolha as datas de entrega para cada produto:</p>
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

                          {/* Alterar o preço */}
                          <div className="mb-3">
                            <label className="form-label">Novo preço:</label>
                            <input
                                type="number"
                                className="form-control"
                                value={product.updatedPrice}
                                onChange={(e) => handleProductChange(product.id, "updatedPrice", parseFloat(e.target.value))}
                            />
                          </div>

                          {/* Alterar o stock */}
                          <div className="mb-3">
                            <label className="form-label">Stock disponível:</label>
                            <input
                                type="number"
                                className="form-control"
                                value={product.stock}
                                onChange={(e) => handleProductChange(product.id, "stock", parseInt(e.target.value) || 0)}
                            />
                          </div>

                          {/* Selecionar as datas de entrega */}
                          <div className="mb-3">
                            <label className="form-label">Datas de entrega:</label>
                            {deliveryDates.map((date) => (
                                <div className="form-check" key={date}>
                                  <input
                                      className="form-check-input"
                                      type="checkbox"
                                      value={date}
                                      checked={product.deliveryDates.includes(date)}
                                      onChange={() => handleProductDateSelection(product.id, date)}
                                      id={`${product.id}-${date}`}
                                  />
                                  <label className="form-check-label" htmlFor={`${product.id}-${date}`}>
                                    {date}
                                  </label>
                                </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    </div>
                ))}
              </div>

              <button className="btn btn-primary" onClick={finalizeConfiguration}>
                Finalizar Configuração
              </button>
            </div>
        )}

        {summary && (
            <div className="mt-4">
              <h2>Resumo da Configuração</h2>
              <p>Datas de entrega selecionadas: {summary.selectedDates.join(", ")}</p>
              <h3>Produtos configurados:</h3>
              <ul>
                {summary.configuredProducts.map((product) => (
                    <li key={product.id}>
                      {product.name} - Stock: {product.stock}, Preço: {product.updatedPrice}€/unidade, Datas de entrega: {product.deliveryDates.join(", ")}
                    </li>
                ))}
              </ul>
            </div>
        )}
      </div>
  );
}

export default OfferPage;
