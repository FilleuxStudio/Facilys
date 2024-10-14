const express = require("express");
const router = express.Router();
const nodemailer = require("nodemailer");
const smtpConfig = require("../config/smtp-server");
const authController = require("../controllers/controller.authController");
const accountController = require("../controllers/controller.accountController");
const managerController = require("../controllers/controller.managerController");

const now = new Date().getFullYear();
////// Page Website //////
//Route pour home 
router.get('/', async (req, res) => {
    const now =  new Date().getFullYear();
    const productShop = await managerController.managerGetProduct();
    console.log(productShop);
    res.render('home', { currentDateTime: now, title: 'Accueil', products: productShop})
  });
  
// Route pour la page "À propos de nous"
router.get("/about", (req, res) => {
  res.render("about", { currentDateTime: now, title: "À propos de Facilys" }); // Affiche la page about.ejs
});

// Route pour la page "contact"
router.get("/contact", (req, res) => {
  res.render("contact", {
    currentDateTime: now,
    title: "Contact",
    message: null,
  });
});

router.post("/contact-support", (req, res) => {
  const transporter = nodemailer.createTransport({
    host: smtpConfig.host, // Remplacez par l'hôte SMTP de PlanetHoster
    port: smtpConfig.port, // Ou 465 si vous utilisez SSL
    secure: smtpConfig.secure, // false pour STARTTLS (recommandé pour le port 587)
    auth: smtpConfig.auth,
    tls: smtpConfig.tls,
  });

  const mailOptions = {
    from: "votre-adresse@votre-domaine.com", // Adresse de l'expéditeur
    to: "test@gmail.com", // Adresse de l'utilisateur
    subject: req.body.subject,
    text: req.body.message + "\n" + req.body.company,
  };

  // Envoyer l'email
  transporter.sendMail(mailOptions, (error, info) => {
    if (error) {
      console.log(error);
      //return res.status(500).send('Erreur lors de l\'envoi de l\'email');
      res.render("contact", {
        currentDateTime: now,
        title: "Contact",
        message: "Erreur",
      });
    }
    console.log("Email envoyé: " + info);
    res.render("contact", {
      currentDateTime: now,
      title: "Contact",
      message: "Votre message a été correctement envoyé",
    });
  });
});

// Route pour la page "privacy-policy"
router.get("/privacy-policy", (req, res) => {
  res.render("privacy-policy", {
    currentDateTime: now,
    title: "Politique de confidentialité",
  });
});

// Route pour la page login et register
router.get("/login", (req, res) => {
  const message = req.query.message || null;
  res.render("login", {
    currentDateTime: now,
    message: message,
    title: "Connexion",
  });
});

// Route pour le formulaire d'inscription
router.post("/register", authController.register);

////// Page Dashboard //////

// Route pour le formulaire de connexion
router.post("/login", authController.login);
router.get("/logout", authController.logout);

router.get("/account", accountController.account);
router.post("/accountDetails", accountController.accountDetails);
router.post("/accountUpdate", accountController.accoutUpdate);

// Route pour le manager
router.post("/managerAddProduct", managerController.managerAddProduct);
//// Exemple de route protégée
//router.get('/home', authenticate, (req, res) => {
//    res.render('home', { user: req.user });
//  });

module.exports = router;