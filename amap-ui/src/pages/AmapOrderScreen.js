import React, { useState } from 'react';
import '../App.css';

const AmapManagerOrderScreen = () => {
    const [data, setData] = useState([
        { id: 1, coProdutor: 'João Silva', produtor: 'Carlos Lima', produto: 'Tomate', quantidadeEncomendada: 10, quantidadeEntregue: 8, valorUnitario: 3, contaCorrente: 50 },
        { id: 2, coProdutor: 'Maria Souza', produtor: 'Ana Pereira', produto: 'Alface', quantidadeEncomendada: 20, quantidadeEntregue: 22, valorUnitario: 2, contaCorrente: 30 },
        { id: 3, coProdutor: 'Carlos Lima', produtor: 'Paulo Rocha', produto: 'Pepino', quantidadeEncomendada: 15, quantidadeEntregue: 12, valorUnitario: 4, contaCorrente: 40 },
        { id: 4, coProdutor: 'João Silva', produtor: 'Maria Souza', produto: 'Batata', quantidadeEncomendada: 25, quantidadeEntregue: 25, valorUnitario: 1.5, contaCorrente: 20 },
        { id: 5, coProdutor: 'Paulo Rocha', produtor: 'Carlos Lima', produto: 'Cenoura', quantidadeEncomendada: 18, quantidadeEntregue: 17, valorUnitario: 2.5, contaCorrente: 60 },
        { id: 6, coProdutor: 'Ana Pereira', produtor: 'Paulo Rocha', produto: 'Tomate', quantidadeEncomendada: 12, quantidadeEntregue: 12, valorUnitario: 3, contaCorrente: 25 },
        { id: 7, coProdutor: 'Maria Souza', produtor: 'Ana Pereira', produto: 'Alface', quantidadeEncomendada: 30, quantidadeEntregue: 28, valorUnitario: 2, contaCorrente: 45 },
        { id: 8, coProdutor: 'Carlos Lima', produtor: 'João Silva', produto: 'Batata', quantidadeEncomendada: 20, quantidadeEntregue: 22, valorUnitario: 1.5, contaCorrente: 10 },
    ]);

    const calcularPrecoPago = (quantidadeEntregue, valorUnitario) => {
        return quantidadeEntregue * valorUnitario;
    };

    const calcularDiferenca = (quantidadeEncomendada, quantidadeEntregue, valorUnitario) => {
        const valorPago = calcularPrecoPago(quantidadeEntregue, valorUnitario);
        const valorEsperado = quantidadeEncomendada * valorUnitario;
        return valorPago - valorEsperado;
    };

    const atualizarContas = () => {
        const updatedData = data.map(item => {
            const diferenca = calcularDiferenca(item.quantidadeEncomendada, item.quantidadeEntregue, item.valorUnitario);
            const novaContaCorrente = item.contaCorrente + diferenca;
            return { ...item, contaCorrente: novaContaCorrente };
        });
        setData(updatedData);
    };

    const enviarNotificacao = () => {
        const notificacoes = data.map(item => {
            const diferenca = calcularDiferenca(item.quantidadeEncomendada, item.quantidadeEntregue, item.valorUnitario);
            if (diferenca !== 0) {
                return `Notificação para ${item.coProdutor}: Produto "${item.produto}" - Diferença de €${diferenca.toFixed(2)}.`;
            }
            return null;
        }).filter(notificacao => notificacao);

        if (notificacoes.length > 0) {
            alert(notificacoes.join('\n'));
        } else {
            alert("Nenhuma diferença para notificar.");
        }
    };

    return (
        <div className="manager-order-screen">
            <h1>Gestão de Entregas: Diferenças entre Produtos Encomendados e Entregues</h1>

            <div className="totals">
                <button onClick={atualizarContas}>Calcular Diferenças e Atualizar Contas Correntes</button>
                <button onClick={enviarNotificacao}>Notificar Co-produtores</button>
            </div>

            <table className="order-table">
                <thead>
                <tr>
                    <th>Co-produtor</th>
                    <th>Produto</th>
                    <th>Quant. Encomendada</th>
                    <th>Quant. Entregue</th>
                    <th>Valor Unitário (€)</th>
                    <th>Preço Pago (€)</th>
                    <th>Conta Corrente Atualizada (€)</th>
                </tr>
                </thead>
                <tbody>
                {data.map(item => {
                    const precoPago = calcularPrecoPago(item.quantidadeEntregue, item.valorUnitario);
                    const diferenca = calcularDiferenca(item.quantidadeEncomendada, item.quantidadeEntregue, item.valorUnitario);
                    return (
                        <tr key={item.id} className={diferenca < 0 ? 'negative' : diferenca > 0 ? 'positive' : ''}>
                            <td>{item.coProdutor}</td>
                            <td>{item.produto}</td>
                            <td>{item.quantidadeEncomendada}</td>
                            <td>{item.quantidadeEntregue}</td>
                            <td>{item.valorUnitario.toFixed(2)}</td>
                            <td>{precoPago.toFixed(2)}</td>
                            <td>{(item.contaCorrente + diferenca).toFixed(2)}</td>
                        </tr>
                    );
                })}
                </tbody>
            </table>
        </div>
    );
};

export default AmapManagerOrderScreen;
