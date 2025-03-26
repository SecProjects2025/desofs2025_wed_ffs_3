import React, { useState } from 'react';
import '../App.css';
import jsPDF from 'jspdf';

const ProducerOrders = () => {
    const [entregas, setEntregas] = useState([
        {
            id: 1,
            dataEntrega: '08/12/2024',
            produtos: [
                {
                    nome: 'Alface',
                    quantidadeSubscrita: 150,
                    quantidadeEntregue: 145,
                    estado: 'pendente'
                },
                {
                    nome: 'Tomate',
                    quantidadeSubscrita: 200,
                    quantidadeEntregue: 180,
                    estado: 'pendente'
                }
            ]
        },
        {
            id: 2,
            dataEntrega: '15/12/2024',
            produtos: [
                {
                    nome: 'Cenouras',
                    quantidadeSubscrita: 300,
                    quantidadeEntregue: 300,
                    estado: 'pendente'
                },
                {
                    nome: 'Batatas',
                    quantidadeSubscrita: 400,
                    quantidadeEntregue: 380,
                    estado: 'pendente'
                }
            ]
        }
    ]);

    // Função para editar a quantidade entregue
    const handleQuantidadeChange = (entregaId, produtoNome, novaQuantidade) => {
        setEntregas(entregas.map(entrega => {
            if (entrega.id === entregaId) {
                return {
                    ...entrega,
                    produtos: entrega.produtos.map(produto => {
                        if (produto.nome === produtoNome) {
                            return {
                                ...produto,
                                quantidadeEntregue: novaQuantidade,
                                estado: novaQuantidade !== produto.quantidadeSubscrita ? 'alterado' : 'pendente'
                            };
                        }
                        return produto;
                    })
                };
            }
            return entrega;
        }));
    };

    // Função para confirmar uma entrega
    const confirmarEntrega = (entregaId) => {
        setEntregas(entregas.map(entrega => {
            if (entrega.id === entregaId) {
                return {
                    ...entrega,
                    produtos: entrega.produtos.map(produto => ({
                        ...produto,
                        estado: 'confirmado'
                    }))
                };
            }
            return entrega;
        }));
    };

    // Função para notificar alterações
    const notificarAlteracao = (entrega) => {
        const produtosAlterados = entrega.produtos.filter(p => p.estado === 'alterado');

        const mensagem = produtosAlterados.map(p =>
            `${p.nome}: ${p.quantidadeSubscrita} → ${p.quantidadeEntregue}`
        ).join('\n');

        alert(`Notificação enviada sobre alterações:\n${mensagem}`);
    };

    // Função para gerar o relatório em PDF
    const gerarRelatorioPDF = () => {
        const doc = new jsPDF();
        doc.setFontSize(18);
        doc.text("Relatório de Entregas", 14, 22);

        let y = 30;
        entregas.forEach((entrega) => {
            doc.setFontSize(14);
            doc.text(`Data de Entrega: ${entrega.dataEntrega}`, 14, y);
            y += 10;

            entrega.produtos.forEach((produto) => {
                doc.text(`Produto: ${produto.nome}`, 14, y);
                doc.text(`Quantidade Subscrita: ${produto.quantidadeSubscrita}`, 14, y + 10);
                doc.text(`Quantidade Entregue: ${produto.quantidadeEntregue}`, 14, y + 20);
                doc.text(`Estado: ${produto.estado}`, 14, y + 30);
                y += 40;
            });

            y += 10;
        });

        doc.save('relatorio_entregas.pdf');
    };

    return (
        <div className="container mt-4">
            <h1 className="text-center mb-4">Gestão de Entregas</h1>

            {entregas.map((entrega) => (
                <div key={entrega.id} className="card mb-4 shadow-sm">
                    <div className="card-header bg-success text-white">
                        <h3 className="mb-0">Data de Entrega: {entrega.dataEntrega}</h3>
                    </div>

                    <div className="card-body">
                        <div className="table-responsive">
                            <table className="table table-hover">
                                <thead className="table-light">
                                <tr>
                                    <th>Produto</th>
                                    <th>Quantidade Subscrita</th>
                                    <th>Quantidade a Entregar</th>
                                    <th>Diferença</th>
                                    <th>Estado</th>
                                </tr>
                                </thead>
                                <tbody>
                                {entrega.produtos.map((produto) => (
                                    <tr key={produto.nome} className={
                                        produto.estado === 'alterado' ? 'table-warning' :
                                            produto.estado === 'confirmado' ? 'table-success' : ''
                                    }>
                                        <td className="align-middle">{produto.nome}</td>
                                        <td className="align-middle">{produto.quantidadeSubscrita}</td>
                                        <td>
                                            <input
                                                type="number"
                                                className="form-control"
                                                value={produto.quantidadeEntregue}
                                                onChange={(e) => handleQuantidadeChange(
                                                    entrega.id,
                                                    produto.nome,
                                                    parseInt(e.target.value) || 0
                                                )}
                                                disabled={produto.estado === 'confirmado'}
                                            />
                                        </td>
                                        <td className="align-middle">
                                                <span className={
                                                    produto.quantidadeEntregue - produto.quantidadeSubscrita < 0
                                                        ? 'text-danger'
                                                        : produto.quantidadeEntregue - produto.quantidadeSubscrita > 0
                                                            ? 'text-success'
                                                            : ''
                                                }>
                                                    {produto.quantidadeEntregue - produto.quantidadeSubscrita}
                                                </span>
                                        </td>
                                        <td className="align-middle">
                                                <span className={`badge ${produto.estado === 'alterado' ? 'bg-warning' :
                                                    produto.estado === 'confirmado' ? 'bg-success' :
                                                        'bg-secondary'
                                                }`}>
                                                    {produto.estado}
                                                </span>
                                        </td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>

                        <div className="mt-3 d-flex justify-content-between align-items-center">
                            <div>
                                <button
                                    className="btn btn-success me-2"
                                    onClick={() => confirmarEntrega(entrega.id)}
                                    disabled={entrega.produtos.every(p => p.estado === 'confirmado')}
                                >
                                    Confirmar Entrega
                                </button>

                                {entrega.produtos.some(p => p.estado === 'alterado') && (
                                    <button
                                        className="btn btn-warning"
                                        onClick={() => notificarAlteracao(entrega)}
                                    >
                                        <i className="bi bi-bell me-2"></i>
                                        Notificar Alterações
                                    </button>
                                )}
                            </div>

                            {entrega.produtos.some(p => p.estado === 'alterado') && (
                                <div className="alert alert-warning mb-0 px-3 py-2">
                                    <i className="bi bi-exclamation-triangle me-2"></i>
                                    Existem alterações nas quantidades que precisam ser notificadas.
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            ))}

            {/* Botão para gerar o PDF - Verde */}
            <button className="btn btn-success mt-4" onClick={gerarRelatorioPDF}>
                Gerar Relatório em PDF
            </button>
        </div>
    );
};

export default ProducerOrders;
