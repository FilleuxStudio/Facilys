const express = require("express");
const router = express.Router();
const authenticationController = require("../controllers/controller.authentication");
const accountController = require("../controllers/controller.account");
const managerController = require("../controllers/controller.manager")
const shoppingController = require("../controllers/controller.shopping");

const now = new Date().getFullYear();

router.get("/", async (req, res) => {
  const productShop = await shoppingController.shoppingGetAllProducts();
  res.render("home", { currentDateTime: now, currentRoute: req.path, products: productShop, session: req.session });
  //res.render('home', { currentDateTime: now, title: 'Accueil', products: productShop})
});

// Route pour la page "À propos de nous"
router.get("/about", async (req, res) => {
  res.render("about", { currentDateTime: now, currentRoute: req.path, session: req.session}); // Affiche la page about.ejs
});

// Route pour la page "contact"
router.get("/contact", (req, res) => {
  res.render("contact", { currentDateTime: now, currentRoute: req.path, session: req.session });
});

// Route pour la page "/term-of-use"
router.get("/term-of-use", (req, res) => {
  res.render("term-of-use", { currentDateTime: now, currentRoute: req.path, session: req.session});
});

// Route pour la page voir le panier
router.get("/shop-checkout", async (req, res) => {
  const csrfToken = req.csrfToken();
  const message = req.query.message || null;
  const product = await shoppingController.shoppingGetProduct(req);
  res.render("check-out", { currentDateTime: now, currentRoute: req.path, product: product, message: message, session: req.session, csrfToken });
});

router.post("/check-out-payment", shoppingController.shoppingPaymentProduct);

// Route pour la page voir plus de details du produit
router.get("/shop-detail", async (req, res) => {
  const product = await shoppingController.shoppingGetProduct(req);
  res.render("product-details", { currentDateTime: now, currentRoute: req.path, product: product, session: req.session });
});

// Route pour la page voir plus de details du produit
router.get("/check-out-payment", async (req, res) => {
  res.render("payment-success", { currentDateTime: now, currentRoute: req.path, session: req.session });
});

// Route pour la page login et register
router.get("/login", (req, res) => {
  const csrfToken = req.csrfToken(); // Générer le token CSRF
  const message = req.query.message || null;
  res.render("login", { currentDateTime: now, currentRoute: req.path, message: message, session: req.session, csrfToken });
});

router.post("/login", authenticationController.login);

router.post("/login", authenticationController.login);

// Route pour la page login et register
router.get("/register", (req, res) => {
  const csrfToken = req.csrfToken(true);
  const message = req.query.message || null;
  res.render("register", { currentDateTime: now, currentRoute: req.path, message: message, session: req.session, csrfToken });
});

router.post("/register", authenticationController.register);

router.get("/logout", authenticationController.logout);

router.get("/account", accountController.account);

router.get("/account-orders", accountController.accountOrders);

router.get("/account-invoices", accountController.accountInvoices);

router.get("/account-teams", accountController.accountTeams);

router.get("/account-deleteteam/:id", accountController.accountDeleteMember);

router.get("/manager-product", managerController.managerProducts);

router.get("/manager-clients", managerController.managerClients);

router.get("/account-download", accountController.accountDownload)

/// Compte
router.post("/update-account", accountController.accoutUpdate);

router.post("/account-addteam", accountController.accountAddTeam);

/// Produit
router.post("/manager-addproduct", managerController.managerAddProduct);

router.post("/manager-getproduct", managerController.managerGetProduct);

router.post("/manager-editproduct", managerController.managerEditProduct);

router.get("/manager-deleteproduct/:id", managerController.managerDeleteProduct);
/// Client

module.exports = router;
