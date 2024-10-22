document.addEventListener('DOMContentLoaded', function() {
    const formAddProdcut = document.getElementById('formAddProdcut');
    const formEditProdcut = document.getElementById('formEditProdcut');
    const ModelEditProduct = document.querySelectorAll('[OpenModelEdit]');
    const ModelDeleteProduct = document.querySelectorAll('[OpenModelDelete]');
  
    formAddProdcut.addEventListener('submit', addProduct);
    formEditProdcut.addEventListener('submit', editProduct);
    ModelEditProduct.forEach(element => {
      element.addEventListener('click', fetchProduct);
    });
    ModelDeleteProduct.forEach(element => {
      element.addEventListener('click', ConfirmDeleteProduct);
    });

    function addProduct(event) {
      event.preventDefault();
      const formData = new FormData(formAddProdcut);
  
      fetch('/managerAddProduct', {
        method: 'POST',
        body: formData,
        headers: {
          'CSRF-Token': document.querySelector('meta[name="csrf-token"]').getAttribute('content'),
        }
      })
      .then(response => {
        if (!response.ok) throw new Error('Erreur réseau');
        return response.json();
      })
      .then(data => {
        alert(data.message);
      })
      .catch(error => {
        console.error('Erreur:', error);
        alert('Une erreur est survenue lors de l\'ajoute du produit.');
      });
    }

    function editProduct(event){

    }

    function fetchProduct(event){
      const productId = event.currentTarget.getAttribute('data-id');
      fetch('/managerGetProduct', {
        method: 'POST',
        headers: {
          'CSRF-Token': document.querySelector('meta[name="csrf-token"]').getAttribute('content'),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ id: productId}),
      })
      .then(response => {
        if (!response.ok) throw new Error('Erreur réseau');
        return response.json();
      })
      .then(data => {
        if (data) {
          document.getElementById('titleEdit').value = data.title
          document.getElementById('subtitleEdit').value = data.subtitle
          document.getElementById('priceEdit').value = data.price
          document.getElementById('oldPriceEdit').value = data.oldPrice
          document.getElementById('adviceEdit').value = data.advice
          const selectElement = document.getElementById('typeEdit');
          // Parcourir toutes les options pour trouver celle qui correspond
          for (let i = 0; i < selectElement.options.length; i++) {
            if (selectElement.options[i].value === data.type) {
            selectElement.selectedIndex = i;
            break;
           }
           document.getElementById('colorBagdeEdit').value = data.colorBagde
          }
        }
      })
      .catch(error => console.error('Erreur:', error));
    }

    function ConfirmDeleteProduct(event){
    }
  
    function previewLogo(event) {
      const file = event.target.files[0];
      if (file && file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = e => imgView.src = e.target.result;
        reader.readAsDataURL(file);
      } else {
        alert('Veuillez sélectionner un fichier image valide.');
        event.target.value = '';
      }
    }
  });
  