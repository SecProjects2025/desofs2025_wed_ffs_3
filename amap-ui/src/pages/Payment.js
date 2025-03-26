import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Card, Table, Button, Alert } from 'react-bootstrap';

const PaymentCalculations = () => {
    const [producers, setProducers] = useState([]);
    const [coProducers, setCoProducers] = useState([]);
    const [error, setError] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    const fetchProducers = async () => {
        try {
            const response = await fetch('https://localhost:7262/api/PaymentsInfo/GetProducerToReceive', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                },
                credentials: 'include',
            });
    
            if (!response.ok) {
                const errorData = await response.json();  // Tenta pegar mais detalhes da resposta de erro
                console.error('Erro de autenticação:', errorData);
                throw new Error('Erro ao carregar dados dos produtores');
            }
    
            const data = await response.json();
            setProducers(Object.entries(data));
        } catch (err) {
            setError(err.message);
        }
    };
    

    // Função para buscar dados dos coprodutores
    const fetchCoProducers = async () => {
        try {
            // Fazendo a requisição com credenciais (cookies) incluídas
            const response = await fetch('https://localhost:7262/api/PaymentsInfo/GetCoProducerToPay', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                },
                credentials: 'include', // Inclui os cookies de sessão
            });
            if (!response.ok) {
                throw new Error('Erro ao carregar dados dos coprodutores');
            }
            const data = await response.json();
            setCoProducers(Object.entries(data));
        } catch (err) {
            setError(err.message);
        }
    };

    // Fetch dos dados na montagem do componente
    useEffect(() => {
        const fetchData = async () => {
            setIsLoading(true);
            await Promise.all([fetchProducers(), fetchCoProducers()]);
            setIsLoading(false);
        };
        fetchData();
    }, []);

    return (
        <Container>
            <h1 className="my-4" style={{ color: '#2E8B57' }}>Cálculos de Pagamentos</h1>

            {error && <Alert variant="danger">{error}</Alert>}

            {isLoading ? (
                <p>A carregar dados...</p>
            ) : (
                <>
                    {/* Valores dos produtores */}
                    <h3 className="mt-4" style={{ color: '#2E8B57' }}>Valores a Receber pelos Produtores</h3>
                    <Table striped bordered hover>
                        <thead>
                            <tr>
                                <th>Produtor (ID)</th>
                                <th>Valor a Receber (€)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {producers.map(([id, value]) => (
                                <tr key={id}>
                                    <td>{id}</td>
                                    <td>€{value.toFixed(2)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>

                    {/* Valores dos coprodutores */}
                    <h3 className="mt-4" style={{ color: '#2E8B57' }}>Valores a Pagar pelos Coprodutores</h3>
                    <Table striped bordered hover>
                        <thead>
                            <tr>
                                <th>Coprodutor (ID)</th>
                                <th>Valor a Pagar (€)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {coProducers.map(([id, value]) => (
                                <tr key={id}>
                                    <td>{id}</td>
                                    <td>€{value.toFixed(2)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>

                    <Button variant="success" className="mt-4" onClick={() => window.location.reload()}>
                        Atualizar Dados
                    </Button>
                </>
            )}
        </Container>
    );
};

export default PaymentCalculations;
