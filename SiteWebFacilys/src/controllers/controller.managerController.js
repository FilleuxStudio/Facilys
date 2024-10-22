const Product = require("../models/product.model");

// Ajouter un produit
exports.managerAddProduct = async (req, res) => {
    const { title, subtitle, price, oldPrice, advice, type, colorBagde } = req.body;
    try {

        var logoDataUrl = ""
    if(req.files != null){
      if (req.files || req.files.pictureProduct) {
        logoDataUrl = await convertToBase64Html(req.files.pictureProduct);
      }
    }

    console.log(req.body);
        // Créer une instance de produit
        const product = new Product({
            title: title,
            subtitle: subtitle,
            picture: logoDataUrl,
            price: price,
            oldPrice: oldPrice,
            advice: advice,
            type: type,
            colorBagde: colorBagde,
        });

        await product.save();

        res.status(201).send('success');
    } catch (error) {
        console.error('Erreur lors de l\'ajout du produit', error);
        res.status(500).send('Erreur lors de l\'ajout du produit');
    }
};

exports.managerGetProduct = async (req, res) => {
    try {

        const productData = await Product.findById(req.body.id);
        console.log(productData);
    if (!productData) {
        return res.status(404).json({ error: "produit non trouvé" });
      }
  
      res.json(productData);
    } catch (error) {
        console.error('Erreur lors de la récupération du produit', error);
        res.status(500).send('Erreur lors de la récupération du produit');
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