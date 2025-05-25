import React, { useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Sidebar from './components/Sidebar';
import Header from './components/Header';
import Dashboard from './components/Dashboard';
import CatalogPage from './pages/CatalogPageBE';
import AmapSubscription from './pages/AmapSubscritionPage';
import CalculationPage from './pages/CalculationPage';
import LoginSignUp from "./pages/LoginSignUp";
import CoProducerMainScreen from "./pages/CoProducerMainScreen";
import OfferPageBE from "./pages/offerPageBE"

import CoProducerSubscription from "./pages/CoProducerSubscription";
import SubscriptionDetails from "./pages/CoProducerSubscriptionDetails";
import CoProducerSubscriptionDetails from "./pages/CoProducerSubscriptionDetails";
import CoProducerSubscriptionPayment from "./pages/CoProducerSubscriptionPayment";
import ProducerOrders from "./pages/ProducerOrders";
import AmapManagerOrderScreen from "./pages/AmapOrderScreen";
import ProducerOffer from "./pages/ProducerOffer";
import AmapKPIPage from "./pages/AmapKPIPage";

import OfferPage from './pages/OfferPage';
import SalePage from './pages/SalePage';
import Payment from './pages/Payment';
import CatalogPageBE from './pages/CatalogPageBE';
import AMAPSubscriptionPageBE from './pages/AmapSubscriptionPageBE';


function App() {
    const [isSidebarVisible, setSidebarVisible] = useState(true);

    const toggleSidebar = () => {
        setSidebarVisible((prevState) => !prevState);
    };

    return (
        <BrowserRouter>
            <div style={{ display: 'flex', minHeight: '100vh' }}>
                <Sidebar isVisible={isSidebarVisible} toggleSidebar={toggleSidebar} />
                <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                    <Header isSidebarVisible={isSidebarVisible} toggleSidebar={toggleSidebar} />
                    <main style={{ flex: 1, padding: '20px', backgroundColor: '#f7f7f7' }}>
                        <Routes>
                            <Route path="/" element={<Dashboard />} />


                            <Route path="/calculationPage" element={<CalculationPage />} />
                            <Route path="/login" element={<LoginSignUp />} />

                            <Route path="/producerCatalogo" element={<CatalogPage />} />
                            <Route path="/producerCatalogoBE" element={<CatalogPageBE />} />
                            <Route path="/producerOrders" element={<ProducerOrders />} />
                            <Route path="/producerOffer" element={<OfferPage />} />
                            <Route path="/producerOfferBE" element={<OfferPageBE />} />

                            <Route path="/amapSubscriptionPeriod" element={< AmapSubscription />} />
                            <Route path="/amapSubscriptionPeriodBE" element={< AMAPSubscriptionPageBE />} />
                            <Route path="/amapCalculationPage" element={<CalculationPage />} />
                            <Route path="/amapOrders" element={<AmapManagerOrderScreen />} />
                            <Route path="/amapKPI" element={<AmapKPIPage />} />

                            <Route path="/coProducerMainScreen" element={< SalePage />} />

                            <Route path='/pagamento' element={< Payment />} />

                        </Routes>
                    </main>
                </div>
            </div>
        </BrowserRouter>
    );
}

export default App;