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

        res.status(201).send('Ajouté avec succès.');
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

exports.managerGetAllProducts = async (req, res) => {
  try {
     return await Product.findAll();
  } catch (error) {
      console.error('Erreur lors de la récupération du produit', error);
      res.status(500).send('Erreur lors de la récupération du produit');
  }
};

exports.managerEditProducts = async (req, res) => {
  const { idProduct, titleEdit, subtitleEdit, priceEdit, oldPriceEdit, adviceEdit, typeEdit, colorBagdeEdit } = req.body;
  try {
    let logoDataUrl = "";
    
    // Si un fichier image est fourni, le convertir en base64
    if (req.files && req.files.pictureProductEdit) {
      logoDataUrl = await convertToBase64Html(req.files.pictureProductEdit);
    }

    // Préparer les données de mise à jour
    const updateData = {
      title: titleEdit,
      subtitle: subtitleEdit,
      price: priceEdit,
      oldPrice: oldPriceEdit,
      advice: adviceEdit,
      type: typeEdit,
      colorBagde: colorBagdeEdit,
    };

    // Si une nouvelle image est fournie, inclure son URL
    if (logoDataUrl) {
      updateData.picture = logoDataUrl;
    }

    // Mettre à jour le produit dans Firestore
    const updateResult = await Product.update(idProduct, updateData);
    
    if (updateResult) {
      return res.status(200).send('Produit modifié avec succès.');
    } else {
      return res.status(403).send('Erreur lors de la modification du produit.');
    }
  } catch (error) {
    console.error('Erreur lors de la modification du produit', error);
    res.status(500).send('Erreur lors de la modification du produit');
  }
};

exports.managerDeleteProducts = async (req, res) => {
  try{
    var deleteResult  = await Product.delete(req.body.idProductDelete);
    if(deleteResult  === true){
      return res.status(200).send("success");
    }else{
      return res.status(403).send("error");
    }
  }catch(error){
    console.error('Erreur lors de la suppression du produit', error);
      res.status(500).send('Erreur lors de la suppression du produit');
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