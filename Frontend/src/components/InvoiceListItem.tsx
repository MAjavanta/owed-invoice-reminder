import type { Invoice } from "../types/invoice";

interface InvoiceListItemProps {
  invoice: Invoice;
}

export default function InvoiceListItem({ invoice }: InvoiceListItemProps) {
  return (
    <li>
      {invoice.id} {invoice.clientName}
    </li>
  );
}
