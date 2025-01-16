const cookieConfig = require('../config/cookie-config');
const jwt = require('jsonwebtoken');
const argon2 = require('@node-rs/argon2');
const User = require('../models/user.model');
const Product = require("../models/product.model");
const Order = require("../models/order.model");
const Team = require("../models/team.model");

const now = new Date().getFullYear();

exports.account = async (req, res) => {
    try {
      // Vérifier si le cookie authToken existe
        var token = await CheckTokenConnection(req);
  
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
        const userData = await User.findByEmail(req.session.user.email);
        OrderList = await Order.findByUserId(req.session.user.email);
        TeamList = await Team.findByUserId(req.session.user.email);

        const csrfToken = req.csrfToken(true);
        const message = req.query.message || null;
        res.render("account", {user: userData, currentDateTime: now, currentRoute: req.path, message: message, Orders: OrderList, Products: productList, Teams: TeamList, Clients: clientList, csrfToken});
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
  
      res.status(200).redirect("/account?message=Vos informations ont été mises à jour avec succès.");
    } catch (err) {
      console.error(
        "Erreur lors de la récupération des informations du compte:",
        err
      );
      res.status(500).json({ error: "Erreur serveur" });
    }
  };


  async function CheckTokenConnection(req){
    var token = req.cookies.authToken;
    if (!token) {
      // Si le cookie n'existe pas, rediriger vers la page de connexion
      return res.redirect("/login");
    }else{
        return token;
    }
  }

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