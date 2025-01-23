const cookieConfig = require("../config/cookie-config");
const jwt = require("jsonwebtoken");
const argon2 = require("@node-rs/argon2");
const Product = require("../models/product.model");
const Order = require("../models/order.model");

exports.shoppingGetAllProducts = async (req, res) => {
    try {
      return await Product.findAll();
    } catch (error) {
      console.error("Erreur lors de la récupération du produit", error);
      res.status(500).send("Erreur lors de la récupération du produit");
    }
  };

  exports.shoppingGetProduct = async (req, res) => {
    try {
      const product = await Product.findById(req.query.id);
      if (!product) {
        return res.status(404).json({ message: "Produit non trouvé" });
      }
      return product;
    } catch (error) {
      console.error("Erreur lors de la récupération du produit", error);
      return res.status(500).json({ message: "Erreur lors de la récupération du produit" });
    }
  };

  exports.shoppingPaymentProduct = async(req, res) => {
    try {
      console.log(req.body.idProduct);
      const product = await Product.findById(req.body.idProduct);
      var renewalDate = new Date();
      switch (product.type) {
        case 'Monthly':
          renewalDate.setMonth(renewalDate.getMonth() + 1);
          break;
        case 'annually':
          renewalDate.setFullYear(renewalDate.getFullYear() + 1);
          break;
        case 'Unlimited':
          renewalDate.setFullYear(renewalDate.getFullYear() + 10);
          break;
        case 'Free':
          renewalDate.setFullYear(renewalDate.getFullYear() + 10);
          break;
        case 'FreeLimited':
          renewalDate.setFullYear(renewalDate.getFullYear() + 10);
          break;
        default:
          break;
      }
          
      // Convertir renewalDate en format ISO string
      renewalDate = renewalDate.toISOString(); 
      const order = new Order({
        idProduct: product.id,
        idUser: req.session.user.email,
        productName: product.title,
        price: product.price,
        renewalDate: renewalDate,
        type: product.type,
      });
  
      await order.save();
  
      res.status(201).redirect('payment-success');
    } catch (error) {
      console.error("Erreur lors de l'ajout du produit", error);
      res.status(500).send("Erreur lors de l'ajout du produit");
    }
  };