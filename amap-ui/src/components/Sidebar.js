import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PropTypes from 'prop-types';
import {
    FaBoxOpen,
    FaClipboardList,
    FaGift,
    FaChartLine,
    FaCarrot,
    FaShoppingBasket,
    FaRegCalendarCheck, FaCalculator, FaList, FaRegCheckSquare, FaAngellist
} from 'react-icons/fa';

const Sidebar = ({ isVisible }) => {
    const userType = sessionStorage.getItem('userType'); // Obter o tipo de usuário (Manager, Producer, CoProducer)

    console.log("userType:", userType); // Adicionei para verificar se o valor está correto

    // Itens do menu com roles específicas
    const menuItems = [

        //{ path: "/producerCatalogo", label: "Ofertas para coprodutores", icon: <FaCarrot />, roles: ["CoProducer"] },
        { path: "/coProducerMainScreen", label: "Subscrições para coprodutores", icon: <FaShoppingBasket />, roles: ["CoProducer"] },

        { path: "/producerCatalogo", label: "Catálogo de Produtos", icon: <FaAngellist />, roles: ["Producer"] },
        { path: "/producerOrders", label: "Catálogo de Produtos", icon: <FaRegCheckSquare />, roles: ["Producer"] },
        { path: "/producerOffer", label: "Ofertas de Produtos", icon: <FaList />, roles: ["Producer"] },

        { path: "/amapSubscriptionPeriod", label: "Criação de Subscrições da amap", icon: <FaRegCalendarCheck />, roles: ["Manager"] },
        { path: "/amapOrders", label: "Períodos de Subscrição", icon: <FaClipboardList />, roles: ["Manager"] },
        { path: "/amapKPI", label: "KPIs", icon: <FaChartLine />, roles: ["Manager"] },
        { path: "/amapCalculationPage", label: "Página de cálculos da amap", icon: <FaCalculator />, roles: ["Manager"] },
    ];

    // Filtrar itens baseados no tipo de usuário
    const filteredMenuItems = menuItems.filter((item) => item.roles.includes(userType));

    console.log("Filtered Menu Items:", filteredMenuItems); // Verificar quais itens estão a ser filtrados

    const navigate = useNavigate(); // Hook para navegação

    const handleLogout = async () => {
        try {
            const response = await fetch('https://localhost:7262/api/Account/logout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include', // Para enviar o cookie de autenticação
            });

            if (response.ok) {
                sessionStorage.clear(); // Limpa o sessionStorage
                localStorage.clear(); // Limpa também o localStorage para garantir
                navigate('/'); // Redireciona para a página de login
            } else {
                console.error('Logout failed on server:', response.statusText);
            }
        } catch (error) {
            console.error('Error during logout:', error);
        }
    };

    return (
        isVisible && (
            <aside
                style={{
                    width: '240px',
                    backgroundColor: '#8B4513',
                    color: '#fff',
                    padding: '20px',
                    height: '800vh',
                    boxShadow: '2px 0 5px rgba(0, 0, 0, 0.1)',
                    position: 'relative',
                    transition: 'width 0.3s ease',
                }}
            >
                <h3 style={{ color: '#fff', marginBottom: '20px', fontSize: '24px' }}>Dashboard</h3>
                <nav>
                    <ul style={{ listStyle: 'none', padding: 0 }}>
                        {filteredMenuItems.length > 0 ? (
                            filteredMenuItems.map((item) => (
                                <li key={item.path} style={{ margin: '20px 0', display: 'flex', alignItems: 'center' }}>
                                    {item.icon}
                                    <Link
                                        to={item.path}
                                        style={{
                                            color: '#fff',
                                            textDecoration: 'none',
                                            fontSize: '18px',
                                            marginLeft: '10px',
                                        }}
                                    >
                                        {item.label}
                                    </Link>
                                </li>
                            ))
                        ) : (
                            <li style={{ color: '#fff', fontSize: '18px', marginTop: '20px' }}>
                                Nenhuma opção disponível para o seu perfil.
                            </li>
                        )}
                    </ul>
                    <button
                        onClick={handleLogout}
                        style={{
                            marginTop: '30px',
                            padding: '10px 20px',
                            backgroundColor: '#a0522d',
                            color: '#fff',
                            border: 'none',
                            borderRadius: '5px',
                            cursor: 'pointer',
                            fontSize: '16px',
                            width: '100%',
                            textAlign: 'left',
                        }}
                    >
                        Logout
                    </button>
                </nav>
            </aside>
        )
    );
};

Sidebar.propTypes = {
    isVisible: PropTypes.bool.isRequired,
};

export default Sidebar;
