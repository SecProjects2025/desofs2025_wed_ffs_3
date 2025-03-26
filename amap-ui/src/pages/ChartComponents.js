import React from 'react';
import { BarChart, Bar, PieChart, Pie, Cell, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

export const BarChartComponent = ({ data }) => (
    <BarChart width={500} height={300} data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="produtor" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Bar dataKey="valorEntregue" fill="#8884d8" />
    </BarChart>
);

export const PieChartComponent = ({ data }) => (
    <PieChart width={400} height={400}>
        <Pie
            data={data}
            dataKey="quantidadeEntregue"
            nameKey="produto"
            cx="50%"
            cy="50%"
            outerRadius={150}
            fill="#8884d8"
            label
        >
            {data.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
            ))}
        </Pie>
        <Tooltip />
    </PieChart>
);

export const LineChartComponent = ({ data }) => (
    <LineChart width={500} height={300} data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="dataEntrega" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Line type="monotone" dataKey="valorSubscrito" stroke="#82ca9d" />
    </LineChart>
);
