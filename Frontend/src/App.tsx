import { Route, Routes } from "react-router-dom";
import "./App.css";
import InvoiceList from "./pages/InvoiceList";
import Home from "./pages/Home";

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/invoices" element={<InvoiceList />} />
      </Routes>
    </>
  );
}

export default App;
