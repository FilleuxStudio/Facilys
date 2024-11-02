document.addEventListener('DOMContentLoaded', function() {
    const formAddProdcut = document.getElementById('formAddProdcut');
    const formEditProdcut = document.getElementById('formEditProdcut');
    const formDeleteProdcut = document.getElementById('formDeleteProdcut');
    const ModelEditProduct = document.querySelectorAll('[OpenModelEdit]');
    const ModelDeleteProduct = document.querySelectorAll('[OpenModelDelete]');
  
    formAddProdcut.addEventListener('submit', addProduct);
    formEditProdcut.addEventListener('submit', editProduct);
    formDeleteProdcut.addEventListener('submit', deleteProduct);

    ModelEditProduct.forEach(element => {
      element.addEventListener('click', fetchProduct);
    });
    ModelDeleteProduct.forEach(element => {
      element.addEventListener('click', fetchProduct);
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
        return response.text(); // Utilisation de .text() pour récupérer le message de réponse en texte brut
    })
    .then(data => {
        alert(data); // Affiche le message "success" du serveur
        window.location.reload();
    })
    .catch(error => {
        console.error('Erreur:', error);
        alert('Une erreur est survenue lors de l\'ajout du produit.');
    });
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
          if( window.getComputedStyle(document.getElementById('EditProdcutModal')).display === 'none'){
            document.getElementById('idProduct').value = data.id
          }else{
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
            }
            document.getElementById('colorBagdeEdit').value = data.colorBagde;
          }
        }
      })
      .catch(error => console.error('Erreur:', error));
    }

    function editProduct(event){
      event.preventDefault();
      const formData = new FormData(formDeleteProdcut);

      fetch('/managerEditProduct', {
        method: 'POST',
        body: formData,
        headers: {
          'CSRF-Token': document.querySelector('meta[name="csrf-token"]').getAttribute('content'),
        }
      })
      .then(response => {
        console.log(response)
        if (!response.ok) throw new Error('Erreur réseau');
        return response.text(); // Utilisation de .text() pour récupérer le message de réponse en texte brut
    })
    .then(data => {
        alert(data); // Affiche le message "success" du serveur
        window.location.reload();
    })
    .catch(error => {
        console.error('Erreur:', error);
        alert('Une erreur est survenue lors de la modification du produit.');
    });
    }

    function deleteProduct(event){
      event.preventDefault();
      const formData = new FormData(formDeleteProdcut);

      fetch('/managerDeleteProduct', {
        method: 'POST',
        body: formData,
        headers: {
          'CSRF-Token': document.querySelector('meta[name="csrf-token"]').getAttribute('content'),
        }
      })
      .then(response => {
        console.log(response)
        if (!response.ok) throw new Error('Erreur réseau');
        return response.text(); // Utilisation de .text() pour récupérer le message de réponse en texte brut
    })
    .then(data => {
        alert(data); // Affiche le message "success" du serveur
        window.location.reload();
    })
    .catch(error => {
        console.error('Erreur:', error);
        alert('Une erreur est survenue lors de la suppression du produit.');
    });
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
  