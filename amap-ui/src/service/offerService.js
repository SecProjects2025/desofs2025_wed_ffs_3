const API_URL = 'https://localhost:7262/api'; // Ajuste a URL conforme necessário

export const getDeliveryDates = async () => {
    try {
      const response = await fetch(`${API_URL}/subscriptionperiod/DeliveryDate/1`, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
        },
        credentials: 'include',
      });
  
      if (!response.ok) {
        const errorData = await response.json();
        console.error("Erro ao buscar datas de entrega:", errorData);
        throw new Error(errorData.message || 'Falha ao buscar as datas de entrega');
      }
  
      const deliveryDate = await response.json();
      console.log("Dados recebidos da API:", deliveryDate);
  
      // Converter a data para um formato legível
      const formattedDate = new Date(deliveryDate.date).toLocaleDateString('pt-PT', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
      });
  
      return { id: deliveryDate.id, date: formattedDate };
    } catch (error) {
      console.error("Erro na função getDeliveryDates:", error);
      throw error;
    }
  };
  

  

// Função para criar uma nova oferta de produto
export const createOffer = async (productId, price, quantity, subscriptionPeriodId) => {
    // Obter as datas de entrega para o período de subscrição
    const deliveryDates = await getDeliveryDates(subscriptionPeriodId);
    
    if (deliveryDates.length === 0) {
      throw new Error('Nenhuma data de entrega disponível para o período de subscrição');
    }
  
    // Selecionar a primeira data de entrega (poderias adicionar lógica para permitir a seleção de outra)
    const deliveryDate = deliveryDates[0].date; 
  
    // Criar a oferta
    const offerData = {
      subscriptionPeriodId: 1, // Assumindo que o ID da subscrição será sempre 1
      productId: productId,
      price: price,
      quantity: quantity,
      deliveryDate: deliveryDate,
      paymentType: 2, // Defina conforme necessário
    };
  
    const response = await fetch(`${API_URL}Offer`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(offerData),
    });
  
    if (!response.ok) {
      const errorData = await response.json();
      console.error("Erro ao criar a oferta:", errorData);
      throw new Error(errorData.message || 'Falha ao criar a oferta');
    }
  
    return await response.json();
  };
  

// Função para obter ofertas de produto por produtor
export const getProductOffersByProducer = async (producerId) => {
  try {
    const response = await fetch(`${API_URL}/ProductOffer/${producerId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
      },
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Erro ao obter ofertas de produtos');
    }
    return await response.json();
  } catch (error) {
    console.error('Erro ao buscar ofertas de produto:', error);
    throw error;
  }
};
