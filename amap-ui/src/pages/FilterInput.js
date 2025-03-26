import React from 'react';
import { Col, Form } from 'react-bootstrap';

const FilterInput = ({ label, value, onChange, options, type = 'text' }) => (
    <Col md={3}>
        <Form.Group>
            <Form.Label>{label}</Form.Label>
            {type === 'select' ? (
                <Form.Control as="select" value={value} onChange={onChange}>
                    <option value="">Todos</option>
                    {options.map(option => (
                        <option key={option} value={option}>
                            {option}
                        </option>
                    ))}
                </Form.Control>
            ) : (
                <Form.Control type={type} value={value} onChange={onChange} />
            )}
        </Form.Group>
    </Col>
);

export default FilterInput;
