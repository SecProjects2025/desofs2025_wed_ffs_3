import React, { useState } from 'react';
import { Container, Row, Col, Form, Button, Table } from 'react-bootstrap';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const AMAPSubscriptionPage = () => {
    const [subscriptions, setSubscriptions] = useState([
        {
            id: 1,
            name: 'Subscrição Semestral - Primavera',
            startDate: new Date('2024-03-01'),
            endDate: new Date('2024-08-31'),
            deliveryDates: [
                new Date('2024-03-10'),
                new Date('2024-04-10'),
                new Date('2024-05-10'),
            ],
        },
        {
            id: 2,
            name: 'Subscrição Trimestral - Outono',
            startDate: new Date('2024-09-01'),
            endDate: new Date('2024-11-30'),
            deliveryDates: [
                new Date('2024-09-15'),
                new Date('2024-10-15'),
                new Date('2024-11-15'),
            ],
        },
    ]);

    const [newSubscription, setNewSubscription] = useState({
        name: '',
        startDate: null,
        endDate: null,
        deliveryDates: [],
    });

    const [editingSubscription, setEditingSubscription] = useState(null); // Subscrição em edição
    const [deliveryDate, setDeliveryDate] = useState(null);
    const [message, setMessage] = useState({ type: '', text: '' });

    // Função para adicionar ou editar subscrição
    const handleSaveSubscription = () => {
        if (
            !newSubscription.name ||
            !newSubscription.startDate ||
            !newSubscription.endDate ||
            newSubscription.deliveryDates.length === 0
        ) {
            setMessage({ type: 'error', text: 'Preencha todos os campos!' });
            return;
        }

        if (newSubscription.endDate <= newSubscription.startDate) {
            setMessage({ type: 'error', text: 'A data de fim deve ser posterior à data de início!' });
            return;
        }

        if (editingSubscription) {
            setSubscriptions((prev) =>
                prev.map((sub) =>
                    sub.id === editingSubscription.id ? { ...newSubscription, id: sub.id } : sub
                )
            );
            setMessage({ type: 'success', text: 'Subscrição editada com sucesso!' });
        } else {
            setSubscriptions([...subscriptions, { ...newSubscription, id: Date.now() }]);
            setMessage({ type: 'success', text: 'Subscrição adicionada com sucesso!' });
        }

        setNewSubscription({ name: '', startDate: null, endDate: null, deliveryDates: [] });
        setEditingSubscription(null);
    };

    // Função para excluir subscrição
    const handleDeleteSubscription = (id) => {
        setSubscriptions(subscriptions.filter((sub) => sub.id !== id));
        setMessage({ type: 'info', text: 'Subscrição excluída!' });
    };

    // Função para editar subscrição
    const handleEditSubscription = (subscription) => {
        setNewSubscription(subscription);
        setEditingSubscription(subscription);
    };

    // Função para adicionar data de entrega
    const handleAddDeliveryDate = () => {
        if (!deliveryDate) {
            setMessage({ type: 'error', text: 'Selecione uma data de entrega válida!' });
            return;
        }

        if (
            newSubscription.deliveryDates.some(
                (date) => date.toDateString() === deliveryDate.toDateString()
            )
        ) {
            setMessage({ type: 'info', text: 'Essa data de entrega já foi adicionada!' });
            return;
        }

        setNewSubscription((prev) => ({
            ...prev,
            deliveryDates: [...prev.deliveryDates, deliveryDate],
        }));
        setDeliveryDate(null);
    };

    return (
        <Container>
            <h1 style={{ color: '#2E8B57', marginBottom: '20px' }}>Criação de Períodos de Subscrição</h1>

            {message.text && (
                <div style={{ color: message.type === 'error' ? 'red' : 'green', marginBottom: '20px' }}>
                    {message.text}
                </div>
            )}

            <Row>
                <Col md={6}>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Nome do Período de Subscrição</Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="Exemplo: Subscrição Semestral"
                                value={newSubscription.name}
                                onChange={(e) =>
                                    setNewSubscription({ ...newSubscription, name: e.target.value })
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
                                dateFormat="dd/MM/yyyy"
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
                                dateFormat="dd/MM/yyyy"
                                className="form-control"
                            />
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label>Adicionar Data de Entrega</Form.Label>
                            <div className="d-flex">
                                <DatePicker
                                    selected={deliveryDate}
                                    onChange={(date) => setDeliveryDate(date)}
                                    dateFormat="dd/MM/yyyy"
                                    className="form-control"
                                />
                                <Button
                                    variant="success"
                                    onClick={handleAddDeliveryDate}
                                    className="ms-2"
                                >
                                    Adicionar
                                </Button>
                            </div>
                        </Form.Group>

                        <ul>
                            {newSubscription.deliveryDates.map((date, index) => (
                                <li key={index}>{date.toLocaleDateString()}</li>
                            ))}
                        </ul>

                        <Button variant="success" onClick={handleSaveSubscription}>
                            {editingSubscription ? 'Editar Subscrição' : 'Salvar Subscrição'}
                        </Button>
                    </Form>
                </Col>
            </Row>

            <Row>
                <Col md={12} className="mt-5">
                    <h2>Períodos Criados</h2>
                    <Table striped bordered hover>
                        <thead>
                        <tr>
                            <th>Nome</th>
                            <th>Data de Início</th>
                            <th>Data de Fim</th>
                            <th>Datas de Entrega</th>
                            <th>Ações</th>
                        </tr>
                        </thead>
                        <tbody>
                        {subscriptions.map((sub) => (
                            <tr key={sub.id}>
                                <td>{sub.name}</td>
                                <td>{sub.startDate.toLocaleDateString()}</td>
                                <td>{sub.endDate.toLocaleDateString()}</td>
                                <td>
                                    {sub.deliveryDates.map((date, index) => (
                                        <span key={index}>
                                                {date.toLocaleDateString()}
                                            {index < sub.deliveryDates.length - 1 && ', '}
                                            </span>
                                    ))}
                                </td>
                                <td>
                                    <Button
                                        variant="success"
                                        className="me-2"
                                        onClick={() => handleEditSubscription(sub)}
                                    >
                                        Editar
                                    </Button>
                                    <Button
                                        variant="danger"
                                        onClick={() => handleDeleteSubscription(sub.id)}
                                    >
                                        Excluir
                                    </Button>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </Table>
                </Col>
            </Row>
        </Container>
    );
};

export default AMAPSubscriptionPage;
