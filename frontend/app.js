let products = [];

async function apiGet(path) {
  const response = await fetch(`${window.API_BASE_URL}${path}`);
  if (!response.ok) {
    throw new Error(`Error ${response.status} al consultar ${path}`);
  }
  return response.json();
}

async function apiPost(path, body) {
  const response = await fetch(`${window.API_BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!response.ok) {
    const payload = await response.json().catch(() => ({}));
    throw new Error(payload.error || `Error ${response.status} al enviar a ${path}`);
  }
  return response.status === 204 ? null : response.json();
}

function formatCurrency(value) {
  return new Intl.NumberFormat("es-CO", { style: "currency", currency: "USD" }).format(value);
}

async function loadCustomers() {
  const customers = await apiGet("/customers");

  const tableBody = document.getElementById("customersTableBody");
  tableBody.innerHTML = customers
    .map((customer) => `<tr><td>${customer.name}</td><td>${customer.email}</td></tr>`)
    .join("");

  const options = customers
    .map((customer) => `<option value="${customer.id}">${customer.name}</option>`)
    .join("");
  document.getElementById("orderCustomer").innerHTML = options;
  document.getElementById("reportCustomer").innerHTML = options;
}

async function loadProducts() {
  products = await apiGet("/products");

  const tableBody = document.getElementById("productsTableBody");
  tableBody.innerHTML = products
    .map(
      (product) =>
        `<tr><td>${product.name}</td><td>${formatCurrency(product.price)}</td><td>${product.stock}</td></tr>`
    )
    .join("");

  document.querySelectorAll(".order-item-product").forEach(updateProductOptions);
}

function updateProductOptions(select) {
  const currentValue = select.value;
  select.innerHTML = products
    .map((product) => `<option value="${product.id}">${product.name} (stock: ${product.stock})</option>`)
    .join("");
  if (currentValue) {
    select.value = currentValue;
  }
}

function addItemRow() {
  const container = document.getElementById("orderItems");
  const row = document.createElement("div");
  row.className = "order-item-row";

  const select = document.createElement("select");
  select.className = "form-select order-item-product";
  updateProductOptions(select);

  const quantity = document.createElement("input");
  quantity.type = "number";
  quantity.min = "1";
  quantity.value = "1";
  quantity.className = "form-control order-item-quantity";

  const removeButton = document.createElement("button");
  removeButton.type = "button";
  removeButton.className = "btn btn-outline-danger btn-sm";
  removeButton.textContent = "Quitar";
  removeButton.addEventListener("click", () => row.remove());

  row.append(select, quantity, removeButton);
  container.appendChild(row);
}

function statusBadge(status) {
  const map = {
    Pending: "secondary",
    Confirmed: "primary",
    Completed: "success",
    Cancelled: "danger",
  };
  return `<span class="badge bg-${map[status] || "secondary"}">${status}</span>`;
}

async function loadOrders() {
  const orders = await apiGet("/orders");

  const tableBody = document.getElementById("ordersTableBody");
  tableBody.innerHTML = orders
    .map((order) => {
      const date = new Date(order.orderDate).toLocaleString();
      const actions = [];
      if (order.status === "Pending") {
        actions.push(`<button class="btn btn-sm btn-outline-primary" onclick="confirmOrder('${order.id}')">Confirmar</button>`);
      }
      if (order.status === "Confirmed") {
        actions.push(`<button class="btn btn-sm btn-outline-success" onclick="completeOrder('${order.id}')">Completar</button>`);
      }
      if (order.status === "Pending" || order.status === "Confirmed") {
        actions.push(`<button class="btn btn-sm btn-outline-danger" onclick="cancelOrder('${order.id}')">Cancelar</button>`);
      }
      return `<tr>
        <td>${date}</td>
        <td>${statusBadge(order.status)}</td>
        <td>${formatCurrency(order.total)}</td>
        <td>${actions.join(" ")}</td>
      </tr>`;
    })
    .join("");
}

async function confirmOrder(id) {
  await apiPost(`/orders/${id}/confirm`);
  await loadOrders();
  await loadProducts();
}

async function completeOrder(id) {
  await apiPost(`/orders/${id}/complete`);
  await loadOrders();
}

async function cancelOrder(id) {
  await apiPost(`/orders/${id}/cancel`);
  await loadOrders();
  await loadProducts();
}

document.getElementById("customerForm").addEventListener("submit", async (event) => {
  event.preventDefault();
  const name = document.getElementById("customerName").value;
  const email = document.getElementById("customerEmail").value;

  await apiPost("/customers", { name, email });
  event.target.reset();
  await loadCustomers();
});

document.getElementById("productForm").addEventListener("submit", async (event) => {
  event.preventDefault();
  const name = document.getElementById("productName").value;
  const price = parseFloat(document.getElementById("productPrice").value);
  const stock = parseInt(document.getElementById("productStock").value, 10);

  await apiPost("/products", { name, price, stock });
  event.target.reset();
  await loadProducts();
});

document.getElementById("addItemButton").addEventListener("click", addItemRow);

document.getElementById("orderForm").addEventListener("submit", async (event) => {
  event.preventDefault();
  const customerId = document.getElementById("orderCustomer").value;
  const rows = document.querySelectorAll("#orderItems .order-item-row");

  const items = Array.from(rows).map((row) => ({
    productId: row.querySelector(".order-item-product").value,
    quantity: parseInt(row.querySelector(".order-item-quantity").value, 10),
  }));

  if (items.length === 0) {
    alert("Agrega al menos un producto al pedido.");
    return;
  }

  await apiPost("/orders", { customerId, items });
  document.getElementById("orderItems").innerHTML = "";
  await loadOrders();
  await loadProducts();
});

document.getElementById("loadReportButton").addEventListener("click", async () => {
  const customerId = document.getElementById("reportCustomer").value;
  if (!customerId) {
    return;
  }

  const resultCard = document.getElementById("reportResult");
  try {
    const summary = await apiGet(`/orders/reports/customer/${customerId}`);
    document.getElementById("reportTotalOrders").textContent = summary.totalOrders;
    document.getElementById("reportTotalSpent").textContent = formatCurrency(summary.totalSpent);
    document.getElementById("reportLastOrder").textContent = summary.lastOrderDate
      ? new Date(summary.lastOrderDate).toLocaleString()
      : "Sin pedidos";
    resultCard.classList.remove("d-none");
  } catch (error) {
    resultCard.classList.add("d-none");
    alert("Este cliente aún no tiene pedidos.");
  }
});

(async function init() {
  await loadCustomers();
  await loadProducts();
  await loadOrders();
  addItemRow();
})();
