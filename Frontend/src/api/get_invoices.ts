import { baseAddress } from "./client";
import { type Invoice } from "../types/invoice";

export default async function GetInvoices(): Promise<Invoice[]> {
  const route = "/invoices";
  const response = await fetch(baseAddress + route);
  const data = await response.json();
  return data;
}
