const User = require('../models/user.model'); 
const jwt = require('jsonwebtoken');
const cookieConfig = require('../config/cookie-config');

exports.account = async (req, res) => {
const now =  new Date().getFullYear();
  try {
    // Vérifier si le cookie authToken existe
    const token = req.cookies.authToken;

    if (!token) {
      // Si le cookie n'existe pas, rediriger vers la page de connexion
      return res.redirect('/login');
    }

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);
      
      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect('/login');
      }

      res.render('account', { user: req.session.user, currentDateTime: now,  csrfToken: req.session.csrfToken, title: 'Mon compte' });

    } catch (error) {
      // Si le token est invalide ou expiré
      console.error('Token invalide ou expiré:', error);
      res.clearCookie('authToken');
      return res.redirect('/login');
    }

  } catch (err) {
    console.error('Erreur lors de l\'accès au compte :', err);
    res.status(500).send('Erreur serveur');
  }
};

exports.accountDetails = async(req, res) => {
    try {
      const { email } = req.body;
  
      if (!email) {
        return res.status(400).json({ error: 'Email requis' });
      }
  
      const userData = await User.findByEmail(email);
  
      if (!userData) {
        return res.status(404).json({ error: 'Utilisateur non trouvé' });
      }
  
      res.json(userData);
  
    } catch (err) {
      console.error('Erreur lors de la récupération des informations du compte:', err);
      res.status(500).json({ error: 'Erreur serveur' });
    }
  };

  exports.accoutUpdate = async(req, res) => {
    try {
      
  
    } catch (err) {
      console.error('Erreur lors de la récupération des informations du compte:', err);
      res.status(500).json({ error: 'Erreur serveur' });
    }
  };