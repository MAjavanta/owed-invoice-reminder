import { Link } from "react-router-dom";

export default function CreateInvoiceForm() {
  function postForm(event: React.SyntheticEvent<HTMLFormElement, SubmitEvent>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const clientName = formData.get("client-name");
    console.log("Client name: ", clientName);
  }
  return (
    <>
      <nav>
        <Link to="/">Home</Link>
        <Link to="/invoices">Invoice List</Link>
      </nav>
      <form onSubmit={postForm}>
        <label htmlFor="client-name">Client Name</label>
        <input id="client-name" name="client-name" type="text" />
        <input type="submit" />
      </form>
    </>
  );
}
