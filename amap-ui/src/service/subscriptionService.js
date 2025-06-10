const API_URL = 'https://localhost:7262/api/subscriptionperiod/'; // Altere conforme a sua URL da API

export const getProducerId = () => {
  const userId = sessionStorage.getItem('userId');
  console.log('User ID no sessionStorage:', userId);
  if (!userId) {
    throw new Error('Manager ID não encontrado');
  }
  return userId; // Retorna o ID do manager
};

export const getSubscriptionPeriodById = async (id) => {
  const response = await fetch(`${API_URL}subscriptionPeriod/${id}`, {
    method: 'GET',
    headers: {
      'Accept': 'application/json',
    },
    credentials: 'include',
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error('Erro ao buscar período de subscrição:', errorData);
    throw new Error(errorData.message || 'Falha ao buscar período de subscrição');
  }

  return await response.json();
};

export const createSubscriptionPeriod = async (subscriptionPeriod) => {
  console.log('Enviando os dados do período de subscrição para o backend:', subscriptionPeriod);

  const response = await fetch(`${API_URL}subscriptionperiod`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(subscriptionPeriod),
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error('Erro ao criar período de subscrição:', errorData);
    throw new Error(errorData.message || 'Falha ao criar período de subscrição');
  }

  return await response.json();
};

export const deleteSubscriptionPeriod = async (id) => {
  const response = await fetch(`${API_URL}subscriptionPeriod/${id}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  });

  if (!response.ok) {
    const errorData = await response.json();
    console.error('Erro ao excluir período de subscrição:', errorData);
    throw new Error(errorData.message || 'Falha ao excluir período de subscrição');
  }
};
