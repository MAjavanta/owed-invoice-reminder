import { Route, Routes } from "react-router-dom";
import "./App.css";
import InvoiceList from "./pages/InvoiceList";
import Home from "./pages/Home";
import CreateInvoiceForm from "./pages/CreateInvoiceForm";

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/invoices" element={<InvoiceList />} />
        <Route path="/create-invoice" element={<CreateInvoiceForm />} />
      </Routes>
    </>
  );
}

export default App;
