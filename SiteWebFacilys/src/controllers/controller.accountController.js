const User = require("../models/user.model");
const Product = require("../models/product.model");
const Order = require("../models/order.model");
const Team = require("../models/team.model");
const jwt = require("jsonwebtoken");
const cookieConfig = require("../config/cookie-config");
const argon2 = require('argon2');

exports.account = async (req, res) => {
  const now = new Date().getFullYear();
  try {
    // Vérifier si le cookie authToken existe
    var token = req.cookies.authToken;
    if (!token) {
      // Si le cookie n'existe pas, rediriger vers la page de connexion
      return res.redirect("/login");
    }

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var productList = [];
      var clientList = [];
      var OrderList = [];
      var TeamList = [];

      if(req.session.user.manager == true){
        productList =  await Product.findAll();
        clientList = await User.findAll();
      }
      OrderList = await Order.findByUserId(req.session.user.email);
      TeamList = await Team.findByUserId(req.session.user.email);

      res.render("account", {user: req.session.user, currentDateTime: now, title: "Mon compte", Orders: OrderList, Products: productList, Teams: TeamList, Clients: clientList});
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès au compte :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.accountDetails = async (req, res) => {
  try {
    const { email } = req.body;

    if (!email) {
      return res.status(400).json({ error: "Email requis" });
    }

    const userData = await User.findByEmail(email);

    if (!userData) {
      return res.status(404).json({ error: "Utilisateur non trouvé" });
    }

    res.json(userData);
  } catch (err) {
    console.error(
      "Erreur lors de la récupération des informations du compte:",
      err
    );
    res.status(500).json({ error: "Erreur serveur" });
  }
};

exports.accoutUpdate = async (req, res) => {

  try {
    var logoDataUrl = ""
    if(req.files != null){
      if (req.files || req.files.filelogo) {
        logoDataUrl = await convertToBase64Html(req.files.filelogo);
      }
    }else{
      logoDataUrl = req.body.imagelogo;
    }
    
    await User.update(req.body.email, {
      companyName: req.body.companyName,
      logo: logoDataUrl,
      firstName: req.body.firstName,
      lastName: req.body.lastName,
      email: req.body.email,
      siret: req.body.siret,
      address: req.body.addressclient,
      phone: req.body.phone,
    });

    res.status(200);
  } catch (err) {
    console.error(
      "Erreur lors de la récupération des informations du compte:",
      err
    );
    res.status(500).json({ error: "Erreur serveur" });
  }
};

exports.accountAddTeam = async (req, res) => {
  const { tlname, tfname, tpassword, ttype, temail, emailmanager } = req.body;
  try {

    var userManager = await User.findByEmail(emailmanager);
      // Créer une instance de produit
      const team = new Team({
          fname: tfname,
          lname: tlname,
          type: ttype,
          tpassword:  await argon2.hash(tpassword),
          team: userManager.companyName,
          manager: userManager.email,
          email: temail,
      });

      await team.save();

      res.status(201).send('Ajouté avec succès.');
  } catch (error) {
      console.error('Erreur lors de l\'ajout du produit', error);
      res.status(500).send('Erreur lors de l\'ajout du produit');
  }
};

async function convertToBase64Html(file) {
  if (!file) {
    throw new Error("Aucun fichier n'a été fourni");
  }
  // Convertir le buffer du fichier en base64
  var base64 = file.data.toString("base64");
  // Déterminer le type MIME du fichier
  var mimeType = file.mimetype;
  // Créer la chaîne data URL
  return `data:${mimeType};base64,${base64}`;

}