const jwt = require('jsonwebtoken');
const User = require('../models/user.model'); 
const cookieConfig = require('../config/cookie-config');
const bcrypt = require('bcrypt');
const argon2 = require('argon2');

exports.login = async (req, res) => {
  const { email, password, rememberMe } = req.body;

  try {
    // Rechercher l'utilisateur dans la base de données
    const user = await User.findByEmail(email);

    if (!user) {
      return res.status(400).redirect('/login?message=Email ou mot de passe incorrect.');
    }

    // Comparer le mot de passe
    const match = await argon2.verify(user.password, password);

    if (!match) {
      return res.status(400).redirect('/login?message=Email ou mot de passe incorrect.');
    }

    // Générer un token JWT
    const token = jwt.sign({ id: user.email }, cookieConfig.cookieSecret, {
      expiresIn: rememberMe ? '7d' : '1h' // Token valide 1 heure ou 7 jours
    });

     // Stocker les informations de l'utilisateur dans la session
     req.session.user = {
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      manager: user.manager,
    };

    // Définir la durée du cookie selon "Remember me"
    const cookieDuration = rememberMe ? cookieConfig.rememberMeDuration : cookieConfig.sessionDuration;

    // Configurer les options du cookie
    res.cookie('authToken', token, {
      ...cookieConfig.cookieOptions,
      maxAge: cookieDuration // Durée de vie du cookie
    });

    res.redirect('/account');
  } catch (err) {
    console.error('Erreur lors de la connexion :', err);
    res.status(500).send('Erreur serveur');
  }
};

exports.logout = (req, res) => {
  res.clearCookie('authToken'); // Supprime le cookie d'authentification
  res.redirect('/login'); // Rediriger vers la page de connexion
};

// Contrôleur d'inscription
exports.register = async (req, res) => {
    const { companyName, firstName, lastName, email, password, passwordControl } = req.body;
    const checkTerm = transformCheckboxValue(req.body.checkTerm);
    try {
      // Hachage du mot de passe avec Argon2
      var hashedPassword = await argon2.hash(password);
        // Créer une instance de l'utilisateur
        const user = new User({
            companyName,
            logo: "null",
            firstName,
            lastName,
            email,
            siret: "null",
            addressclient: "null",
            phone: "null",
            password: hashedPassword, // Mot de passe haché
            manager: false, // Par défaut, false, peut être ajusté selon vos besoins
        });

        // Sauvegarder l'utilisateur dans Firestore
        if(checkTerm == true){
          await user.save();
        }

        res.status(201).send('Utilisateur créé avec succès');
    } catch (error) {
        console.error('Erreur lors de la création de l\'utilisateur:', error);
        res.status(500).send('Erreur lors de la création de l\'utilisateur');
    }
};

function transformCheckboxValue(value) {
  if (value === 'on' || value === true) {
      return true;
  }
  return false;
}