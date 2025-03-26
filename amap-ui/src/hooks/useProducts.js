import { useState, useEffect } from 'react';
import { getProducerId, getProducts, createProduct, updateProduct, deleteProduct } from '../service/productService';
const useProducts = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Função para buscar produtos do produtor autenticado
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const producerId = getProducerId(); // Obtém o producerId do localStorage
        const data = await getProducts(producerId); // Busca os dados do catálogo
        console.log('Dados dos produtos:', data);

        if (data && data.length > 0 && data[0].products) {
          setProducts(data[0].products); // Atualiza o estado com a lista de produtos
        } else {
          setProducts([]); // Caso não haja produtos, seta uma lista vazia
        }
      } catch (err) {
        setError('Erro ao carregar produtos.'); // Exibe erro caso a requisição falhe
      } finally {
        setLoading(false); // Finaliza o carregamento
      }
    };

    fetchProducts(); // Chama a função para buscar os produtos
  }, []);
// O efeito só será chamado uma vez, após o componente ser montado
  // Dependência de `producerId` para refazer a requisição quando ele mudar
  

  // Função para criar um novo produto
  const handleCreate = async (productData) => {
    try {
      const newProduct = await createProduct(productData);
      setProducts((prev) => [...prev, newProduct]);
    } catch (err) {
      setError('Erro ao adicionar produto.');
    }
  };

  // Função para atualizar um produto existente
  const handleUpdate = async (id, updatedData) => {
    if (!id) {
      setError("ID do produto inválido ou ausente.");
      return;
    }
  
    try {
      const updatedProduct = await updateProduct(id, updatedData);
  
      setProducts((prev) =>
        prev.map((product) =>
          product.id === id ? { ...product, ...updatedData } : product
        )
      );
  
      console.log("Produto atualizado:", updatedProduct || "Nenhum conteúdo recebido");
      setError(""); // Limpa o erro após sucesso
    } catch (err) {
      const errorMessage = err.message || "Erro desconhecido";
      setError(`Erro ao editar produto: ${errorMessage}`);
      console.error(`Erro ao editar produto: ${errorMessage}`);
    }
  };
  
  
  

  // Função para deletar um produto
  const handleDelete = async (id) => {
    try {
      await deleteProduct(id);
      setProducts((prev) => prev.filter((product) => product.id !== id));
    } catch (err) {
      setError('Erro ao excluir produto.');
    }
  };

  return { products, loading, error, handleCreate, handleUpdate, handleDelete };
};

export default useProducts;
