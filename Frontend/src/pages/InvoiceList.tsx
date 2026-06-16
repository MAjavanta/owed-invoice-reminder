import { Link } from "react-router-dom";
import getInvoices from "../api/get_invoices";
import { useEffect, useState } from "react";
import type { Invoice } from "../types/invoice";
import InvoiceListItem from "../components/InvoiceListItem";

export default function InvoiceList() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      const data = await getInvoices();
      setInvoices(data);
    };
    fetchData();
  }, []);

  const invoiceNames = invoices.map((invoice) => (
    <InvoiceListItem key={invoice.id} invoice={invoice} />
  ));
  return (
    <>
      <div>InvoiceList</div>
      <nav>
        <Link to="/">Home</Link>
      </nav>
      <ul>{invoiceNames}</ul>
      <Link to="/create-invoice">
        <button>Create Form</button>
      </Link>
    </>
  );
}
