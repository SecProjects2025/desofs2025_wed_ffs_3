import React, { useState } from 'react';
import { Container, Form, Button, Row, Col, Card } from 'react-bootstrap';
import axios from 'axios'; // Biblioteca para realizar requisições HTTP
axios.defaults.withCredentials = true;

const AuthPage = () => {
    const [formData, setFormData] = useState({
        email: '',
        password: '',
    });
    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');


    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value,
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await axios.post('/api/account/login', formData, {
                withCredentials: true, // Garante que o cookie seja enviado e armazenado
            });

            const { message, userId, role } = response.data;

            // Armazenar o papel do user com a chave correta
            
            localStorage.setItem('userType', role); // Alterado para 'userType'
            localStorage.setItem('userId', userId);
            // Mensagem de sucesso amigável
            setSuccessMessage(message);


            // Exemplo: Redirecionar com base no papel
            if (role === 'Producer') {
                window.location.href = '/catalogo'; // Ajuste para a sua rota
            } else if (role === 'Manager') {
                window.location.href = '/calculationPage';
            } else if (role === 'CoProducer') {
                window.location.href = '/subscription';
            }
            setErrorMessage('');
        } catch (error) {
            if (error.response) {
                setErrorMessage(error.response.data.message || 'Erro ao fazer login.');
            } else {
                setErrorMessage('Erro ao conectar ao servidor.');
            }
            setSuccessMessage('');
        }
    };
    
    return (
        <Container>
            <Row className="justify-content-center mt-5">
                <Col md={6}>
                    <Card>
                        <Card.Body>
                            <h2 className="text-center">Login</h2>
                            {errorMessage && <p className="text-danger">{errorMessage}</p>}
                            {successMessage && <p className="text-success">{successMessage}</p>}
                            <Form onSubmit={handleSubmit}>
                                <Form.Group controlId="formEmail">
                                    <Form.Label>Email</Form.Label>
                                    <Form.Control
                                        type="email"
                                        name="email"
                                        value={formData.email}
                                        onChange={handleChange}
                                        placeholder="Digite seu email"
                                        required
                                    />
                                </Form.Group>
                                <Form.Group controlId="formPassword">
                                    <Form.Label>Senha</Form.Label>
                                    <Form.Control
                                        type="password"
                                        name="password"
                                        value={formData.password}
                                        onChange={handleChange}
                                        placeholder="Digite sua palavra-passe"
                                        required
                                    />
                                </Form.Group>
                                <Button variant="primary" type="submit" block>
                                    Entrar
                                </Button>
                            </Form>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
};

export default AuthPage;
