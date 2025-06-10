const API_URL = 'https://localhost:7262/api/'; // Altere conforme sua URL da API

/*const convertTypeForBackend = (type) => {
    return type === 'Simples' ? 'Simple' : 'Composite';
};*/

export const getProducerId = () => {
  // Recupera o userId do sessionStorage (que será igual ao producerId após o login)
  const userId = sessionStorage.getItem('userId');
  console.log('User ID no sessionStorage:', userId);
  if (!userId) {
    throw new Error('Producer ID não encontrado');
  }
  return userId; // Retorna o ID do produtor
};

export const getProducts = async (producerId) => {
  const response = await fetch(`${API_URL}Catalog/GetCatalog/${producerId}`, {
    method: 'GET',
    headers: {
      'Accept': 'application/json',
    },
    credentials: 'include', // Se necessário, para enviar cookies
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error("Erro ao buscar produtos:", errorData);
    throw new Error(errorData.message || 'Falha ao buscar os produtos');
  }

  const products = await response.json();

  // Ajusta os dados para o frontend
  return products.map((product) => ({
    ...product,
    compositeItems: product.components || [], // Transforma components em compositeItems
  }));
};


export const createProduct = async (productData) => {
  // Converte o tipo para o formato esperado pelo backend
  //productData.type = convertTypeForBackend(productData.type);
  console.log("Enviando os dados do produto para o backend:", productData);

  const response = await fetch(`${API_URL}Product`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      credentials: 'include',
      //'Authorization': `Bearer ${localStorage.getItem('token')}`, // Adicionando token de autenticação
    },
    credentials: 'include',
    body: JSON.stringify(productData),
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error("Erro ao criar produto:", errorData);
    throw new Error(errorData.message || 'Failed to create product');
  }

  return await response.json();


};

export const updateProduct = async (id, updatedData) => {
  if (!id) {
    throw new Error("ID do produto inválido ou ausente.");
  }

  const payload = {
    producerId: getProducerId(),
    id: id,
    name: updatedData.name,
    description: updatedData.description,
    photo: updatedData.photo,
    referencePrice: updatedData.referencePrice,
    deliveryUnits: updatedData.deliveryUnits,
    composites: [], // Subprodutos
  };

  console.log("Payload enviado:", payload);

  const response = await fetch(`${API_URL}Product/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
    mode: "cors",
  });

  if (!response.ok) {
    try {
      const errorData = await response.json();
      console.error("Erro na atualização:", errorData);
      throw new Error(errorData.message || "Falha ao atualizar o produto");
    } catch {
      throw new Error("Erro desconhecido na resposta do servidor.");
    }
  }

  // Verifica se há conteúdo na resposta
  try {
    const data = await response.json();
    return data;
  } catch {
    console.warn("Resposta vazia ou não JSON.");
    return null; // Resposta sem conteúdo
  }
};



export const deleteProduct = async (id) => {
  const response = await fetch(`${API_URL}Product/${id}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
      //'Authorization': `Bearer ${localStorage.getItem('token')}`, // Adicionando token de autenticação
    },
    credentials: 'include',
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error('Erro ao excluir produto:', errorData);
    throw new Error(errorData.message || 'Failed to delete product');
  }
};
