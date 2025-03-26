import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Table, Modal } from 'react-bootstrap';

const CalculationPage = () => {
    // Dados mockados
    const mockSubscriptions = [
        { coproducerId: 1, producerName: 'Fazenda Verde', startDate: '2024-01-01', endDate: '2024-06-30', value: 50 },
        { coproducerId: 1, producerName: 'Horta Alegre', startDate: '2024-01-01', endDate: '2024-06-30', value: 30 },
        { coproducerId: 2, producerName: 'Fazenda Verde', startDate: '2024-01-01', endDate: '2024-06-30', value: 70 },
        { coproducerId: 2, producerName: 'Pomar Encantado', startDate: '2024-01-01', endDate: '2024-06-30', value: 40 },
        { coproducerId: 3, producerName: 'Horta Alegre', startDate: '2024-01-01', endDate: '2024-06-30', value: 45 },
        { coproducerId: 3, producerName: 'Pomar Encantado', startDate: '2024-01-01', endDate: '2024-06-30', value: 55 },
    ];

    const mockPayments = [
        { coproducerId: 1, amount: 80, paymentDate: '2024-01-10' },
        { coproducerId: 2, amount: 110, paymentDate: '2024-01-12' },
        { coproducerId: 3, amount: 100, paymentDate: '2024-01-14' },
    ];

    // Estados
    const [totalPaid, setTotalPaid] = useState(0);
    const [totalValue, setTotalValue] = useState(0);
    const [coproducerPayments, setCoproducerPayments] = useState([]);
    const [distribution, setDistribution] = useState([]);
    const [selectedInfo, setSelectedInfo] = useState(null);
    const [showModal, setShowModal] = useState(false);

    useEffect(() => {
        const totalPaid = mockPayments.reduce((total, payment) => total + payment.amount, 0);
        const totalValue = mockSubscriptions.reduce((total, subscription) => total + subscription.value, 0);

        // Processar pagamentos de cada co-produtor
        const paymentsByCoproducer = mockPayments.map(payment => {
            const subscriptionsForCoproducer = mockSubscriptions.filter(
                subscription => subscription.coproducerId === payment.coproducerId
            );
            const totalSubscriptionValue = subscriptionsForCoproducer.reduce((sum, sub) => sum + sub.value, 0);
            return {
                coproducerId: payment.coproducerId,
                totalPaid: payment.amount,
                totalSubscriptionValue,
            };
        });

        // Distribuição proporcional entre os produtores (com base nas assinaturas)
        const producersDistribution = paymentsByCoproducer.map(payment => {
            return {
                coproducerId: payment.coproducerId,
                distribution: mockSubscriptions.map(subscription => {
                    if (subscription.coproducerId === payment.coproducerId) {
                        const producerShare = (subscription.value / payment.totalSubscriptionValue) * payment.totalPaid;
                        return {
                            producerName: subscription.producerName,
                            valueToReceive: producerShare,
                        };
                    }
                    return null;
                }).filter(Boolean),
            };
        });

        setTotalPaid(totalPaid);
        setTotalValue(totalValue);
        setCoproducerPayments(paymentsByCoproducer);
        setDistribution(producersDistribution);
    }, []);

    // Função para abrir o modal
    const handleRowClick = (coproducerId) => {
        const selectedPayment = coproducerPayments.find(payment => payment.coproducerId === coproducerId);
        const selectedSubscriptions = mockSubscriptions.filter(sub => sub.coproducerId === coproducerId);

        const message = selectedSubscriptions.map(sub => {
            return `O Co-produtor ${coproducerId} comprou do ${sub.producerName} um total de €${sub.value}.`;
        }).join("\n");

        setSelectedInfo(`Pagamentos totais: €${selectedPayment.totalPaid}\n\n${message}`);
        setShowModal(true);
    };

    const handleCloseModal = () => setShowModal(false);

    return (
        <Container>
            <h1 className="my-4" style={{ color: '#2E8B57' }}>Cálculo e Distribuição de Pagamentos</h1>

            <Row className="mb-4">
                <Col md={6}>
                    <Card className="text-center" bg="success" text="white">
                        <Card.Body>
                            <Card.Title>Total Pago pelos Coprodutores</Card.Title>
                            <Card.Text>€{totalPaid || 0}</Card.Text>
                        </Card.Body>
                    </Card>
                </Col>
                <Col md={6}>
                    <Card className="text-center" bg="primary" text="white">
                        <Card.Body>
                            <Card.Title>Total a Receber pelos Produtores</Card.Title>
                            <Card.Text>€{totalValue || 0}</Card.Text>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>

            <h3 className="my-4" style={{ color: '#2E8B57' }}>Pagamentos dos Coprodutores</h3>
            <Table striped bordered hover>
                <thead>
                <tr>
                    <th>Coprodutor</th>
                    <th>Valor Pago</th>
                    <th>Valor das Assinaturas</th>
                </tr>
                </thead>
                <tbody>
                {coproducerPayments.map((payment, index) => (
                    <tr key={index} onClick={() => handleRowClick(payment.coproducerId)}>
                        <td>Coprodutor {payment.coproducerId}</td>
                        <td>€{payment.totalPaid.toFixed(2)}</td>
                        <td>€{payment.totalSubscriptionValue.toFixed(2)}</td>
                    </tr>
                ))}
                </tbody>
            </Table>

            <h3 className="my-4" style={{ color: '#2E8B57' }}>Distribuição entre os Produtores</h3>
            <Table striped bordered hover>
                <thead>
                <tr>
                    <th>Coprodutor</th>
                    <th>Produtor</th>
                    <th>Valor a Receber</th>
                </tr>
                </thead>
                <tbody>
                {distribution.map((payment, index) => (
                    payment.distribution.map((distribution, subIndex) => (
                        <tr key={`${index}-${subIndex}`}>
                            <td>Coprodutor {payment.coproducerId}</td>
                            <td>{distribution.producerName}</td>
                            <td>€{distribution.valueToReceive.toFixed(2)}</td>
                        </tr>
                    ))
                ))}
                </tbody>
            </Table>

            <Button variant="success" className="mt-4" onClick={() => alert("Cálculos Recalculados!")}>
                Recalcular
            </Button>

            {/* Modal para mostrar informações */}
            <Modal show={showModal} onHide={handleCloseModal}>
                <Modal.Header closeButton>
                    <Modal.Title>Detalhes da Transação</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <pre>{selectedInfo}</pre>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseModal}>
                        Fechar
                    </Button>
                </Modal.Footer>
            </Modal>
        </Container>
    );
};

export default CalculationPage;
