const User = require("../models/user.model");
const Order = require("../models/order.model");
const Product = require("../models/product.model");

exports.paymentProduct = async (req, res) => {
    
  try {
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

    res.status(201).render('payment-success');
  } catch (error) {
    console.error("Erreur lors de l'ajout du produit", error);
    res.status(500).send("Erreur lors de l'ajout du produit");
  }
};
