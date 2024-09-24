const jwt = require('jsonwebtoken');
const cookieConfig = require('../config/cookie-config');

function authenticate(req, res, next) {
  const token = req.cookies.authToken; // Récupérer le token du cookie

  if (!token) {
    return res.redirect('/login'); // Pas de token, rediriger vers la page de connexion
  }

  // Vérifier et décoder le token
  jwt.verify(token, cookieConfig.cookieSecret, (err, decoded) => {
    if (err) {
      return res.redirect('/login'); // Si le token est invalide, rediriger vers la connexion
    }

    // Ajoute les informations de l'utilisateur au requête
    req.user = { id: decoded.id };
    next(); // Passe à la prochaine middleware ou route
  });
}

module.exports = authenticate;
