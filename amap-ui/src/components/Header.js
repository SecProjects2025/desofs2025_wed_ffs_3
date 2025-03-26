import React, { useState } from 'react';
import { FaBars, FaTimes, FaUser } from 'react-icons/fa';  // Ícone de usuário adicionado
import { Button, Modal } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';  // Importando o hook useNavigate
import 'bootstrap/dist/css/bootstrap.min.css';

const Header = ({ isVisible, toggleSidebar }) => {
    const [showMessage, setShowMessage] = useState(false);  // Estado para mostrar ou esconder a mensagem
    const navigate = useNavigate();  // Hook para navegação entre páginas

    // Função que exibe a mensagem de login
    const handleUserIconClick = () => {
        setShowMessage(true);  // Exibe o modal
    };

    const handleCloseMessage = () => {
        setShowMessage(false);  // Fecha o modal
    };

    const handleLogin = () => {
        // Redirecionando para a página de login
        navigate('/login');
        setShowMessage(false);  // Fecha o modal
    };

    return (
        <header
            style={{
                backgroundColor: '#2E8B57',
                color: '#fff',
                padding: '10px 20px',
                boxShadow: '0px 2px 5px rgba(0, 0, 0, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between', // Espaça o conteúdo para as extremidades
            }}
        >
            {/* Botão para abrir/fechar o sidebar à esquerda */}
            <button
                onClick={toggleSidebar}
                style={{
                    backgroundColor: 'transparent', // Remover fundo
                    color: '#fff', // Cor do ícone
                    border: 'none', // Remover borda
                    cursor: 'pointer',
                    fontSize: '24px', // Ajustar o tamanho do ícone
                    padding: '0', // Remover padding
                    marginRight: '20px', // Espaço entre o botão e o título
                }}
            >
                {isVisible ? <FaTimes /> : <FaBars />} {/* Ícone para abrir/fechar */}
            </button>

            {/* Título do Dashboard */}
            <h1 style={{ margin: 0 }}>AMAP Dashboard</h1>

            {/* Ícone de usuário à direita */}
            <FaUser
                size={30}
                style={{ cursor: 'pointer' }}
                onClick={handleUserIconClick}  // Ao clicar no ícone, mostra o modal
            />

            {/* Modal de login */}
            <Modal show={showMessage} onHide={handleCloseMessage}>
                <Modal.Header closeButton>
                    <Modal.Title>Deseja fazer login?</Modal.Title>
                </Modal.Header>
                <Modal.Body>É preciso entrar na plataforma para ter acesso a esta área.\n Deseja fazer login?</Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseMessage}>
                        Cancelar
                    </Button>
                    <Button variant="primary" onClick={handleLogin}>
                        Fazer login
                    </Button>
                </Modal.Footer>
            </Modal>
        </header>
    );
};

export default Header;
