import React, { useState } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

const initialProducers = [
  {
    id: 1,
    name: "Maria Pereira",
    image: "https://media.istockphoto.com/id/1412751704/pt/foto/female-farmer-is-holding-a-digital-tablet-in-a-farm-field-smart-farming.jpg?s=1024x1024&w=is&k=20&c=tC8k5p4e8Cy0KuYlMUZ0mgDAitJ_DakUt2x74O0fDqY=",
    products: [
      { id: 1, name: "Mel", price: 10, unit: "unidade", maxQuantity: 5, availableDates: ["2024-12-15", "2024-12-22", "2024-12-29"], image: "https://media.istockphoto.com/id/1445503284/pt/foto/honey-isolated-on-white-background.jpg?s=1024x1024&w=is&k=20&c=HXFEaBl6i-gP4xLDAt4nggJPcpttMrkg_zqgniMw0JM=" },
      { id: 2, name: "Azeite", price: 5, unit: "Litro", maxQuantity: 10, availableDates: ["2024-12-16", "2024-12-23", "2024-12-30"], image: "https://media.istockphoto.com/id/643938958/pt/foto/olive-oil-bottle-and-green-olives-isolated-on-white-background.jpg?s=1024x1024&w=is&k=20&c=1EgNbkKb2mGRjEe8_NhLraNxKiA53JtBu1ld7tYrA7w=" },
      { id: 3, name: "Queijo de Cabra Curado", price: 10, unit: "kg", maxQuantity: 5, availableDates: ["2024-12-15", "2024-12-22", "2024-12-29"], image: "https://media.istockphoto.com/id/155442529/pt/foto/machego-semi-curada-queijo-de-cabra.jpg?s=1024x1024&w=is&k=20&c=4Ntl41AitW4rir0cJnWM9x_RmlsSQW_RwSmW_GR8X4g=" },
      { id: 4, name: "Cabaz de Legumes", price: 20, unit: "unidade", maxQuantity: 10, availableDates: ["2024-12-16", "2024-12-23", "2024-12-30"], image: "https://media.istockphoto.com/id/168400647/pt/foto/cesta-de-frutas-e-produtos-hortícolas.jpg?s=1024x1024&w=is&k=20&c=SaHxlNbZU57ovR_mYKDZswyDUpbly9k0S2ildGJUuqs=" }
    ]
  },
  {
    id: 2,
    name: "José carvalho",
    image: "https://media.istockphoto.com/id/1391908452/pt/foto/mature-gardener-with-box-of-onions.jpg?s=1024x1024&w=is&k=20&c=Eb7Vph85lK1tewKqg-l_W6NX6ApG0JcNvEZiK9r-V60=",
    products: [
      { id: 3, name: "Tomate Biológico", price: 3, unit: "kg", maxQuantity: 8, availableDates: ["2024-12-15", "2024-12-22", "2024-12-29"], image: "https://media.istockphoto.com/id/466175630/pt/foto/tomate-isolado-em-fundo-branco.jpg?s=612x612&w=is&k=20&c=-r3uaJ4RFUqcbwwSyFGcbRplp8Qb981oeSorQwUc0DQ=" },
      { id: 4, name: "Alface Orgânica", price: 2, unit: "unidade", maxQuantity: 12, availableDates: ["2024-12-16", "2024-12-23", "2024-12-30"], image: "https://media.istockphoto.com/id/469159458/pt/foto/alface-isolado-em-fundo-branco.jpg?s=1024x1024&w=is&k=20&c=J_Ai9Tcnvd2xl5yRbAaJxBVGdbo30wiDD2O6pSQ8Mts=" }
    ]
  }
];

function SubscriptionPage() {
  const [subscriptions, setSubscriptions] = useState({});
  const [paymentMethods, setPaymentMethods] = useState({});
  const [summary, setSummary] = useState([]);

  const handleQuantityChange = (producerId, productId, date, quantity) => {
    const parsedQuantity = parseInt(quantity) || 0;
    const producer = initialProducers.find((p) => p.id === producerId);
    const product = producer.products.find((p) => p.id === productId);

    if (parsedQuantity > product.maxQuantity) {
      alert(`A quantidade não pode exceder o máximo permitido de ${product.maxQuantity}.`);
      return;
    }

    setSubscriptions((prev) => {
      const newSubscriptions = { ...prev };
      if (!newSubscriptions[producerId]) {
        newSubscriptions[producerId] = {};
      }
      if (!newSubscriptions[producerId][productId]) {
        newSubscriptions[producerId][productId] = {};
      }
      newSubscriptions[producerId][productId][date] = parsedQuantity;
      return newSubscriptions;
    });
  };

  const handlePaymentMethodChange = (producerId, paymentMethod) => {
    setPaymentMethods((prev) => ({
      ...prev,
      [producerId]: paymentMethod
    }));
  };

  const handleSubmit = () => {
    const newSummary = [];

    Object.keys(subscriptions).forEach((producerId) => {
      const producer = initialProducers.find((p) => p.id === parseInt(producerId));
      const products = subscriptions[producerId];

      Object.keys(products).forEach((productId) => {
        const product = producer.products.find((p) => p.id === parseInt(productId));
        const dates = products[productId];

        Object.keys(dates).forEach((date) => {
          if (dates[date] > 0) {
            newSummary.push({
              date,
              producer: producer.name,
              product: product.name,
              quantity: dates[date],
              price: product.price,
              total: product.price * dates[date],
              paymentMethod: paymentMethods[producerId] || "Não Definido"
            });
          }
        });
      });
    });

    newSummary.sort((a, b) => new Date(a.date) - new Date(b.date));

    setSummary(newSummary);
  };

  return (
      <div className="container mt-4">
        <h1 className="text-center mb-4">Página de Subscrição</h1>
        <p className="lead text-center">Escolha os produtos que deseja subscrever de cada produtor:</p>

        {initialProducers.map((producer) => (
            <div key={producer.id} className="mb-4">
              <div className="d-flex align-items-center mb-3">
                <img src={producer.image} alt={producer.name} className="rounded-circle" style={{ width: '50px', height: '50px', marginRight: '15px' }} />
                <h3>{producer.name}</h3>
              </div>

              <div className="mb-3">
                <h4>Método de Pagamento</h4>
                <div className="form-check">
                  <input
                      className="form-check-input"
                      type="radio"
                      name={`paymentMethod-${producer.id}`}
                      value="Integral"
                      checked={paymentMethods[producer.id] === "Integral"}
                      onChange={() => handlePaymentMethodChange(producer.id, "Integral")}
                      id={`payment-${producer.id}-integral`}
                  />
                  <label className="form-check-label" htmlFor={`payment-${producer.id}-integral`}>
                    Integral
                  </label>
                </div>
                <div className="form-check">
                  <input
                      className="form-check-input"
                      type="radio"
                      name={`paymentMethod-${producer.id}`}
                      value="Parcelado"
                      checked={paymentMethods[producer.id] === "Parcelado"}
                      onChange={() => handlePaymentMethodChange(producer.id, "Parcelado")}
                      id={`payment-${producer.id}-parcelado`}
                  />
                  <label className="form-check-label" htmlFor={`payment-${producer.id}-parcelado`}>
                    Parcelado
                  </label>
                </div>
              </div>

              <div className="row">
                {producer.products.map((product) => (
                    <div key={product.id} className="col-md-6 col-lg-4 mb-4">
                      <div className="card">
                        <img src={product.image} className="card-img-top" alt={product.name} />
                        <div className="card-body">
                          <h5 className="card-title">{product.name}</h5>
                          <p className="card-text">Preço: {product.price}€/{product.unit}</p>
                          <p className="card-text">Unidade Máxima: {product.maxQuantity}</p>

                          <div className="mb-3">
                            <label className="form-label">Datas e Quantidades:</label>
                            <div>
                              {product.availableDates.map((date) => (
                                  <div key={date} className="mb-2">
                                    <label className="form-label">{date}:</label>
                                    <input
                                        type="number"
                                        className="form-control"
                                        max={product.maxQuantity}
                                        placeholder={`Máx: ${product.maxQuantity}`}
                                        value={subscriptions[producer.id]?.[product.id]?.[date] || ""}
                                        onChange={(e) =>
                                            handleQuantityChange(producer.id, product.id, date, e.target.value)
                                        }
                                    />
                                  </div>
                              ))}
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                ))}
              </div>
            </div>
        ))}

        <button className="btn btn-success btn-lg mb-4" onClick={handleSubmit}>Finalizar Subscrição</button>

        {summary.length > 0 && (
            <div>
              <h2 className="mb-4">Resumo da Subscrição</h2>
              {summary.reduce((acc, item) => {
                    const date = item.date;
                    if (!acc[date]) {
                      acc[date] = [];
                    }
                    acc[date].push(item);
                    return acc;
                  }, {})

                  && Object.entries(summary.reduce((acc, item) => {
                    const date = item.date;
                    if (!acc[date]) {
                      acc[date] = [];
                    }
                    acc[date].push(item);
                    return acc;
                  }, {})).map(([date, items]) => (
                      <div key={date} className="mb-4">
                        <h4 className="bg-light p-2">Data de Recolha: {date}</h4>
                        <table className="table table-bordered table-striped">
                          <thead>
                          <tr>
                            <th>Produtor</th>
                            <th>Produto</th>
                            <th>Quantidade</th>
                            <th>Preço Unitário (€)</th>
                            <th>Total (€)</th>
                            <th>Método de Pagamento</th>
                          </tr>
                          </thead>
                          <tbody>
                          {items.map((item, index) => (
                              <tr key={index}>
                                <td>{item.producer}</td>
                                <td>{item.product}</td>
                                <td>{item.quantity}</td>
                                <td>{item.price}</td>
                                <td>{item.total}</td>
                                <td>{item.paymentMethod}</td>
                              </tr>
                          ))}
                          <tr className="table-success">
                            <td colSpan="4"><strong>Total do dia:</strong></td>
                            <td colSpan="2">
                              <strong>{items.reduce((acc, item) => acc + item.total, 0).toFixed(2)}€</strong>
                            </td>
                          </tr>
                          </tbody>
                        </table>
                      </div>
                  ))}
              <div className="card mt-4 bg-light">
                <div className="card-body">
                  <h5 className="card-title">Total Geral da Subscrição</h5>
                  <p className="card-text h4">
                    {summary.reduce((acc, item) => acc + item.total, 0).toFixed(2)}€
                  </p>
                </div>
              </div>
            </div>
        )}
      </div>
  );
}

export default SubscriptionPage;
