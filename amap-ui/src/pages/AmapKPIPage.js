import React, { useState, useMemo, useEffect } from 'react';
import { Container, Row, Col, Button } from 'react-bootstrap';
import FilterInput from './FilterInput';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { jsPDF } from 'jspdf'; // Importa jsPDF
import html2canvas from 'html2canvas'; // Importa html2canvas para capturar a imagem
import * as XLSX from 'xlsx'; // Importa a biblioteca XLSX para gerar arquivos Excel

const KPIPage = () => {
    const [realData, setRealData] = useState([
        {
            id: 1,
            entregaId: 'E001',
            produtor: 'Quinta do Arneiro',
            coProdutor: 'Maria Santos',
            produto: 'Tomates Biológicos',
            quantidadeEncomendada: 100,
            quantidadeEntregue: 80,
            valorUnitario: 10,
            dataEntrega: '2024-01-01',
        },
        {
            id: 2,
            entregaId: 'E002',
            produtor: 'Horta do Vale',
            coProdutor: 'João Oliveira',
            produto: 'Alfaces',
            quantidadeEncomendada: 150,
            quantidadeEntregue: 150,
            valorUnitario: 15,
            dataEntrega: '2024-02-01',
        },
        {
            id: 3,
            entregaId: 'E003',
            produtor: 'Quinta da Terra',
            coProdutor: 'Ana Ribeiro',
            produto: 'Cenouras',
            quantidadeEncomendada: 200,
            quantidadeEntregue: 180,
            valorUnitario: 12,
            dataEntrega: '2024-03-01',
        },
        {
            id: 4,
            entregaId: 'E004',
            produtor: 'Bio Cantinho',
            coProdutor: 'Pedro Costa',
            produto: 'Couves',
            quantidadeEncomendada: 50,
            quantidadeEntregue: 45,
            valorUnitario: 20,
            dataEntrega: '2024-01-15',
        },
        {
            id: 5,
            entregaId: 'E005',
            produtor: 'Quinta do Arneiro',
            coProdutor: 'Maria Santos',
            produto: 'Tomates Biológicos',
            quantidadeEncomendada: 120,
            quantidadeEntregue: 110,
            valorUnitario: 10,
            dataEntrega: '2024-02-10',
        },
        {
            id: 6,
            entregaId: 'E006',
            produtor: 'Horta do Vale',
            coProdutor: 'João Oliveira',
            produto: 'Alfaces',
            quantidadeEncomendada: 130,
            quantidadeEntregue: 120,
            valorUnitario: 15,
            dataEntrega: '2024-03-15',
        }
    ]);
    const [filteredData, setFilteredData] = useState([]);
    const [selectedPeriod, setSelectedPeriod] = useState({ start: '', end: '' });
    const [selectedProducer, setSelectedProducer] = useState('');
    const [selectedCoProducer, setSelectedCoProducer] = useState('');
    const [selectedProduct, setSelectedProduct] = useState('');

    // Função de filtragem
    useEffect(() => {
        const filtered = realData.filter(item => {
            const inPeriod =
                (!selectedPeriod.start || new Date(item.dataEntrega) >= new Date(selectedPeriod.start)) &&
                (!selectedPeriod.end || new Date(item.dataEntrega) <= new Date(selectedPeriod.end));
            const byProducer = !selectedProducer || item.produtor === selectedProducer;
            const byCoProducer = !selectedCoProducer || item.coProdutor === selectedCoProducer;
            const byProduct = !selectedProduct || item.produto === selectedProduct;

            return inPeriod && byProducer && byCoProducer && byProduct;
        });
        setFilteredData(filtered);
    }, [realData, selectedPeriod, selectedProducer, selectedCoProducer, selectedProduct]);

    // Função de exportação para PDF
    const exportToPDF = () => {
        const input = document.getElementById('kpi-report'); // Captura o conteúdo com o id 'kpi-report'

        // Usa html2canvas para gerar a imagem do conteúdo
        html2canvas(input).then(canvas => {
            const imgData = canvas.toDataURL('image/png');
            const pdf = new jsPDF();
            pdf.addImage(imgData, 'PNG', 0, 0, 210, 297); // Posição e tamanho da imagem no PDF
            pdf.save('kpi-report.pdf'); // Nome do arquivo PDF gerado
        });
    };

    // Função de exportação para Excel
    const exportToExcel = () => {
        const worksheet = XLSX.utils.json_to_sheet(filteredData); // Converte os dados para planilha
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, 'KPIs');
        XLSX.writeFile(workbook, 'kpi-report.xlsx'); // Gera e baixa o arquivo Excel
    };

    // Função de renderização dos gráficos
    const renderGraficos = () => (
        <Row className="mb-4">
            <Col md={6}>
                <h4>Valor Total por Produtor</h4>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={filteredData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="produtor" />
                        <YAxis />
                        <Tooltip />
                        <Bar dataKey="quantidadeEntregue" fill="#8884d8" />
                    </BarChart>
                </ResponsiveContainer>
            </Col>
            <Col md={6}>
                <h4>Valor Médio por Co-Produtor</h4>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={filteredData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="coProdutor" />
                        <YAxis />
                        <Tooltip />
                        <Bar dataKey="quantidadeEntregue" fill="#82ca9d" />
                    </BarChart>
                </ResponsiveContainer>
            </Col>
        </Row>
    );

    return (
        <Container fluid>
            <h1 className="text-center my-4">KPIs da Gestão</h1>

            {/* Filtros */}
            <Row className="mb-4">
                <FilterInput
                    label="Data Início"
                    value={selectedPeriod.start}
                    onChange={e => setSelectedPeriod({ ...selectedPeriod, start: e.target.value })}
                    type="date"
                />
                <FilterInput
                    label="Data Fim"
                    value={selectedPeriod.end}
                    onChange={e => setSelectedPeriod({ ...selectedPeriod, end: e.target.value })}
                    type="date"
                />
                <FilterInput
                    label="Produtor"
                    value={selectedProducer}
                    onChange={e => setSelectedProducer(e.target.value)}
                    options={[...new Set(realData.map(item => item.produtor))]}
                    type="select"
                />
                <FilterInput
                    label="Co-produtor"
                    value={selectedCoProducer}
                    onChange={e => setSelectedCoProducer(e.target.value)}
                    options={[...new Set(realData.map(item => item.coProdutor))]}
                    type="select"
                />
                <FilterInput
                    label="Produto"
                    value={selectedProduct}
                    onChange={e => setSelectedProduct(e.target.value)}
                    options={[...new Set(realData.map(item => item.produto))]}
                    type="select"
                />
                <Col md={3} className="d-flex align-items-end">
                    <Button onClick={() => {
                        setSelectedPeriod({ start: '', end: '' });
                        setSelectedProducer('');
                        setSelectedCoProducer('');
                        setSelectedProduct('');
                    }} variant="secondary">
                        Limpar Filtros
                    </Button>
                </Col>
            </Row>

            {renderGraficos()}

            {/* Exibindo dados filtrados */}
            <Row>
                <Col md={12}>
                    <h4>Dados Filtrados</h4>
                    <table className="table table-bordered table-striped">
                        <thead className="thead-dark">
                        <tr>
                            <th>Entrega ID</th>
                            <th>Produtor</th>
                            <th>Co-produtor</th>
                            <th>Produto</th>
                            <th>Quantidade Encomendada</th>
                            <th>Quantidade Entregue</th>
                            <th>Valor Unitário (€)</th>
                            <th>Data Entrega</th>
                        </tr>
                        </thead>
                        <tbody>
                        {filteredData.map(item => (
                            <tr key={item.id}>
                                <td>{item.entregaId}</td>
                                <td>{item.produtor}</td>
                                <td>{item.coProdutor}</td>
                                <td>{item.produto}</td>
                                <td>{item.quantidadeEncomendada}</td>
                                <td>{item.quantidadeEntregue}</td>
                                <td>{item.valorUnitario.toFixed(2)}</td>
                                <td>{item.dataEntrega}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </Col>
            </Row>

            {/* Botões de exportação */}
            <Row className="mt-4">
                <Col className="d-flex justify-content-center">
                    <Button
                        variant="success"
                        onClick={exportToPDF}
                        className="mx-2">
                        Exportar para PDF
                    </Button>
                    <Button
                        variant="success"
                        onClick={exportToExcel}
                        className="mx-2">
                        Exportar para Excel
                    </Button>
                </Col>
            </Row>
        </Container>
    );
};

export default KPIPage;
