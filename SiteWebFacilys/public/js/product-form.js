document.addEventListener('DOMContentLoaded', function() {
    const editProductModal = document.getElementById('EditProductModal');
    const csrfToken = document.getElementById('_csrf').value;

    editProductModal.addEventListener('show.bs.modal', async function (event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-id');

        try {
            const response = await fetch('/manager-getproduct', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Token': csrfToken
                },
                body: JSON.stringify({ id: productId })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const product = await response.json();

            document.getElementById('editProductId').value = product.id;
            document.getElementById('editProductTitle').value = product.title;
            document.getElementById('editProductSubtitle').value = product.subtitle;
            document.getElementById('editProductPrice').value = product.price;
            document.getElementById('editProductOldPrice').value = product.oldPrice;
            // Sélectionner l'option correspondante dans le select
            const typeSelect = document.getElementById('editProductType');
            for(let i = 0; i < typeSelect.options.length; i++) {
                if(typeSelect.options[i].value === product.type) {
                    typeSelect.selectedIndex = i;
                    break;
                }
            }
            document.getElementById('editProductAdvice').value = product.advice;
            const colorInput = document.getElementById('editProductColorBadge');
            if (product.colorBadge && product.colorBadge.startsWith('#')) {
                colorInput.value = product.colorBadge;
            } else {
                console.warn('La valeur de colorBadge n\'est pas au format hexadécimal:', product.colorBadge);
                // Vous pouvez définir une couleur par défaut ici si nécessaire
                colorInput.value = '#000000';
            }
        } catch (error) {
            console.error('Erreur lors de la récupération du produit:', error);
            alert('Erreur lors de la récupération du produit. Veuillez réessayer.');
        }
    });
});
