const express = require("express");
const User = require("../models/user.model");
const router = express.Router();

router.post('/version', (req, res) => {
        res.send('version 1');

});

router.post('/login', async (req, res) => {
        console.log(req.body.email);
        const userData = await User.findByEmail(req.body.email);
        console.log(userData);
        if (!userData) {
                return res.status(404).json({
                  success: false,
                  message: 'Utilisateur non trouvé',
                  data: null
                });
              }
              
              res.status(200).json({
                success: true,
                message: 'Utilisateur trouvé avec succès',
                data: userData
              });
});

module.exports = router;