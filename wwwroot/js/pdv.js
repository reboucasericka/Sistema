document.addEventListener("DOMContentLoaded", function () {
    // Variáveis globais
    let cartItems = [];
    let currentProduct = null;
    let customers = [];
    let professionals = [];
    let paymentMethods = [];

    // Inicialização
    loadCustomers();
    loadProfessionals();
    loadPaymentMethods();

    // Event Listeners para o PDV Principal (Index.cshtml)
    const barcodeInput = document.getElementById("barcodeInput");
    const addProductBtn = document.getElementById("addProductBtn");
    const checkoutBtn = document.getElementById("checkoutBtn");

    if (barcodeInput) {
        barcodeInput.addEventListener('keypress', function(e) {
            if (e.which === 13) {
                searchProduct();
            }
        });
    }

    if (addProductBtn) {
        addProductBtn.addEventListener('click', function() {
            const barcode = barcodeInput ? barcodeInput.value.trim() : '';
            if (!barcode) {
                alert("Informe um código de produto válido!");
                return;
            }
            searchProduct();
        });
    }

    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', finalizeSale);
    }

    // Event Listeners para o PDV Partial (_PDVPartial.cshtml)
    const searchProductBtn = document.getElementById("searchProductBtn");
    const addProductBtnPartial = document.getElementById("btnAddProduct");
    const clearCartBtn = document.getElementById("clearCartBtn");
    const saleForm = document.getElementById("saleForm");

    if (searchProductBtn) {
        searchProductBtn.addEventListener('click', searchProduct);
    }

    if (addProductBtnPartial) {
        addProductBtnPartial.addEventListener('click', addProductToCart);
    }

    if (clearCartBtn) {
        clearCartBtn.addEventListener('click', clearCart);
    }

    if (saleForm) {
        saleForm.addEventListener('submit', finalizeSale);
    }

    // Event Listeners para os modais
    const btnSelectClient = document.getElementById("btnSelectClient");
    const btnSelectProfessional = document.getElementById("btnSelectProfessional");
    const btnConfirmClient = document.getElementById("btnConfirmClient");
    const btnConfirmProfessional = document.getElementById("btnConfirmProfessional");

    if (btnSelectClient) {
        btnSelectClient.addEventListener('click', function() {
            const modal = new bootstrap.Modal(document.getElementById("modalSelectClient"));
            modal.show();
        });
    }

    if (btnSelectProfessional) {
        btnSelectProfessional.addEventListener('click', function() {
            const modal = new bootstrap.Modal(document.getElementById("modalSelectProfessional"));
            modal.show();
        });
    }

    if (btnConfirmClient) {
        btnConfirmClient.addEventListener('click', function() {
            const clientSelect = document.getElementById("clientSelect");
            const customerId = document.getElementById("customerId");
            if (clientSelect && customerId) {
                customerId.value = clientSelect.value;
                const modal = bootstrap.Modal.getInstance(document.getElementById("modalSelectClient"));
                modal.hide();
            }
        });
    }

    if (btnConfirmProfessional) {
        btnConfirmProfessional.addEventListener('click', function() {
            const professionalSelect = document.getElementById("professionalSelect");
            const professionalId = document.getElementById("professionalId");
            if (professionalSelect && professionalId) {
                professionalId.value = professionalSelect.value;
                const modal = bootstrap.Modal.getInstance(document.getElementById("modalSelectProfessional"));
                modal.hide();
            }
        });
    }

    // Event Listeners para campos de quantidade e preço
    const productQuantity = document.getElementById("productQuantity");
    const productUnitPrice = document.getElementById("productUnitPrice");
    const discount = document.getElementById("discount");
    const professionalId = document.getElementById("professionalId");

    if (productQuantity && productUnitPrice) {
        productQuantity.addEventListener('input', calculateSubtotal);
        productUnitPrice.addEventListener('input', calculateSubtotal);
    }

    if (discount) {
        discount.addEventListener('input', calculateTotals);
    }

    if (professionalId) {
        professionalId.addEventListener('change', calculateCommission);
    }

    // Funções principais
    async function searchProduct() {
        const barcodeInput = document.getElementById("barcodeInput");
        const code = barcodeInput ? barcodeInput.value.trim() : '';
        if (!code) return;

        try {
            const response = await fetch(`/Admin/AdminCashRegister/GetProductByBarcode?barcode=${code}`);
            if (!response.ok) {
                alert("Produto não encontrado!");
                return;
            }

            const product = await response.json();
            currentProduct = {
                id: product.id,
                name: product.name,
                price: product.price,
                stock: product.stock
            };
            showProductResult();
        } catch (error) {
            console.error('Erro ao buscar produto:', error);
            alert("Erro ao buscar produto!");
        }
    }

    function showProductResult() {
        const productName = document.getElementById("productName");
        const productPrice = document.getElementById("productPrice");
        const productStock = document.getElementById("productStock");
        const productUnitPrice = document.getElementById("productUnitPrice");
        const productQuantity = document.getElementById("productQuantity");
        const productSearchResult = document.getElementById("productSearchResult");
        const addProductBtn = document.getElementById("addProductBtn");

        if (productName) productName.textContent = currentProduct.name;
        if (productPrice) productPrice.textContent = '€' + currentProduct.price.toFixed(2);
        if (productStock) productStock.textContent = currentProduct.stock;
        if (productUnitPrice) productUnitPrice.value = currentProduct.price;
        if (productQuantity) productQuantity.value = 1;
        
        calculateSubtotal();
        
        if (productSearchResult) productSearchResult.style.display = 'block';
        if (addProductBtn) addProductBtn.disabled = false;
    }

    function calculateSubtotal() {
        const productQuantity = document.getElementById("productQuantity");
        const productUnitPrice = document.getElementById("productUnitPrice");
        const productSubtotal = document.getElementById("productSubtotal");

        if (!productQuantity || !productUnitPrice || !productSubtotal) return;

        const quantity = parseInt(productQuantity.value) || 0;
        const unitPrice = parseFloat(productUnitPrice.value) || 0;
        const subtotal = quantity * unitPrice;
        productSubtotal.value = '€' + subtotal.toFixed(2);
    }

    function addProductToCart() {
        if (!currentProduct) return;

        const productQuantity = document.getElementById("productQuantity");
        const productUnitPrice = document.getElementById("productUnitPrice");

        if (!productQuantity || !productUnitPrice) return;

        const quantity = parseInt(productQuantity.value) || 0;
        const unitPrice = parseFloat(productUnitPrice.value) || 0;

        if (quantity <= 0) {
            alert("Quantidade deve ser maior que zero");
            return;
        }

        if (quantity > currentProduct.stock) {
            alert("Quantidade maior que o estoque disponível");
            return;
        }

        const existingItem = cartItems.find(item => item.productId === currentProduct.id);
        if (existingItem) {
            existingItem.quantity += quantity;
            existingItem.subtotal = existingItem.quantity * existingItem.unitPrice;
        } else {
            cartItems.push({
                productId: currentProduct.id,
                productName: currentProduct.name,
                quantity: quantity,
                unitPrice: unitPrice,
                subtotal: quantity * unitPrice
            });
        }

        updateCartTable();
        calculateTotals();
        clearProductSearch();
    }

    function updateCartTable() {
        const tbody = document.querySelector("#cartTable tbody");
        if (!tbody) return;

        tbody.innerHTML = '';

        cartItems.forEach((item, index) => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${item.productName}</td>
                <td>${item.quantity}</td>
                <td>€${item.unitPrice.toFixed(2)}</td>
                <td>€${item.subtotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-danger" onclick="removeCartItem(${index})">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });

        const finalizeSaleBtn = document.getElementById("finalizeSaleBtn");
        if (finalizeSaleBtn) {
            finalizeSaleBtn.disabled = cartItems.length === 0;
        }
    }

    function removeCartItem(index) {
        cartItems.splice(index, 1);
        updateCartTable();
        calculateTotals();
    }

    function clearCart() {
        cartItems = [];
        updateCartTable();
        calculateTotals();
    }

    function clearProductSearch() {
        const barcodeInput = document.getElementById("barcodeInput");
        const productSearchResult = document.getElementById("productSearchResult");
        const addProductBtn = document.getElementById("addProductBtn");

        if (barcodeInput) barcodeInput.value = '';
        if (productSearchResult) productSearchResult.style.display = 'none';
        if (addProductBtn) addProductBtn.disabled = true;
        currentProduct = null;
    }

    function calculateTotals() {
        const subtotal = cartItems.reduce((sum, item) => sum + item.subtotal, 0);
        const discount = document.getElementById("discount");
        const discountValue = discount ? parseFloat(discount.value) || 0 : 0;
        const finalTotal = subtotal - discountValue;

        const totalAmount = document.getElementById("totalAmount");
        const finalTotalInput = document.getElementById("finalTotal");

        if (totalAmount) totalAmount.value = '€' + subtotal.toFixed(2);
        if (finalTotalInput) finalTotalInput.value = '€' + finalTotal.toFixed(2);

        calculateCommission();
    }

    function calculateCommission() {
        const professionalId = document.getElementById("professionalId");
        const finalTotalInput = document.getElementById("finalTotal");
        const commissionAmount = document.getElementById("commissionAmount");
        const commissionPercentage = document.getElementById("commissionPercentage");

        if (!professionalId || !finalTotalInput || !commissionAmount) return;

        const professionalIdValue = professionalId.value;
        const finalTotal = parseFloat(finalTotalInput.value.replace('€', '')) || 0;

        if (professionalIdValue && finalTotal > 0) {
            const professional = professionals.find(p => p.professionalId == professionalIdValue);
            if (professional && professional.commissionPercentage > 0) {
                const commission = finalTotal * (professional.commissionPercentage / 100);
                commissionAmount.value = '€' + commission.toFixed(2);
                if (commissionPercentage) {
                    commissionPercentage.textContent = professional.commissionPercentage + '% de comissão';
                }
            } else {
                commissionAmount.value = '€0.00';
                if (commissionPercentage) {
                    commissionPercentage.textContent = 'Sem comissão';
                }
            }
        } else {
            commissionAmount.value = '€0.00';
            if (commissionPercentage) {
                commissionPercentage.textContent = '';
            }
        }
    }

    async function loadCustomers() {
        try {
            const response = await fetch('/Admin/AdminCashRegister/GetClients');
            if (response.ok) {
                const data = await response.json();
                customers = data;
                
                // Popular select principal
                const select = document.getElementById("customerId");
                if (select) {
                    select.innerHTML = '<option value="">Selecione um cliente</option>';
                    data.forEach(customer => {
                        const option = document.createElement('option');
                        option.value = customer.customerId;
                        option.textContent = customer.name;
                        select.appendChild(option);
                    });
                }

                // Popular select do modal
                const modalSelect = document.getElementById("clientSelect");
                if (modalSelect) {
                    modalSelect.innerHTML = '<option value="">Selecione um cliente...</option>';
                    data.forEach(customer => {
                        const option = document.createElement('option');
                        option.value = customer.customerId;
                        option.textContent = customer.name;
                        modalSelect.appendChild(option);
                    });
                }
            }
        } catch (error) {
            console.error('Erro ao carregar clientes:', error);
        }
    }

    async function loadProfessionals() {
        try {
            const response = await fetch('/Admin/AdminCashRegister/GetProfessionals');
            if (response.ok) {
                const data = await response.json();
                professionals = data;
                
                // Popular select principal
                const select = document.getElementById("professionalId");
                if (select) {
                    select.innerHTML = '<option value="">Selecione um profissional</option>';
                    data.forEach(professional => {
                        const option = document.createElement('option');
                        option.value = professional.professionalId;
                        option.textContent = professional.name;
                        select.appendChild(option);
                    });
                }

                // Popular select do modal
                const modalSelect = document.getElementById("professionalSelect");
                if (modalSelect) {
                    modalSelect.innerHTML = '<option value="">Selecione um profissional...</option>';
                    data.forEach(professional => {
                        const option = document.createElement('option');
                        option.value = professional.professionalId;
                        option.textContent = professional.name;
                        modalSelect.appendChild(option);
                    });
                }
            }
        } catch (error) {
            console.error('Erro ao carregar profissionais:', error);
        }
    }

    async function loadPaymentMethods() {
        try {
            const response = await fetch('/Admin/AdminCashRegister/GetPaymentMethods');
            if (response.ok) {
                const data = await response.json();
                paymentMethods = data;
                const select = document.getElementById("paymentMethodId");
                if (select) {
                    select.innerHTML = '<option value="">Selecione o método</option>';
                    data.forEach(method => {
                        const option = document.createElement('option');
                        option.value = method.paymentMethodId;
                        option.textContent = method.name;
                        select.appendChild(option);
                    });
                }
            }
        } catch (error) {
            console.error('Erro ao carregar métodos de pagamento:', error);
        }
    }

    async function finalizeSale(e) {
        if (e) e.preventDefault();

        if (cartItems.length === 0) {
            alert("Adicione pelo menos um produto");
            return;
        }

        const customerId = document.getElementById("customerId");
        const professionalId = document.getElementById("professionalId");
        const paymentMethodId = document.getElementById("paymentMethodId");
        const discount = document.getElementById("discount");

        const saleData = {
            customerId: customerId ? parseInt(customerId.value) : null,
            professionalId: professionalId ? parseInt(professionalId.value) : null,
            paymentMethodId: paymentMethodId ? parseInt(paymentMethodId.value) : null,
            total: cartItems.reduce((sum, item) => sum + item.subtotal, 0),
            discount: discount ? parseFloat(discount.value) || 0 : 0,
            items: cartItems
        };

        if (confirm(`Finalizar venda? Total: €${(saleData.total - saleData.discount).toFixed(2)}`)) {
            try {
                const response = await fetch('/Admin/AdminCashRegister/FinalizeSale', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(saleData)
                });

                const result = await response.json();
                if (result.success) {
                    alert(result.message);
                    clearCart();
                    const saleForm = document.getElementById("saleForm");
                    if (saleForm) saleForm.reset();
                    const barcodeInput = document.getElementById("barcodeInput");
                    if (barcodeInput) barcodeInput.focus();
                } else {
                    alert(result.message);
                }
            } catch (error) {
                console.error('Erro ao finalizar venda:', error);
                alert("Erro ao finalizar venda");
            }
        }
    }

    // Handlers de Pagamento
    const paymentSelect = document.getElementById("paymentMethod");
    const cashGroup = document.getElementById("cashReceivedGroup");
    const changeRow = document.getElementById("changeRow");
    const txtCashReceived = document.getElementById("txtCashReceived");
    const lblChange = document.getElementById("lblChange");
    const txtTotal = document.getElementById("txtTotal");
    const finalizeBtn = document.getElementById("btnFinalizeSale");

    // Alternar campo de valor recebido
    if (paymentSelect) {
        paymentSelect.addEventListener("change", function () {
            if (this.value === "Dinheiro") {
                if (cashGroup) cashGroup.style.display = "block";
                if (changeRow) changeRow.style.display = "block";
            } else {
                if (cashGroup) cashGroup.style.display = "none";
                if (changeRow) changeRow.style.display = "none";
            }
        });
    }

    // Calcular troco
    if (txtCashReceived) {
        txtCashReceived.addEventListener("input", function () {
            const total = parseFloat(txtTotal?.value || 0) || 0;
            const received = parseFloat(this.value) || 0;
            const change = received - total;
            if (lblChange) {
                lblChange.textContent = change > 0 ? change.toFixed(2) : "0.00";
            }
        });
    }

    // Finalizar Venda
    if (finalizeBtn) {
        finalizeBtn.addEventListener("click", async function () {
            const total = parseFloat(txtTotal?.value || 0) || 0;
            const payment = paymentSelect?.value || "";
            const received = parseFloat(txtCashReceived?.value || 0) || 0;
            const change = received - total;

            if (!payment) {
                alert("Selecione uma forma de pagamento!");
                return;
            }

            if (payment === "Dinheiro" && received < total) {
                alert("Valor recebido deve ser maior ou igual ao total!");
                return;
            }

            const saleData = {
                paymentMethod: payment,
                total: total,
                received: received,
                change: change > 0 ? change : 0,
                items: cartItems
            };

            try {
                const response = await fetch('/Admin/AdminCashRegister/FinalizeSale', {
                    method: 'POST',
                    headers: { 
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(saleData)
                });

                if (response.ok) {
                    const result = await response.json();
                    alert(`Venda finalizada com sucesso!\n\nRecibo nº: ${result.receiptId}`);
                    window.location.reload();
                } else {
                    alert("Erro ao finalizar a venda!");
                }
            } catch (error) {
                console.error('Erro ao finalizar venda:', error);
                alert("Erro ao finalizar a venda!");
            }
        });
    }

    // Atualizar total quando itens mudam
    function updateTotalDisplay() {
        const total = cartItems.reduce((sum, item) => sum + item.subtotal, 0);
        if (txtTotal) {
            txtTotal.value = total.toFixed(2);
        }
    }

    // Sobrescrever função calculateTotals para incluir atualização do display
    const originalCalculateTotals = calculateTotals;
    calculateTotals = function() {
        originalCalculateTotals();
        updateTotalDisplay();
    };

    // Tornar funções globais para uso em onclick
    window.removeCartItem = removeCartItem;
});
