// Recupera carrinho salvo ou cria vazio
let cart = JSON.parse(localStorage.getItem("cart")) || [];

// Renderizar carrinho na página
function renderCart() {
  const cartItems = document.getElementById("cart-items");
  const cartTotal = document.getElementById("cart-total");
  cartItems.innerHTML = "";

  let total = 0;

  cart.forEach((item, index) => {
    const row = document.createElement("tr");

    row.innerHTML = `
      <td>${item.name}</td>
      <td>${item.price}€</td>
      <td>
        <input type="number" min="1" value="${item.quantity}" 
          onchange="updateQuantity(${index}, this.value)">
      </td>
      <td>${(item.price * item.quantity).toFixed(2)}€</td>
      <td><button class="btn btn-danger btn-sm" onclick="removeItem(${index})">Remover</button></td>
    `;

    total += item.price * item.quantity;
    cartItems.appendChild(row);
  });

  cartTotal.textContent = total.toFixed(2) + "€";
  localStorage.setItem("cart", JSON.stringify(cart));
}

// Adicionar item ao carrinho
function addToCart(name, price) {
  const existing = cart.find(item => item.name === name);
  if (existing) {
    existing.quantity++;
  } else {
    cart.push({ name, price, quantity: 1 });
  }
  localStorage.setItem("cart", JSON.stringify(cart));
  alert(`${name} foi adicionado ao carrinho!`);
}

// Atualizar quantidade
function updateQuantity(index, qty) {
  cart[index].quantity = parseInt(qty);
  renderCart();
}

// Remover item
function removeItem(index) {
  cart.splice(index, 1);
  renderCart();
}

// Se estivermos na página do carrinho, renderizar
if (document.getElementById("cart-items")) {
  renderCart();
}
