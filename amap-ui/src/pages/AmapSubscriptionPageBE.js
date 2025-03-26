import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Table } from "react-bootstrap";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { createSubscriptionPeriod, deleteSubscriptionPeriod, getSubscriptionPeriodById } from "../service/subscriptionService";
import { ToastContainer, toast } from "react-toastify"; // Importando Toast
import "react-toastify/dist/ReactToastify.css"; // Importando o estilo do Toast

const AMAPSubscriptionPage = () => {
  const [subscriptions, setSubscriptions] = useState([]);
  const [newSubscription, setNewSubscription] = useState({
    duration: "",
    startDate: null,
    endDate: null,
    deliveryDates: [],
  });
  const [deliveryDate, setDeliveryDate] = useState(null);

  useEffect(() => {
    // Função para obter subscrições
    const fetchSubscriptions = async () => {
      try {
        const result = await getSubscriptionPeriodById(1); // Exemplo de ID
        setSubscriptions([result]);
      } catch (error) {
        // Não exibe nenhuma mensagem de erro para o usuário
        console.error('Erro ao buscar período de subscrição');
      }
    };

    fetchSubscriptions();
  }, []);

  const handleAddSubscription = async () => {
    try {
      // Verificar se há datas de entrega
      if (newSubscription.deliveryDates.length === 0) {
        // Mensagem de erro sem exibição na interface
        console.error("É necessário adicionar pelo menos uma data de entrega!");
        return; // Impede o envio se não houver datas de entrega
      }

      // Formatar os dados para o formato correto
      const formattedSubscription = {
        duration: newSubscription.duration,
        startDate: newSubscription.startDate.toISOString(),
        endDate: newSubscription.endDate.toISOString(),
        deliveryDates: newSubscription.deliveryDates.map((date) => ({
          date: date.toISOString(),
        })),
      };

      // Criar a subscrição
      const created = await createSubscriptionPeriod(formattedSubscription);
      setSubscriptions([...subscriptions, created]);
      setNewSubscription({
        duration: "",
        startDate: null,
        endDate: null,
        deliveryDates: [],
      });

      // Mensagem de sucesso com envio de email
      toast.success("Período de subscrição adicionado com sucesso! Um e-mail foi enviado aos produtores.");
    } catch (error) {
      // Nenhuma mensagem de erro é exibida
      console.error('Erro ao criar período de subscrição');
    }
  };

  const handleDeleteSubscription = async (id) => {
    try {
      await deleteSubscriptionPeriod(id);
      setSubscriptions(subscriptions.filter((sub) => sub.id !== id));

      // Mensagem de sucesso com envio de email
      toast.info("Período de subscrição eliminado! Um e-mail foi enviado aos produtores.");
    } catch (error) {
      // Nenhuma mensagem de erro é exibida
      console.error('Erro ao excluir período de subscrição');
    }
  };

  const handleAddDeliveryDate = () => {
    if (!deliveryDate) {
      toast.error("Selecione uma data de entrega válida!"); // Mensagem de erro
      return;
    }

    // Verificar se a data de entrega está dentro do período de início e fim
    if (deliveryDate < newSubscription.startDate || deliveryDate > newSubscription.endDate) {
      toast.error("A data de entrega está fora do período. Selecione uma data dentro do intervalo de início e fim."); // Mensagem de erro
      return;
    }

    setNewSubscription((prev) => ({
      ...prev,
      deliveryDates: [...prev.deliveryDates, deliveryDate],
    }));
    setDeliveryDate(null);
    toast.success("Data de entrega adicionada!"); // Mensagem de sucesso
  };

  return (
    <Container>
      <h1>Gestão de Períodos de Subscrição</h1>

      <Row>
        <Col md={6}>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Nome do Período</Form.Label>
              <Form.Control
                type="text"
                value={newSubscription.duration}
                onChange={(e) =>
                  setNewSubscription({
                    ...newSubscription,
                    duration: e.target.value,
                  })
                }
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Data de Início</Form.Label>
              <DatePicker
                selected={newSubscription.startDate}
                onChange={(date) =>
                  setNewSubscription({ ...newSubscription, startDate: date })
                }
                className="form-control"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Data de Fim</Form.Label>
              <DatePicker
                selected={newSubscription.endDate}
                onChange={(date) =>
                  setNewSubscription({ ...newSubscription, endDate: date })
                }
                className="form-control"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Data de Entrega</Form.Label>
              <div>
                <DatePicker
                  selected={deliveryDate}
                  onChange={(date) => setDeliveryDate(date)}
                  className="form-control"
                />
                <Button
                  variant="success"
                  onClick={handleAddDeliveryDate}
                  className="mt-2"
                >
                  Adicionar
                </Button>
              </div>
            </Form.Group>
            <Button variant="primary" onClick={handleAddSubscription}>
              Criar Subscrição
            </Button>
          </Form>
        </Col>
      </Row>

      {/* Exibindo as datas de entrega abaixo */}
      {newSubscription.deliveryDates.length > 0 && (
        <Row className="mt-4">
          <Col md={12}>
            <h3>Datas de Entrega Selecionadas</h3>
            <ul>
              {newSubscription.deliveryDates.map((date, idx) => (
                <li key={idx}>{new Date(date).toLocaleDateString()}</li>
              ))}
            </ul>
          </Col>
        </Row>
      )}

      <Row>
        <Col md={12} className="mt-4">
          <h2>Lista de Subscrições</h2>
          <Table striped bordered>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Início</th>
                <th>Fim</th>
                <th>Entregas</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {subscriptions.map((sub) => (
                <tr key={sub.id}>
                  <td>{sub.duration}</td>
                  <td>{new Date(sub.startDate).toLocaleDateString()}</td>
                  <td>{new Date(sub.endDate).toLocaleDateString()}</td>
                  <td>
                    {sub.deliveryDates.map((date, idx) => (
                      <span key={idx}>
                        {new Date(date.date).toLocaleDateString()}
                        {idx < sub.deliveryDates.length - 1 ? ", " : ""}
                      </span>
                    ))}
                  </td>

                  <td>
                    <Button
                      variant="danger"
                      onClick={() => handleDeleteSubscription(sub.id)}
                    >
                      Eliminar
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </Col>
      </Row>

      {/* ToastContainer para exibir as notificações */}
      <ToastContainer position="top-right" autoClose={5000} hideProgressBar />
    </Container>
  );
};

export default AMAPSubscriptionPage;
