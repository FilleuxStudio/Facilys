const express = require("express");
const router = express.Router();
const nodemailer = require("nodemailer");
const smtpConfig = require("../config/smtp-server");
const authController = require("../controllers/controller.authController");
const accountController = require("../controllers/controller.accountController");
const managerController = require("../controllers/controller.managerController");
const paymentController = require("../controllers/controller.paymentController");

const now = new Date().getFullYear();
////// Page Website //////
//Route pour home 
router.get('/', async (req, res) => {
    const now =  new Date().getFullYear();
    const productShop = await managerController.managerGetAllProducts();
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

router.get("/shop-checkout", async (req, res) => {
  const productId = req.query.id;

  if(!productId){
    return res.status(400).redirect("/");
  }

  req.body = { id: productId };

  try {
      const productShop = await new Promise((resolve, reject) => {
          managerController.managerGetProduct(req, {
              json: resolve,
              status: (code) => ({
                  json: (error) => reject({ code, error })
              }),
              send: (error) => reject({ code: 500, error })
          });
      });

      // Check if the user is logged in
     if (!req.session.user) {
          return res.redirect("/login");
      }

      // Calculate the result
      let result = parseFloat(productShop.price) + (parseFloat(productShop.price) * 15 / 100);

      res.render("shop-checkout", {
          currentDateTime: now,
          title: "Commandes",
          Product: productShop,
          Result: result
      });
  } catch (error) {
      console.error('Erreur lors de la récupération du produit:', error.error || error);

      // Redirect to the home page or display an error
      res.status(error.code || 500).redirect("/");
  }
})

router.post("/payment", paymentController.paymentProduct);

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
router.post("/accountAddTeam", accountController.accountAddTeam);

// Route pour le manager
router.post("/managerAddProduct", managerController.managerAddProduct);
router.post("/managerGetProduct", managerController.managerGetProduct);
router.post("/managerEditProduct", managerController.managerEditProducts);
router.post("/managerDeleteProduct", managerController.managerDeleteProducts);
//// Exemple de route protégée
//router.get('/home', authenticate, (req, res) => {
//    res.render('home', { user: req.user });
//  });

module.exports = router;