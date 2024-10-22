document.addEventListener('DOMContentLoaded', function() {
    const accountDetailTab = document.getElementById('account-detail-tab');
    const formUserInformation = document.getElementById('formUserInformation');
    const inputLogo = document.getElementById('filelogo');
    const imgView = document.getElementById('imglogo');
    const email = document.getElementById('email');
  
    accountDetailTab.addEventListener('click', fetchAccountDetails);
    formUserInformation.addEventListener('submit', updateAccount);
    inputLogo.addEventListener('change', previewLogo);
  
    function fetchAccountDetails() {
      fetch('/accountDetails', {
        method: 'POST',
        headers: {
          'CSRF-Token': document.querySelector('meta[name="csrf-token"]').getAttribute('content'),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email: email.value}),
      })
      .then(response => {
        if (!response.ok) throw new Error('Erreur réseau');
        return response.json();
      })
      .then(data => {
        if (data) {
          Object.keys(data).forEach(key => {
            const input = document.getElementById(key);
            if (input) input.value = data[key] || '';
          });
          if (data.logo) imgView.src = `${data.logo}`;
        }
      })
      .catch(error => console.error('Erreur:', error));
    }
  
    function updateAccount(event) {
      event.preventDefault();
      const formData = new FormData(formUserInformation);
  
      fetch('/accountUpdate', {
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
        alert('Une erreur est survenue lors de la mise à jour du compte');
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
  