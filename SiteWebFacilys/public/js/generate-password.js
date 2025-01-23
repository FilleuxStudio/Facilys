function generatePassword() {
    const charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!+-*$#@";
    let password = "";
    for (let i = 0; i < 8; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    document.getElementById("tpassword").value = password;
    alert("Mot de passe généré : " + password);
  }
  
  function togglePasswordVisibility() {
    const passwordInput = document.getElementById("tpassword");
    const toggleButton = document.getElementById("togglePassword");
    
    if (passwordInput.type === "password") {
      passwordInput.type = "text";
      toggleButton.textContent = "Masquer";
    } else {
      passwordInput.type = "password";
      toggleButton.textContent = "Afficher";
    }
  }
  
  document.getElementById("generatePassword").addEventListener("click", generatePassword);
  document.getElementById("togglePassword").addEventListener("click", togglePasswordVisibility);